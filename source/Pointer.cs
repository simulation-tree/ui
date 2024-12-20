using InteractionKit.Components;
using System;
using System.Numerics;
using Worlds;

namespace InteractionKit
{
    public readonly struct Pointer : IEntity
    {
        private readonly Entity entity;

        /// <summary>
        /// Position of the pointer in screen space.
        /// </summary>
        public readonly ref Vector2 Position => ref entity.GetComponent<IsPointer>().position;

        public readonly bool HasPrimaryIntent
        {
            get => entity.GetComponent<IsPointer>().HasPrimaryIntent;
            set => entity.GetComponent<IsPointer>().HasPrimaryIntent = value;
        }

        public readonly bool HasSecondaryIntent
        {
            get => entity.GetComponent<IsPointer>().HasSecondaryIntent;
            set => entity.GetComponent<IsPointer>().HasSecondaryIntent = value;
        }

        /// <summary>
        /// The currently hovered over entity.
        /// <para>May be <c>default</c>.</para>
        /// </summary>
        public readonly Entity HoveringOver
        {
            get
            {
                rint hoveringOverReference = entity.GetComponent<IsPointer>().hoveringOverReference;
                if (hoveringOverReference == default)
                {
                    return default;
                }

                uint hoveringOverEntity = entity.GetReference(hoveringOverReference);
                return new Entity(entity.world, hoveringOverEntity);
            }
            set
            {
                ref IsPointer pointer = ref entity.GetComponent<IsPointer>();
                if (pointer.hoveringOverReference == default)
                {
                    pointer.hoveringOverReference = entity.AddReference(value);
                }
                else
                {
                    entity.SetReference(pointer.hoveringOverReference, value);
                }
            }
        }

        public readonly ref Vector2 Scroll => ref entity.GetComponent<IsPointer>().scroll;

        readonly uint IEntity.Value => entity.value;
        readonly World IEntity.World => entity.world;
        readonly Definition IEntity.Definition => new Definition().AddComponentType<IsPointer>();

#if NET
        [Obsolete("Default constructor not available", true)]
        public Pointer()
        {
            throw new NotSupportedException();
        }
#endif

        public Pointer(World world, uint existingEntity)
        {
            entity = new(world, existingEntity);
        }

        public Pointer(World world)
        {
            entity = new Entity<IsPointer>(world);
        }

        public readonly void Dispose()
        {
            entity.Dispose();
        }

        public static implicit operator Entity(Pointer pointer)
        {
            return pointer.entity;
        }
    }
}