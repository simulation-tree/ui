using Collections.Generic;
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
        public readonly ref Vector2 Position => ref As<UITransform>().Position;
        public readonly ref float X => ref As<UITransform>().X;
        public readonly ref float Y => ref As<UITransform>().Y;
        public readonly ref float Z => ref As<UITransform>().Z;
        public readonly ref Vector2 Size => ref As<UITransform>().Size;
        public readonly ref float Width => ref As<UITransform>().Width;
        public readonly ref float Height => ref As<UITransform>().Height;

        public readonly float Rotation
        {
            get => As<UITransform>().Rotation;
            set => As<UITransform>().Rotation = value;
        }

        public readonly ref Anchor Anchor => ref As<UITransform>().Anchor;
        public readonly ref Vector3 Pivot => ref As<UITransform>().Pivot;

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
        public readonly bool IsArrayType => GetComponent<IsControlField>().dataType.IsArrayElement;
        public readonly int ComponentType => GetComponent<IsControlField>().dataType.index;
        public readonly int ArrayType => GetComponent<IsControlField>().dataType.index;

        public ControlField(Canvas canvas, ASCIIText256 label, Entity target, DataType dataType, ControlEditor editor, uint offset = 0)
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
            Values<ControlEntity> referencesArray = transform.CreateArray<ControlEntity>(createdEntities.Count);
            for (int i = 0; i < createdEntities.Count; i++)
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

        public static ControlField Create<C, E>(Canvas canvas, ASCIIText256 label, Entity target) where C : unmanaged where E : unmanaged, IControlEditor
        {
            Schema schema = canvas.world.Schema;
            ControlEditor editor = ControlEditor.Get<E>();
            return new ControlField(canvas, label, target, DataType.GetComponent<C>(schema), editor);
        }

        [Conditional("DEBUG")]
        private static void ThrowIfMissingData(Entity entity, DataType dataType)
        {
            if (dataType.IsComponent)
            {
                if (!entity.ContainsComponent(dataType.index))
                {
                    throw new NullReferenceException($"Entity `{entity}` is missing component `{dataType.ToString(entity.world.Schema)}`");
                }
            }
            else if (dataType.IsArrayElement)
            {
                if (!entity.ContainsArray(dataType.index))
                {
                    throw new NullReferenceException($"Entity `{entity}` is missing array `{dataType.ToString(entity.world.Schema)}`");
                }
            }
        }

        public static implicit operator Transform(ControlField controlField)
        {
            return controlField.As<Transform>();
        }

        public static implicit operator UITransform(ControlField controlField)
        {
            return controlField.As<UITransform>();
        }
    }
}