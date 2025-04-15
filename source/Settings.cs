using Automations;
using Cameras;
using Cameras.Components;
using Data;
using DefaultPresentationAssets;
using Fonts;
using Materials;
using Meshes;
using Rendering;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Textures;
using Transforms.Components;
using UI.Components;
using Worlds;

namespace UI
{
    public readonly partial struct Settings : IEntity
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
        public const char EscapeCharacter = '\e';
        public const char EnterCharacter = '\n';

        public readonly ref float SingleLineHeight => ref GetComponent<UISettings>().singleLineHeight;
        public readonly ref PressedCharacters PressedCharacters => ref GetComponent<UISettings>().pressedCharacters;

        public readonly StateMachine ControlStateMachine
        {
            get
            {
                ref AssetReferences component = ref GetComponent<AssetReferences>();
                rint stateMachineReference = component.stateMachineReference;
                uint stateMachineEntity = GetReference(stateMachineReference);
                return new Entity(world, stateMachineEntity).As<StateMachine>();
            }
        }

        public readonly AutomationEntity<Vector4> IdleAutomation
        {
            get
            {
                ref AssetReferences component = ref GetComponent<AssetReferences>();
                rint idleAutomationReference = component.idleAutomationReference;
                uint idleAutomationEntity = GetReference(idleAutomationReference);
                return new Entity(world, idleAutomationEntity).As<AutomationEntity<Vector4>>();
            }
        }

        public readonly AutomationEntity<Vector4> SelectedAutomation
        {
            get
            {
                ref AssetReferences component = ref GetComponent<AssetReferences>();
                rint selectedAutomationReference = component.selectedAutomationReference;
                uint selectedAutomationEntity = GetReference(selectedAutomationReference);
                return new Entity(world, selectedAutomationEntity).As<AutomationEntity<Vector4>>();
            }
        }

        public readonly AutomationEntity<Vector4> PressedAutomation
        {
            get
            {
                ref AssetReferences component = ref GetComponent<AssetReferences>();
                rint pressedAutomationReference = component.pressedAutomationReference;
                uint pressedAutomationEntity = GetReference(pressedAutomationReference);
                return new Entity(world, pressedAutomationEntity).As<AutomationEntity<Vector4>>();
            }
        }

        public readonly Mesh QuadMesh
        {
            get
            {
                ref AssetReferences component = ref GetComponent<AssetReferences>();
                rint quadMeshReference = component.quadMeshReference;
                uint quadMeshEntity = GetReference(quadMeshReference);
                return new Entity(world, quadMeshEntity).As<Mesh>();
            }
        }

        public readonly Font Font
        {
            get
            {
                ref AssetReferences component = ref GetComponent<AssetReferences>();
                rint fontReference = component.fontReference;
                uint fontEntity = GetReference(fontReference);
                return new Entity(world, fontEntity).As<Font>();
            }
        }

        public readonly ref TextSelection TextSelection => ref GetComponent<TextEditState>().value;

