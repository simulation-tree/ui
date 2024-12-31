using Collections;
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
        private readonly Transform transform;

        public readonly Label Label
        {
            get
            {
                rint labelReference = transform.AsEntity().GetComponent<IsControlField>().labelReference;
                uint labelEntity = transform.GetReference(labelReference);
                return new Label(transform.GetWorld(), labelEntity);
            }
        }

        public readonly ref Vector4 LabelColor => ref Label.Color;

        public unsafe readonly ref Vector2 Position
        {
            get
            {
                ref Vector3 localPosition = ref transform.LocalPosition;
                fixed (Vector3* position = &localPosition)
                {
                    return ref *(Vector2*)position;
                }
            }
        }

        public unsafe readonly ref Vector2 Size
        {
            get
            {
                ref Vector3 localScale = ref transform.LocalScale;
                fixed (Vector3* scale = &localScale)
                {
                    return ref *(Vector2*)scale;
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

        public readonly bool IsArrayElementType => !IsComponentType;

        public readonly ComponentType ComponentType
        {
            get
            {
                ref IsControlField component = ref transform.AsEntity().GetComponent<IsControlField>();
                return new(component.typeIndex);
            }
        }

        public readonly ArrayElementType ArrayElementType
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
            return new Definition().AddComponentType<IsControlField>(schema).AddArrayElementType<ControlEntity>(schema);
        }

        public ControlField(World world, uint existingEntity)
        {
            transform = new(world, existingEntity);
        }

        public ControlField(Canvas canvas, FixedString label, Entity target, ComponentType componentType, ControlEditor editor, uint offset = 0) :
            this(canvas, label, target, componentType.index, true, editor, offset)
        {
        }

        public ControlField(Canvas canvas, FixedString label, Entity target, ArrayElementType arrayElementType, ControlEditor editor, uint offset = 0) :
            this(canvas, label, target, arrayElementType.index, false, editor, offset)
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
            labelEntity.Color = new(1, 1, 1, 1);
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
                    throw new NullReferenceException($"Entity `{entity}` is missing component `{componentType.ToString(entity.GetWorld().Schema)}`");
                }
            }
            else
            {
                ArrayElementType arrayElementType = new(typeIndex);
                if (!entity.ContainsArray(arrayElementType))
                {
                    throw new NullReferenceException($"Entity `{entity}` is missing array `{arrayElementType.ToString(entity.GetWorld().Schema)}`");
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