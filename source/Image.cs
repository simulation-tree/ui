using Automations;
using Data;
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

        public readonly Vector2 Position
        {
            get
            {
                Vector3 position = transform.LocalPosition;
                return new(position.X, position.Y);
            }
            set
            {
                ref Vector3 position = ref transform.LocalPosition;
                position.X = value.X;
                position.Y = value.Y;
            }
        }

        public readonly float Z
        {
            get
            {
                return transform.LocalPosition.Z;
            }
            set
            {
                ref Vector3 position = ref transform.LocalPosition;
                position.Z = value;
            }
        }

        public readonly Vector2 Size
        {
            get
            {
                Vector3 scale = transform.LocalScale;
                return new(scale.X, scale.Y);
            }
            set
            {
                ref Vector3 scale = ref transform.LocalScale;
                scale.X = value.X;
                scale.Y = value.Y;
            }
        }

        public readonly ref Anchor Anchor => ref transform.AsEntity().GetComponent<Anchor>();
        public readonly ref Vector3 Pivot => ref transform.AsEntity().GetComponent<Pivot>().value;
        public readonly ref Color Color => ref transform.AsEntity().GetComponent<BaseColor>().value;

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
        readonly Definition IEntity.Definition => new Definition().AddComponentTypes<IsTransform, IsRenderer>();

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
            Settings settings = canvas.GetSettings();

            transform = new(world);
            transform.LocalPosition = new(0f, 0f, Settings.ZScale);
            transform.AddComponent(new Anchor());
            transform.AddComponent(new Pivot());
            transform.AddComponent(new ColorTint(new Vector4(1f)));
            transform.AddComponent(new BaseColor(new Vector4(1f)));
            transform.AddComponent(new Color(new Vector4(1f)));
            transform.AddComponent(ComponentMix.Create<ColorTint, BaseColor, Color>(ComponentMix.Operation.FloatingMultiply, 4));
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
            renderer.Mask = 1; //todo: customizing the layer that ui controls are on
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