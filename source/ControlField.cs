using Data;
using InteractionKit.Components;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms;
using Transforms.Components;
using Unmanaged;
using Worlds;

namespace InteractionKit
{
    public readonly struct ControlField : ICanvasDescendant
    {
        public readonly Transform transform;

        public readonly Label Label
        {
            get
            {
                rint labelReference = transform.AsEntity().GetComponent<IsControlField>().labelReference;
                uint labelEntity = transform.GetReference(labelReference);
                return new Label(transform.GetWorld(), labelEntity);
            }
        }

        public readonly ref Color LabelColor => ref Label.Color;

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

        public readonly (Entity entity, ComponentType componentType) Target
        {
            get
            {
                IsControlField component = transform.AsEntity().GetComponent<IsControlField>();
                rint entityReference = component.entityReference;
                uint entity = transform.GetReference(entityReference);
                return (new Entity(transform.GetWorld(), entity), component.componentType);
            }
        }

        public readonly ref Anchor Anchor => ref transform.AsEntity().GetComponent<Anchor>();
        public readonly ref Pivot Pivot => ref transform.AsEntity().GetComponent<Pivot>();

        readonly uint IEntity.Value => transform.GetEntityValue();
        readonly World IEntity.World => transform.GetWorld();
        readonly Definition IEntity.Definition => new Definition().AddComponentType<IsControlField>();

        public ControlField(World world, Canvas canvas, FixedString label, Entity entity, ComponentType componentType)
            : this(world, canvas, label, entity.GetEntityValue(), componentType)
        {
        }

        public unsafe ControlField(World world, Canvas canvas, FixedString label, uint entity, ComponentType componentType)
        {
            ThrowIfEntityIsMissingComponent(world, entity, componentType);

            transform = new(world);
            transform.LocalPosition = new(0f, 0f, 0.1f);
            transform.SetParent(canvas);
            transform.AddComponent(new Anchor());
            transform.AddComponent(new Pivot());

            Label labelEntity = new(canvas, label);
            labelEntity.SetParent(transform);
            labelEntity.Anchor = Anchor.TopLeft;
            labelEntity.Color = Color.White;
            labelEntity.Position = new(4f, -4f);
            labelEntity.Pivot = new(0f, 1f, 0f);

            rint controlReference = default;
            if (componentType == ComponentType.Get<bool>())
            {
                bool initialValue = world.GetComponent<bool>(entity);
                Toggle toggle = new(canvas, initialValue);
                toggle.SetParent(transform);
                //toggle.Position = new(100f, 0f);
                toggle.Size = new(24, 24f);
                toggle.BackgroundColor = new(0.2f, 0.2f, 0.2f, 1f);
                toggle.CheckmarkColor = Color.White;
                toggle.Anchor = Anchor.Centered;
                toggle.Pivot = new(0f, 0.5f, 0f);
                toggle.Callback = new(&BooleanToggled);
                controlReference = transform.AddReference(toggle);
            }
            else
            {
                throw new NotImplementedException($"ControlField does not support component type `{componentType}`");
            }

            rint labelReference = transform.AddReference(labelEntity);
            rint entityReference = transform.AddReference(entity);
            transform.AddComponent(new IsControlField(labelReference, controlReference, entityReference, componentType));
        }

        public readonly void Dispose()
        {
            transform.Dispose();
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfEntityIsMissingComponent(World world, uint entity, ComponentType componentType)
        {
            if (!world.ContainsComponent(entity, componentType))
            {
                throw new NullReferenceException($"Entity `{entity}` is missing component {componentType}");
            }
        }

        [Conditional("DEBUG")]
        private static void ThrowIfTypeMismatches<T>(ComponentType targetType) where T : unmanaged
        {
            if (targetType != ComponentType.Get<T>())
            {
                throw new ArgumentException($"Type mismatch: {typeof(T)} != {targetType}");
            }
        }

        [UnmanagedCallersOnly]
        private static void BooleanToggled(Toggle toggle, byte newValue)
        {
            ControlField controlField = toggle.GetParent().As<ControlField>();
            (Entity entity, ComponentType componentType) = controlField.Target;
            ThrowIfTypeMismatches<bool>(componentType);
            entity.SetComponent<bool>(newValue == 1);
        }

        public static implicit operator Transform(ControlField controlField)
        {
            return controlField.transform;
        }

        public static implicit operator Entity(ControlField controlField)
        {
            return controlField.AsEntity();
        }
    }
}