        [SkipLocalsInit]
        public Settings(World world)
        {
            ThrowIfAlreadyExists(world);

            this.world = world;
            value = world.CreateEntity();

            Span<AvailableState> states = stackalloc AvailableState[3];
            states[0] = new("idle");
            states[1] = new("selected");
            states[2] = new("pressed");

            Span<Transition> transitions = stackalloc Transition[4];
            transitions[0] = new("idle", "selected", "selected", Transition.Condition.GreaterThan, 0f);
            transitions[1] = new("selected", "pressed", "pressed", Transition.Condition.GreaterThan, 0f);
            transitions[2] = new("pressed", "selected", "pressed", Transition.Condition.LessThan, 1f);
            transitions[3] = new("selected", "idle", "selected", Transition.Condition.LessThan, 1f);

            StateMachine controlStateMachine = new(world, states, transitions);

            //automations for each state
            Span<Vector4> keyframes = stackalloc Vector4[1];
            keyframes[0] = new Vector4(0.8f, 0.8f, 0.8f, 1f);
            AutomationEntity<Vector4> idleAutomation = new(world, [0f], keyframes);

            keyframes[0] = new Vector4(1.4f, 1.4f, 1.4f, 1f);
            AutomationEntity<Vector4> selectedAutomation = new(world, [0f], keyframes);

            keyframes[0] = new Vector4(0.6f, 0.6f, 0.6f, 1f);
            AutomationEntity<Vector4> pressedAutomation = new(world, [0f], keyframes);

            //create default quad mesh
            Mesh quadMesh = Mesh.CreateBottomLeftQuad(world);

            Texture squareTexture = new(world, EmbeddedResource.GetAddress<SquareTexture>());
            Texture triangleTexture = new(world, EmbeddedResource.GetAddress<TriangleTexture>());
            Texture radialGradientTexture = new(world, EmbeddedResource.GetAddress<RadialGradientAlphaTexture>());

            //create default coloured unlit material
            Material squareMaterial = new(world, EmbeddedResource.GetAddress<UnlitTexturedMaterial>());
            squareMaterial.AddInstanceBinding<Color>();
            squareMaterial.AddInstanceBinding<LocalToWorld>();
            squareMaterial.AddComponentBinding<CameraMatrices>(new(0, 0), default(Entity));
            squareMaterial.AddTextureBinding(new(1, 0), squareTexture);

            Material triangleMaterial = new(world, EmbeddedResource.GetAddress<UnlitTexturedTransparentMaterial>());
            triangleMaterial.AddInstanceBinding<Color>();
            triangleMaterial.AddInstanceBinding<LocalToWorld>();
            triangleMaterial.AddComponentBinding<CameraMatrices>(new(0, 0), default(Entity));
            triangleMaterial.AddTextureBinding(new(1, 0), triangleTexture);

            Material textMaterial = new(world, EmbeddedResource.GetAddress<TextMaterial>());
            textMaterial.AddComponentBinding<CameraMatrices>(new(1, 0), default(Entity));
            textMaterial.AddInstanceBinding<Color>();
            textMaterial.AddInstanceBinding<LocalToWorld>();

            Material dropShadowMaterial = new(world, EmbeddedResource.GetAddress<UnlitTexturedTransparentMaterial>());
            dropShadowMaterial.AddInstanceBinding<Color>();
            dropShadowMaterial.AddInstanceBinding<LocalToWorld>();
            dropShadowMaterial.AddComponentBinding<CameraMatrices>(new(0, 0), default(Entity));
            dropShadowMaterial.AddTextureBinding(new(1, 0), radialGradientTexture);

            Font defaultFont = new(world, EmbeddedResource.GetAddress<CascadiaMonoFont>());

            rint quadMeshReference = AddReference(quadMesh);
            rint squareMaterialReference = AddReference(squareMaterial);
            rint triangleMaterialReference = AddReference(triangleMaterial);
            rint textMaterialReference = AddReference(textMaterial);
            rint dropShadowMaterialReference = AddReference(dropShadowMaterial);
            rint fontReference = AddReference(defaultFont);
            rint stateMachineReference = AddReference(controlStateMachine);
            rint idleAutomationReference = AddReference(idleAutomation);
            rint selectedAutomationReference = AddReference(selectedAutomation);
            rint pressedAutomationReference = AddReference(pressedAutomation);

            AssetReferences assetReferences = new();
            assetReferences.stateMachineReference = stateMachineReference;
            assetReferences.idleAutomationReference = idleAutomationReference;
            assetReferences.selectedAutomationReference = selectedAutomationReference;
            assetReferences.pressedAutomationReference = pressedAutomationReference;
            assetReferences.quadMeshReference = quadMeshReference;
            assetReferences.fontReference = fontReference;

            UISettings uiSettings = new();
            uiSettings.singleLineHeight = 24f;

            AddComponent(assetReferences);
            AddComponent(uiSettings);
            AddComponent(new TextEditState());

            MaterialSettings materialSettings = new(default, squareMaterialReference, triangleMaterialReference, textMaterialReference, dropShadowMaterialReference);
            CreateArray([materialSettings]);
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<AssetReferences>();
            archetype.AddComponentType<UISettings>();
            archetype.AddComponentType<TextEditState>();
            archetype.AddArrayType<MaterialSettings>();
        }

        public readonly Material GetSquareMaterial(Camera camera)
        {
            MaterialSettings component = GetMaterialSettings(camera);
            rint squareMaterialReference = component.squareMaterialReference;
            uint squareMaterialEntity = GetReference(squareMaterialReference);
            return new Entity(world, squareMaterialEntity).As<Material>();
        }

        public readonly Material GetTriangleMaterial(Camera camera)
        {
            MaterialSettings component = GetMaterialSettings(camera);
            rint triangleMaterialReference = component.triangleMaterialReference;
            uint triangleMaterialEntity = GetReference(triangleMaterialReference);
            return new Entity(world, triangleMaterialEntity).As<Material>();
        }

