using Automations;
using InteractionKit.Components;
using Rendering;
using Rendering.Components;
using System;
using System.Numerics;
using Textures;
using Transforms;
using Transforms.Components;
using Worlds;

namespace InteractionKit
{
    public readonly struct Image : IEntity
    {
        private readonly Transform transform;

        public unsafe readonly ref Vector2 Position
        {
            get
            {
                ref Vector3 localPosition = ref transform.LocalPosition;
                fixed (Vector3* positionPointer = &localPosition)
                {
                    return ref *(Vector2*)positionPointer;
                }
            }
        }

        public readonly ref float Z
        {
            get
            {
                ref Vector3 localPosition = ref transform.LocalPosition;
                return ref localPosition.Z;
            }
        }

        public unsafe readonly ref Vector2 Size
        {
            get
            {
                ref Vector3 localScale = ref transform.LocalScale;
                fixed (Vector3* sizePointer = &localScale)
                {
                    return ref *(Vector2*)sizePointer;
                }
            }
        }

        public readonly float Rotation
        {
            get
            {
                Quaternion rotation = transform.LocalRotation;
                return new EulerAngles(rotation).value.Z;
            }
            set
            {
                ref Quaternion rotation = ref transform.LocalRotation;
                rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, value);
            }
        }

        public readonly ref Anchor Anchor => ref transform.AsEntity().GetComponent<Anchor>();
        public readonly ref Vector3 Pivot => ref transform.AsEntity().GetComponent<Pivot>().value;
        public readonly ref Vector4 Color => ref transform.AsEntity().GetComponent<BaseColor>().value;

        public readonly Material Material
        {
            get
            {
                rint materialReference = transform.AsEntity().GetComponent<IsRenderer>().materialReference;
                uint materialEntity = transform.GetReference(materialReference);
                return new(transform.GetWorld(), materialEntity);
            }
            set
            {
                ref IsRenderer renderer = ref transform.AsEntity().GetComponent<IsRenderer>();
                if (renderer.materialReference != default)
                {
                    transform.SetReference(renderer.materialReference, value.GetEntityValue());
                }
                else
                {
                    renderer.materialReference = transform.AddReference(value.GetEntityValue());
                }
            }
        }

        public readonly Texture Texture
        {
            get
            {
                MaterialTextureBinding binding = Material.GetTextureBindingRef(1, 0);
                uint textureEntity = binding.TextureEntity;
                return new(transform.GetWorld(), textureEntity);
            }
            set
            {
                ref MaterialTextureBinding binding = ref Material.GetTextureBindingRef(1, 0);
                binding.SetTexture(value);
            }
        }

        readonly uint IEntity.Value => transform.GetEntityValue();
        readonly World IEntity.World => transform.GetWorld();

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsRenderer>();
            archetype.AddTagType<IsTransform>();
        }

#if NET
        [Obsolete("Default constructor not available", true)]
        public Image()
        {
            throw new NotSupportedException();
        }
#endif
        public Image(World world, uint existingEntity)
        {
            transform = new(world, existingEntity);
        }

        public Image(Canvas canvas)
        {
            World world = canvas.GetWorld();
            Settings settings = canvas.Settings;
            Schema schema = world.Schema;

            transform = new(world);
            transform.LocalPosition = new(0f, 0f, Settings.ZScale);
            transform.AddComponent(new Anchor());
            transform.AddComponent(new Pivot());
            transform.AddComponent(new ColorTint(new Vector4(1f)));
            transform.AddComponent(new BaseColor(new Vector4(1f)));
            transform.AddComponent(new Color(new Vector4(1f)));
            transform.AddComponent(ComponentMix.Create<ColorTint, BaseColor, Color>(ComponentMix.Operation.FloatingMultiply, 4, schema));
            transform.SetParent(canvas);

            StatefulAutomationPlayer stateful = transform.AsEntity().Become<StatefulAutomationPlayer>();
            stateful.StateMachine = settings.ControlStateMachine;
            stateful.AddParameter("pressed", 0f);
            stateful.AddParameter("selected", 1f);
            stateful.AddOrSetLink<ColorTint>("idle", settings.IdleAutomation);
            stateful.AddOrSetLink<ColorTint>("selected", settings.SelectedAutomation);
            stateful.AddOrSetLink<ColorTint>("pressed", settings.PressedAutomation);

            MeshRenderer renderer = transform.AsEntity().Become<MeshRenderer>();
            renderer.Mesh = settings.QuadMesh;
            renderer.Material = settings.GetSquareMaterial(canvas.Camera);
            renderer.Mask = canvas.RenderMask;
        }

        public readonly void Dispose()
        {
            transform.Dispose();
        }

        public static implicit operator Entity(Image box)
        {
            return box.AsEntity();
        }

        public static implicit operator Transform(Image box)
        {
            return box.transform;
        }

        public static implicit operator MeshRenderer(Image box)
        {
            return box.AsEntity().As<MeshRenderer>();
        }
    }
}