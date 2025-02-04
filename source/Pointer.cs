using UI.Components;
using Rendering;
using System.Numerics;
using Worlds;

namespace UI
{
    public readonly partial struct Pointer : IEntity
    {
        /// <summary>
        /// Position of the pointer in screen space.
        /// </summary>
        public readonly ref Vector2 Position => ref GetComponent<IsPointer>().position;

        public readonly bool HasPrimaryIntent
        {
            get => GetComponent<IsPointer>().HasPrimaryIntent;
            set => GetComponent<IsPointer>().HasPrimaryIntent = value;
        }

        public readonly bool HasSecondaryIntent
        {
            get => GetComponent<IsPointer>().HasSecondaryIntent;
            set => GetComponent<IsPointer>().HasSecondaryIntent = value;
        }

        public readonly ref LayerMask SelectionMask => ref GetComponent<IsPointer>().selectionMask;

        /// <summary>
        /// The currently hovered over entity.
        /// <para>May be <c>default</c>.</para>
        /// </summary>
        public readonly Entity HoveringOver
        {
            get
            {
                rint hoveringOverReference = GetComponent<IsPointer>().hoveringOverReference;
                if (hoveringOverReference == default)
                {
                    return default;
                }

                uint hoveringOverEntity = GetReference(hoveringOverReference);
                return new Entity(world, hoveringOverEntity);
            }
            set
            {
                ref IsPointer pointer = ref GetComponent<IsPointer>();
                if (pointer.hoveringOverReference == default)
                {
                    pointer.hoveringOverReference = AddReference(value);
                }
                else
                {
                    SetReference(pointer.hoveringOverReference, value);
                }
            }
        }

        public readonly ref Vector2 Scroll => ref GetComponent<IsPointer>().scroll;

        public Pointer(World world)
        {
            this.world = world;
            value = world.CreateEntity(new IsPointer(default, LayerMask.All));
        }

        public Pointer(World world, LayerMask selectionMask)
        {
            this.world = world;
            value = world.CreateEntity(new IsPointer(default, selectionMask));
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsPointer>();
        }
    }
}