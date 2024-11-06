using Automations;
using Data;
using InteractionKit.Components;
using Rendering;
using Rendering.Components;
using Simulation;
using System;
using System.Numerics;
using Textures;
using Transforms;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct Image : IEntity
    {
        public readonly Transform transform;

        public readonly Entity Parent
        {
            get => transform.Parent;
            set => transform.Parent = value;
        }

        public readonly Vector2 Position
        {
            get
            {
                Vector3 position = transform.LocalPosition;
                return new(position.X, position.Y);
            }
            set
            {
                Vector3 position = transform.LocalPosition;
                transform.LocalPosition = new(value.X, value.Y, position.Z);
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
                Vector3 scale = transform.LocalScale;
                transform.LocalScale = new(value.X, value.Y, scale.Z);
            }
        }

        public readonly ref Anchor Anchor => ref transform.entity.GetComponentRef<Anchor>();
        public readonly ref Vector3 Pivot => ref transform.entity.GetComponentRef<Pivot>().value;
        public readonly ref Color Color => ref transform.entity.GetComponentRef<BaseColor>().value;

        public readonly Material Material
        {
            get
            {
                rint materialReference = transform.entity.GetComponent<IsRenderer>().materialReference;
                uint materialEntity = transform.GetReference(materialReference);
                return new(transform.GetWorld(), materialEntity);
            }
            set
            {
                ref IsRenderer renderer = ref transform.entity.GetComponentRef<IsRenderer>();
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
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsTransform>(), RuntimeType.Get<IsRenderer>()], []);

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

        public Image(World world, Canvas canvas)
        {
            Settings.ThrowIfMissing(world);
            Settings settings = world.GetFirst<Settings>();

            transform = new(world);
            transform.LocalPosition = new(0f, 0f, 0.1f);
            transform.AddComponent(new Anchor());
            transform.AddComponent(new Pivot());
            transform.AddComponent(new ColorTint(new Vector4(1f)));
            transform.AddComponent(new BaseColor(new Vector4(1f)));
            transform.AddComponent(new Color(new Vector4(1f)));
            transform.AddComponent(ComponentMix.Create<ColorTint, BaseColor, Color>(ComponentMix.Operation.FloatingMultiply, 4));
            transform.Parent = canvas;

            StatefulAutomationPlayer stateful = transform.entity.Become<StatefulAutomationPlayer>();
            stateful.StateMachine = settings.ControlStateMachine;
            stateful.AddParameter("pressed", 0f);
            stateful.AddParameter("selected", 1f);
            stateful.AddOrSetLink<ColorTint>("idle", settings.IdleAutomation);
            stateful.AddOrSetLink<ColorTint>("selected", settings.SelectedAutomation);
            stateful.AddOrSetLink<ColorTint>("pressed", settings.PressedAutomation);

            MeshRenderer renderer = transform.entity.Become<MeshRenderer>();
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
    }
}