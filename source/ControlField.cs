using Collections;
using System;
using System.Diagnostics;
using System.Numerics;
using Transforms;
using Transforms.Components;
using UI.Components;
using Unmanaged;
using Worlds;

namespace UI
{
    public readonly partial struct ControlField : IEntity
    {
        public readonly Label Label
        {
            get
            {
                rint labelReference = GetComponent<IsControlField>().labelReference;
                uint labelEntity = GetReference(labelReference);
                return new Entity(world, labelEntity).As<Label>();
            }
        }

        public readonly ref Vector4 LabelColor => ref Label.Color;

        public unsafe readonly ref Vector2 Position
        {
            get
            {
                ref Vector3 localPosition = ref As<Transform>().LocalPosition;
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
                ref Vector3 localScale = ref As<Transform>().LocalScale;
                fixed (Vector3* scale = &localScale)
                {
                    return ref *(Vector2*)scale;
                }
            }
        }

        public readonly ref float Z => ref As<Transform>().LocalPosition.Z;

        public readonly Entity Target
        {
            get
            {
                ref IsControlField component = ref GetComponent<IsControlField>();
                rint entityReference = component.entityReference;
                uint entity = GetReference(entityReference);
                return new Entity(world, entity);
            }
        }

        public readonly bool IsComponentType => GetComponent<IsControlField>().dataType.IsComponent;
        public readonly bool IsArrayElementType => GetComponent<IsControlField>().dataType.IsArrayElement;
        public readonly ComponentType ComponentType => GetComponent<IsControlField>().dataType.ComponentType;
        public readonly ArrayElementType ArrayElementType => GetComponent<IsControlField>().dataType.ArrayElementType;
        public readonly ref Anchor Anchor => ref GetComponent<Anchor>();
        public readonly ref Pivot Pivot => ref GetComponent<Pivot>();

        public ControlField(Canvas canvas, FixedString label, Entity target, ComponentType componentType, ControlEditor editor, uint offset = 0) :
            this(canvas, label, target, new DataType(componentType, 0), editor, offset)
        {
        }

        public ControlField(Canvas canvas, FixedString label, Entity target, ArrayElementType arrayElementType, ControlEditor editor, uint offset = 0) :
            this(canvas, label, target, new DataType(arrayElementType, 0), editor, offset)
        {
        }

        private ControlField(Canvas canvas, FixedString label, Entity target, DataType dataType, ControlEditor editor, uint offset)
        {
            ThrowIfMissingData(target, dataType);

            world = canvas.world;
            Transform transform = new(canvas.world);
            value = transform.value;

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
            editor.initializeControlField.Invoke(createdEntities, this, canvas, target, dataType, offset);
            USpan<ControlEntity> referencesArray = transform.AsEntity().CreateArray<ControlEntity>(createdEntities.Count);
            for (uint i = 0; i < createdEntities.Count; i++)
            {
                Entity createdEntity = createdEntities[i];
                rint reference = transform.AddReference(createdEntity);
                referencesArray[i] = new(reference);
            }

            rint labelReference = transform.AddReference(labelEntity);
            rint entityReference = transform.AddReference(target);
            transform.AddComponent(new IsControlField(labelReference, entityReference, dataType));
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsControlField>();
            archetype.AddArrayType<ControlEntity>();
        }

        public static ControlField Create<C, E>(Canvas canvas, FixedString label, Entity target) where C : unmanaged where E : unmanaged, IControlEditor
        {
            Schema schema = canvas.world.Schema;
            ComponentType componentType = schema.GetComponent<C>();
            ControlEditor editor = ControlEditor.Get<E>();
            return new ControlField(canvas, label, target, componentType, editor);
        }

        [Conditional("DEBUG")]
        private static void ThrowIfMissingData(Entity entity, DataType dataType)
        {
            if (dataType.IsComponent)
            {
                ComponentType componentType = dataType.ComponentType;
                if (!entity.Contains(componentType))
                {
                    throw new NullReferenceException($"Entity `{entity}` is missing component `{componentType.ToString(entity.world.Schema)}`");
                }
            }
            else if (dataType.IsArrayElement)
            {
                ArrayElementType arrayElementType = dataType.ArrayElementType;
                if (!entity.Contains(arrayElementType))
                {
                    throw new NullReferenceException($"Entity `{entity}` is missing array `{arrayElementType.ToString(entity.world.Schema)}`");
                }
            }
        }

        public static implicit operator Transform(ControlField controlField)
        {
            return controlField.As<Transform>();
        }
    }
}