        public readonly Material GetTextMaterial(Camera camera)
        {
            MaterialSettings component = GetMaterialSettings(camera);
            rint textMaterialReference = component.textMaterialReference;
            uint textMaterialEntity = GetReference(textMaterialReference);
            return new Entity(world, textMaterialEntity).As<Material>();
        }

        public readonly Material GetDropShadowMaterial(Camera camera)
        {
            MaterialSettings component = GetMaterialSettings(camera);
            rint dropShadowMaterialReference = component.dropShadowMaterialReference;
            uint dropShadowMaterialEntity = GetReference(dropShadowMaterialReference);
            return new Entity(world, dropShadowMaterialEntity).As<Material>();
        }

        public readonly bool IsDropShadowMaterial(uint materialEntity)
        {
            ReadOnlySpan<MaterialSettings> settings = GetArray<MaterialSettings>();
            for (int i = 0; i < settings.Length; i++)
            {
                MaterialSettings materialSettings = settings[i];
                rint dropShadowMaterialReference = materialSettings.dropShadowMaterialReference;
                uint dropShadowMaterialEntity = GetReference(dropShadowMaterialReference);
                if (dropShadowMaterialEntity == materialEntity)
                {
                    return true;
                }
            }

            return false;
        }

        private readonly MaterialSettings GetMaterialSettings(Camera camera)
        {
            Values<MaterialSettings> settings = GetArray<MaterialSettings>();
            int settingsCount = settings.Length;
            for (int i = 1; i < settingsCount; i++)
            {
                MaterialSettings materialSettings = settings[i];
                rint cameraReference = materialSettings.cameraReference;
                uint cameraEntity = GetReference(cameraReference);
                if (cameraEntity == camera.value)
                {
                    return materialSettings;
                }
            }

            MaterialSettings defaultSettings = settings[0];
            rint squareMaterialReference = defaultSettings.squareMaterialReference;
            rint triangleMaterialReference = defaultSettings.triangleMaterialReference;
            rint textMaterialReference = defaultSettings.textMaterialReference;
            rint dropShadowMaterialReference = defaultSettings.dropShadowMaterialReference;
            uint squareMaterialEntity = GetReference(squareMaterialReference);
            uint triangleMaterialEntity = GetReference(triangleMaterialReference);
            uint textMaterialEntity = GetReference(textMaterialReference);
            uint dropShadowMaterialEntity = GetReference(dropShadowMaterialReference);
            Material squareMaterial = new Entity(world, squareMaterialEntity).As<Material>();
            Material triangleMaterial = new Entity(world, triangleMaterialEntity).As<Material>();
            Material textMaterial = new Entity(world, textMaterialEntity).As<Material>();
            Material dropShadowMaterial = new Entity(world, dropShadowMaterialEntity).As<Material>();
            //todo: why exactly are these being cloned? its wasteful isnt it?
            //i guess its because it will be different cameras
            squareMaterial = squareMaterial.Clone();
            triangleMaterial = triangleMaterial.Clone();
            textMaterial = textMaterial.Clone();
            dropShadowMaterial = dropShadowMaterial.Clone();
            squareMaterial.SetComponentBinding<CameraMatrices>(new(0, 0), camera);
            triangleMaterial.SetComponentBinding<CameraMatrices>(new(0, 0), camera);
            textMaterial.SetComponentBinding<CameraMatrices>(new(1, 0), camera);
            dropShadowMaterial.SetComponentBinding<CameraMatrices>(new(0, 0), camera);
            MaterialSettings newSettings = defaultSettings;
            newSettings.cameraReference = AddReference(camera);
            newSettings.squareMaterialReference = AddReference(squareMaterial);
            newSettings.triangleMaterialReference = AddReference(triangleMaterial);
            newSettings.textMaterialReference = AddReference(textMaterial);
            newSettings.dropShadowMaterialReference = AddReference(dropShadowMaterial);
            settings.Add(newSettings);
            return newSettings;
        }

        [Conditional("DEBUG")]
        public static void ThrowIfAlreadyExists(World world)
        {
            if (world.TryGetFirst<Settings>(out _))
            {
                throw new InvalidOperationException($"A `{nameof(Settings)}` entity already exists in world, only 1 is permitted");
            }
        }

        [Conditional("DEBUG")]
        public static void ThrowIfMissing(World world)
        {
            if (!world.TryGetFirst<Settings>(out _))
            {
                throw new InvalidOperationException($"A `{nameof(Settings)}` entity is missing from the world");
            }
        }
    }
}