using Collections;
using System;
using System.Diagnostics;
using Worlds;

namespace InteractionKit.Functions
{
    public readonly unsafe struct InitializeControlField
    {
        private readonly delegate* unmanaged<Input, void> function;

        public InitializeControlField(delegate* unmanaged<Input, void> function)
        {
            this.function = function;
        }

        public readonly void Invoke(List<Entity> createdEntities, ControlField controlField, Canvas canvas, Entity target, byte typeIndex, bool isComponentType, uint offset)
        {
            World world = controlField.GetWorld();
            function(new(world, createdEntities, controlField.GetEntityValue(), canvas.GetEntityValue(), target.GetEntityValue(), typeIndex, isComponentType, offset));
        }

        public readonly struct Input
        {
            public readonly World world;
            public readonly uint offset;
            public readonly uint controlField;
            public readonly uint canvas;
            public readonly uint target;
            public readonly Boolean isComponentType;

            private readonly byte typeIndex;
            private readonly List<Entity> createdEntities;

            public readonly ControlField ControlField => new(world, controlField);
            public readonly Canvas Canvas => new(world, canvas);
            public readonly Entity Target => new(world, target);

            public readonly ComponentType ComponentType
            {
                get
                {
                    ThrowIfNotComponentType();

                    return new(typeIndex);
                }
            }

            public readonly ArrayElementType ArrayElementType
            {
                get
                {
                    ThrowIfNotArrayElementType();

                    return new(typeIndex);
                }
            }

            public Input(World world, List<Entity> createdEntities, uint controlField, uint canvas, uint target, byte typeIndex, bool isComponentType, uint offset)
            {
                this.world = world;
                this.createdEntities = createdEntities;
                this.controlField = controlField;
                this.canvas = canvas;
                this.target = target;
                this.typeIndex = typeIndex;
                this.isComponentType = isComponentType;
                this.offset = offset;
            }

            public readonly void AddEntity(Entity entity)
            {
                createdEntities.Add(entity);
            }

            [Conditional("DEBUG")]
            private readonly void ThrowIfNotComponentType()
            {
                if (!isComponentType)
                {
                    throw new InvalidOperationException("Target is an array type, not a component type");
                }
            }

            [Conditional("DEBUG")]
            private readonly void ThrowIfNotArrayElementType()
            {
                if (isComponentType)
                {
                    throw new InvalidOperationException("Target is a component type, not an array type");
                }
            }
        }
    }
}