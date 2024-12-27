using Collections;
using Data;
using InteractionKit.Components;
using System;
using System.Diagnostics;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Unmanaged;
using Worlds;

namespace InteractionKit
{
    public readonly struct ControlField : IEntity
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

        public readonly Entity Target
        {
            get
            {
                ref IsControlField component = ref transform.AsEntity().GetComponent<IsControlField>();
                rint entityReference = component.entityReference;
                uint entity = transform.GetReference(entityReference);
                return new Entity(transform.GetWorld(), entity);
            }
        }

        public readonly bool IsComponentType
        {
            get
            {
                ref IsControlField component = ref transform.AsEntity().GetComponent<IsControlField>();
                return component.isComponentType;
            }
        }

        public readonly bool IsArrayType => !IsComponentType;

        public readonly ComponentType ComponentType
        {
            get
            {
                ref IsControlField component = ref transform.AsEntity().GetComponent<IsControlField>();
                return new(component.typeIndex);
            }
        }

        public readonly ArrayType ArrayType
        {
            get
            {
                ref IsControlField component = ref transform.AsEntity().GetComponent<IsControlField>();
                return new(component.typeIndex);
            }
        }

        public readonly ref Anchor Anchor => ref transform.AsEntity().GetComponent<Anchor>();
        public readonly ref Pivot Pivot => ref transform.AsEntity().GetComponent<Pivot>();

        readonly uint IEntity.Value => transform.GetEntityValue();
        readonly World IEntity.World => transform.GetWorld();

        readonly Definition IEntity.GetDefinition(Schema schema)
        {
            return new Definition().AddComponentType<IsControlField>(schema).AddArrayType<ControlEntity>(schema);
        }

        public ControlField(World world, uint existingEntity)
        {
            transform = new(world, existingEntity);
        }

        public ControlField(Canvas canvas, FixedString label, Entity target, ComponentType componentType, ControlEditor editor, uint offset = 0) :
            this(canvas, label, target, componentType.index, true, editor, offset)
        {
        }

        public ControlField(Canvas canvas, FixedString label, Entity target, ArrayType arrayType, ControlEditor editor, uint offset = 0) :
            this(canvas, label, target, arrayType.index, false, editor, offset)
        {
        }

        private ControlField(Canvas canvas, FixedString label, Entity target, byte typeIndex, bool isComponentType, ControlEditor editor, uint offset)
        {
            ThrowIfMissingData(target, typeIndex, isComponentType);

            transform = new(canvas.GetWorld());
            transform.LocalPosition = new(0f, 0f, Settings.ZScale);
            transform.SetParent(canvas);
            transform.AddComponent(new Anchor());
            transform.AddComponent(new Pivot());

            Label labelEntity = new(canvas, label);
            labelEntity.SetParent(transform);
            labelEntity.Anchor = Anchor.TopLeft;
            labelEntity.Color = Color.White;
            labelEntity.Position = new(4f, -4f);
            labelEntity.Pivot = new(0f, 1f, 0f);

            using List<Entity> createdEntities = new();
            editor.initializeControlField.Invoke(createdEntities, this, canvas, target, typeIndex, isComponentType, offset);
            USpan<ControlEntity> referencesArray = transform.AsEntity().CreateArray<ControlEntity>(createdEntities.Count);
            for (uint i = 0; i < createdEntities.Count; i++)
            {
                Entity createdEntity = createdEntities[i];
                rint reference = transform.AddReference(createdEntity);
                referencesArray[i] = new(reference);
            }

            rint labelReference = transform.AddReference(labelEntity);
            rint entityReference = transform.AddReference(target);
            transform.AddComponent(new IsControlField(labelReference, entityReference, typeIndex, isComponentType));
        }

        public readonly void Dispose()
        {
            transform.Dispose();
        }

        public static ControlField Create<C, E>(Canvas canvas, FixedString label, Entity target) where C : unmanaged where E : unmanaged, IControlEditor
        {
            Schema schema = canvas.GetWorld().Schema;
            ComponentType componentType = schema.GetComponent<C>();
            ControlEditor editor = ControlEditor.Get<E>();
            return new ControlField(canvas, label, target, componentType, editor);
        }

        [Conditional("DEBUG")]
        private static void ThrowIfMissingData(Entity entity, byte typeIndex, bool isComponentType)
        {
            if (isComponentType)
            {
                ComponentType componentType = new(typeIndex);
                if (!entity.ContainsComponent(componentType))
                {
                    throw new NullReferenceException($"Entity `{entity}` is missing component `{componentType}`");
                }
            }
            else
            {
                ArrayType arrayType = new(typeIndex);
                if (!entity.ContainsArray(arrayType))
                {
                    throw new NullReferenceException($"Entity `{entity}` is missing array `{arrayType}`");
                }
            }
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