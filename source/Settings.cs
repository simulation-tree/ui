using Automations;
using Cameras;
using Cameras.Components;
using Data;
using DefaultPresentationAssets;
using Fonts;
using InteractionKit.Components;
using Meshes;
using Rendering;
using Rendering.Components;
using System;
using System.Diagnostics;
using System.Numerics;
using Textures;
using Transforms.Components;
using Unmanaged;
using Worlds;

namespace InteractionKit
{
    public readonly struct Settings : IEntity
    {
        public const float ZScale = 0.2f;
        public const char ShiftCharacter = (char)14;
        public const char MoveLeftCharacter = (char)17;
        public const char MoveRightCharacter = (char)18;
        public const char MoveUpCharacter = (char)19;
        public const char MoveDownCharacter = (char)20;
        public const char StartOfTextCharacter = (char)2;
        public const char EndOfTextCharacter = (char)3;
        public const char ControlCharacter = (char)29;
        public const char EscapeCharacter = (char)27;

        private readonly Entity entity;

        public readonly USpan<char> PressedCharacters => entity.GetArray<TextCharacter>().As<char>();
        public readonly ref float SingleLineHeight => ref entity.GetComponent<UISettings>().singleLineHeight;

        public readonly StateMachine ControlStateMachine
        {
            get
            {
                ref AssetReferences component = ref entity.GetComponent<AssetReferences>();
                rint stateMachineReference = component.stateMachineReference;
                uint stateMachineEntity = entity.GetReference(stateMachineReference);
                return new Entity(entity.GetWorld(), stateMachineEntity).As<StateMachine>();
            }
        }

        public readonly Automation<Vector4> IdleAutomation
        {
            get
            {
                ref AssetReferences component = ref entity.GetComponent<AssetReferences>();
                rint idleAutomationReference = component.idleAutomationReference;
                uint idleAutomationEntity = entity.GetReference(idleAutomationReference);
                return new Entity(entity.GetWorld(), idleAutomationEntity).As<Automation<Vector4>>();
            }
        }

        public readonly Automation<Vector4> SelectedAutomation
        {
            get
            {
                ref AssetReferences component = ref entity.GetComponent<AssetReferences>();
                rint selectedAutomationReference = component.selectedAutomationReference;
                uint selectedAutomationEntity = entity.GetReference(selectedAutomationReference);
                return new Entity(entity.GetWorld(), selectedAutomationEntity).As<Automation<Vector4>>();
            }
        }

        public readonly Automation<Vector4> PressedAutomation
        {
            get
            {
                ref AssetReferences component = ref entity.GetComponent<AssetReferences>();
                rint pressedAutomationReference = component.pressedAutomationReference;
                uint pressedAutomationEntity = entity.GetReference(pressedAutomationReference);
                return new Entity(entity.GetWorld(), pressedAutomationEntity).As<Automation<Vector4>>();
            }
        }

        public readonly Mesh QuadMesh
        {
            get
            {
                ref AssetReferences component = ref entity.GetComponent<AssetReferences>();
                rint quadMeshReference = component.quadMeshReference;
                uint quadMeshEntity = entity.GetReference(quadMeshReference);
                return new Entity(entity.GetWorld(), quadMeshEntity).As<Mesh>();
            }
        }

        public readonly Font Font
        {
            get
            {
                ref AssetReferences component = ref entity.GetComponent<AssetReferences>();
                rint fontReference = component.fontReference;
                uint fontEntity = entity.GetReference(fontReference);
                return new Entity(entity.GetWorld(), fontEntity).As<Font>();
            }
        }

        public readonly ref TextSelection TextSelection
        {
            get
            {
                ref TextEditState component = ref entity.GetComponent<TextEditState>();
                return ref component.value;
            }
        }

        readonly uint IEntity.Value => entity.GetEntityValue();
        readonly World IEntity.World => entity.GetWorld();
        readonly Definition IEntity.Definition => new Definition().AddComponentTypes<AssetReferences, UISettings, TextEditState>().AddArrayTypes<TextCharacter, MaterialSettings>();

#if NET
        [Obsolete("Default constructor not supported", true)]
        public Settings()
        {
            throw new NotSupportedException();
        }
#endif

        public Settings(World world)
        {
            ThrowIfAlreadyExists(world);
            entity = new(world);

            USpan<AvailableState> states = stackalloc AvailableState[3];
            states[0] = new("idle");
            states[1] = new("selected");
            states[2] = new("pressed");

            USpan<Transition> transitions = stackalloc Transition[4];
            transitions[0] = new("idle", "selected", "selected", Transition.Condition.GreaterThan, 0f);
            transitions[1] = new("selected", "pressed", "pressed", Transition.Condition.GreaterThan, 0f);
            transitions[2] = new("pressed", "selected", "pressed", Transition.Condition.LessThan, 1f);
            transitions[3] = new("selected", "idle", "selected", Transition.Condition.LessThan, 1f);

            StateMachine controlStateMachine = new(world, states, transitions);

            //automations for each state
            USpan<Vector4> keyframes = stackalloc Vector4[1];
            keyframes[0] = new Vector4(0.8f, 0.8f, 0.8f, 1f);
            Automation<Vector4> idleAutomation = new(world, [0f], keyframes);

            keyframes[0] = new Vector4(1.4f, 1.4f, 1.4f, 1f);
            Automation<Vector4> selectedAutomation = new(world, [0f], keyframes);

            keyframes[0] = new Vector4(0.6f, 0.6f, 0.6f, 1f);
            Automation<Vector4> pressedAutomation = new(world, [0f], keyframes);

            //create default quad mesh
            Mesh quadMesh = new(world);
            USpan<Vector3> positions = quadMesh.CreatePositions(4);
            positions[0] = new(0, 0, 0);
            positions[1] = new(1, 0, 0);
            positions[2] = new(1, 1, 0);
            positions[3] = new(0, 1, 0);
            USpan<Vector2> uvs = quadMesh.CreateUVs(4);
            uvs[0] = new(0, 0);
            uvs[1] = new(1, 0);
            uvs[2] = new(1, 1);
            uvs[3] = new(0, 1);
            USpan<Vector3> normals = quadMesh.CreateNormals(4);
            normals[0] = new(0, 0, 1);
            normals[1] = new(0, 0, 1);
            normals[2] = new(0, 0, 1);
            normals[3] = new(0, 0, 1);
            quadMesh.AddTriangle(0, 1, 2);
            quadMesh.AddTriangle(2, 3, 0);

            Texture squareTexture = new(world, Address.Get<SquareTexture>());
            Texture triangleTexture = new(world, Address.Get<TriangleTexture>());
            Texture radialGradientTexture = new(world, Address.Get<RadialGradientAlphaTexture>());

            //create default coloured unlit material
            Material squareMaterial = new(world, Address.Get<UnlitTexturedMaterial>());
            squareMaterial.AddPushBinding<Color>();
            squareMaterial.AddPushBinding<LocalToWorld>();
            squareMaterial.AddComponentBinding<CameraMatrices>(0, 0, default(Entity));
            squareMaterial.AddTextureBinding(1, 0, squareTexture);

            Material triangleMaterial = new(world, Address.Get<UnlitTexturedMaterial>());
            triangleMaterial.AddPushBinding<Color>();
            triangleMaterial.AddPushBinding<LocalToWorld>();
            triangleMaterial.AddComponentBinding<CameraMatrices>(0, 0, default(Entity));
            triangleMaterial.AddTextureBinding(1, 0, triangleTexture);

            Material textMaterial = new(world, Address.Get<TextMaterial>());
            textMaterial.AddComponentBinding<CameraMatrices>(1, 0, default(Entity));
            textMaterial.AddPushBinding<Color>();
            textMaterial.AddPushBinding<LocalToWorld>();

            Material dropShadowMaterial = new(world, Address.Get<UnlitTexturedMaterial>());
            dropShadowMaterial.AddPushBinding<Color>();
            dropShadowMaterial.AddPushBinding<LocalToWorld>();
            dropShadowMaterial.AddComponentBinding<CameraMatrices>(0, 0, default(Entity));
            dropShadowMaterial.AddTextureBinding(1, 0, radialGradientTexture);

            Font defaultFont = new(world, Address.Get<CascadiaMonoFont>());

            rint quadMeshReference = entity.AddReference(quadMesh);
            rint squareMaterialReference = entity.AddReference(squareMaterial);
            rint triangleMaterialReference = entity.AddReference(triangleMaterial);
            rint textMaterialReference = entity.AddReference(textMaterial);
            rint dropShadowMaterialReference = entity.AddReference(dropShadowMaterial);
            rint fontReference = entity.AddReference(defaultFont);
            rint stateMachineReference = entity.AddReference(controlStateMachine);
            rint idleAutomationReference = entity.AddReference(idleAutomation);
            rint selectedAutomationReference = entity.AddReference(selectedAutomation);
            rint pressedAutomationReference = entity.AddReference(pressedAutomation);

            AssetReferences assetReferences = new();
            assetReferences.stateMachineReference = stateMachineReference;
            assetReferences.idleAutomationReference = idleAutomationReference;
            assetReferences.selectedAutomationReference = selectedAutomationReference;
            assetReferences.pressedAutomationReference = pressedAutomationReference;
            assetReferences.quadMeshReference = quadMeshReference;
            assetReferences.fontReference = fontReference;

            UISettings uiSettings = new();
            uiSettings.singleLineHeight = 24f;

            entity.AddComponent(assetReferences);
            entity.AddComponent(uiSettings);
            entity.AddComponent(new TextEditState());
            entity.CreateArray<TextCharacter>();

            MaterialSettings materialSettings = new(default, squareMaterialReference, triangleMaterialReference, textMaterialReference, dropShadowMaterialReference);
            entity.CreateArray([materialSettings]);
        }

        public readonly void Dispose()
        {
            entity.Dispose();
        }

        public readonly void SetPressedCharacters(USpan<char> characters)
        {
            USpan<TextCharacter> array = entity.ResizeArray<TextCharacter>(characters.Length);
            characters.As<TextCharacter>().CopyTo(array);
        }

        public readonly Material GetSquareMaterial(Camera camera)
        {
            MaterialSettings component = GetMaterialSettings(camera);
            rint squareMaterialReference = component.squareMaterialReference;
            uint squareMaterialEntity = entity.GetReference(squareMaterialReference);
            return new Entity(entity.GetWorld(), squareMaterialEntity).As<Material>();
        }

        public readonly Material GetTriangleMaterial(Camera camera)
        {
            MaterialSettings component = GetMaterialSettings(camera);
            rint triangleMaterialReference = component.triangleMaterialReference;
            uint triangleMaterialEntity = entity.GetReference(triangleMaterialReference);
            return new Entity(entity.GetWorld(), triangleMaterialEntity).As<Material>();
        }

        public readonly Material GetTextMaterial(Camera camera)
        {
            MaterialSettings component = GetMaterialSettings(camera);
            rint textMaterialReference = component.textMaterialReference;
            uint textMaterialEntity = entity.GetReference(textMaterialReference);
            return new Entity(entity.GetWorld(), textMaterialEntity).As<Material>();
        }

        public readonly Material GetDropShadowMaterial(Camera camera)
        {
            MaterialSettings component = GetMaterialSettings(camera);
            rint dropShadowMaterialReference = component.dropShadowMaterialReference;
            uint dropShadowMaterialEntity = entity.GetReference(dropShadowMaterialReference);
            return new Entity(entity.GetWorld(), dropShadowMaterialEntity).As<Material>();
        }

        private readonly MaterialSettings GetMaterialSettings(Camera camera)
        {
            World world = entity.GetWorld();
            USpan<MaterialSettings> settings = entity.GetArray<MaterialSettings>();
            uint settingsCount = settings.Length;
            for (uint i = 1; i < settingsCount; i++)
            {
                MaterialSettings materialSettings = settings[i];
                rint cameraReference = materialSettings.cameraReference;
                uint cameraEntity = entity.GetReference(cameraReference);
                if (cameraEntity == camera.GetEntityValue())
                {
                    return materialSettings;
                }
            }

            MaterialSettings defaultSettings = settings[0];
            rint squareMaterialReference = defaultSettings.squareMaterialReference;
            rint triangleMaterialReference = defaultSettings.triangleMaterialReference;
            rint textMaterialReference = defaultSettings.textMaterialReference;
            rint dropShadowMaterialReference = defaultSettings.dropShadowMaterialReference;
            uint squareMaterialEntity = entity.GetReference(squareMaterialReference);
            uint triangleMaterialEntity = entity.GetReference(triangleMaterialReference);
            uint textMaterialEntity = entity.GetReference(textMaterialReference);
            uint dropShadowMaterialEntity = entity.GetReference(dropShadowMaterialReference);
            Material squareMaterial = new(world, squareMaterialEntity);
            Material triangleMaterial = new(world, triangleMaterialEntity);
            Material textMaterial = new(world, textMaterialEntity);
            Material dropShadowMaterial = new(world, dropShadowMaterialEntity);
            //todo: why exactly are these being cloned? its wasteful isnt it?
            //i guess its because it will be different cameras
            squareMaterial = squareMaterial.Clone();
            triangleMaterial = triangleMaterial.Clone();
            textMaterial = textMaterial.Clone();
            dropShadowMaterial = dropShadowMaterial.Clone();
            squareMaterial.SetComponentBinding<CameraMatrices>(0, 0, camera);
            triangleMaterial.SetComponentBinding<CameraMatrices>(0, 0, camera);
            textMaterial.SetComponentBinding<CameraMatrices>(1, 0, camera);
            dropShadowMaterial.SetComponentBinding<CameraMatrices>(0, 0, camera);
            MaterialSettings newSettings = defaultSettings;
            newSettings.cameraReference = entity.AddReference(camera);
            newSettings.squareMaterialReference = entity.AddReference(squareMaterial);
            newSettings.triangleMaterialReference = entity.AddReference(triangleMaterial);
            newSettings.textMaterialReference = entity.AddReference(textMaterial);
            newSettings.dropShadowMaterialReference = entity.AddReference(dropShadowMaterial);
            settings = entity.ResizeArray<MaterialSettings>(settingsCount + 1);
            settings[settingsCount] = newSettings;
            return newSettings;
        }

        [Conditional("DEBUG")]
        public static void ThrowIfAlreadyExists(World world)
        {
            if (world.TryGetFirst<Settings>(out _))
            {
                throw new InvalidOperationException($"An entity with `{nameof(Settings)}` already exists in world, only 1 is permitted");
            }
        }

        [Conditional("DEBUG")]
        public static void ThrowIfMissing(World world)
        {
            if (!world.TryGetFirst<Settings>(out _))
            {
                throw new InvalidOperationException($"An entity with `{nameof(Settings)}` component is missing from the world");
            }
        }

        public static implicit operator Entity(Settings settings)
        {
            return settings.entity;
        }
    }
}