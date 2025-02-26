using Collections.Generic;
using System;
using System.Diagnostics;
using Worlds;

namespace UI.Functions
{
    public readonly unsafe struct InitializeControlField
    {
        private readonly delegate* unmanaged<Input, void> function;

        public InitializeControlField(delegate* unmanaged<Input, void> function)
        {
            this.function = function;
        }

        public readonly void Invoke(List<Entity> createdEntities, ControlField controlField, Canvas canvas, Entity target, DataType dataType, uint offset)
        {
            World world = controlField.world;
            function(new(world, createdEntities, controlField.value, canvas.value, target.value, dataType, offset));
        }

        public readonly struct Input
        {
            public readonly World world;
            public readonly uint offset;
            public readonly uint controlField;
            public readonly uint canvas;
            public readonly uint target;
            public readonly DataType dataType;

            private readonly List<Entity> createdEntities;

            public readonly ControlField ControlField => new Entity(world, controlField).As<ControlField>();
            public readonly Canvas Canvas => new Entity(world, canvas).As<Canvas>();
            public readonly Entity Target => new(world, target);

            public readonly ComponentType ComponentType
            {
                get
                {
                    ThrowIfNotComponentType();

                    return dataType.ComponentType;
                }
            }

            public readonly ArrayElementType ArrayElementType
            {
                get
                {
                    ThrowIfNotArrayElementType();

                    return dataType.ArrayElementType;
                }
            }

            public Input(World world, List<Entity> createdEntities, uint controlField, uint canvas, uint target, DataType dataType, uint offset)
            {
                this.world = world;
                this.createdEntities = createdEntities;
                this.controlField = controlField;
                this.canvas = canvas;
                this.target = target;
                this.dataType = dataType;
                this.offset = offset;
            }

            public readonly void AddEntity(Entity entity)
            {
                createdEntities.Add(entity);
            }

            [Conditional("DEBUG")]
            private readonly void ThrowIfNotComponentType()
            {
                if (!dataType.IsComponent)
                {
                    throw new InvalidOperationException("Target is an array type, not a component type");
                }
            }

            [Conditional("DEBUG")]
            private readonly void ThrowIfNotArrayElementType()
            {
                if (!dataType.IsArrayElement)
                {
                    throw new InvalidOperationException("Target is a component type, not an array type");
                }
            }
        }
    }
}