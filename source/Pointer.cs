using InteractionKit.Components;
using Simulation;
using System;
using System.Numerics;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct Pointer : IEntity
    {
        public readonly Entity entity;

        /// <summary>
        /// Position of the pointer in screen space.
        /// </summary>
        public readonly ref Vector2 Position => ref entity.GetComponentRef<IsPointer>().position;

        public readonly bool HasPrimaryIntent
        {
            get => entity.GetComponent<IsPointer>().HasPrimaryIntent;
            set => entity.GetComponentRef<IsPointer>().HasPrimaryIntent = value;
        }

        public readonly bool HasSecondaryIntent
        {
            get => entity.GetComponent<IsPointer>().HasSecondaryIntent;
            set => entity.GetComponentRef<IsPointer>().HasSecondaryIntent = value;
        }

        /// <summary>
        /// The currently selected entity.
        /// <para>Can be <c>default</c>.</para>
        /// </summary>
        public readonly Entity Selected
        {
            get
            {
                rint selectedReference = entity.GetComponent<IsPointer>().selectedReference;
                uint selectedEntity = entity.GetReference(selectedReference);
                return new Entity(entity.world, selectedEntity);
            }
            set
            {
                ref IsPointer pointer = ref entity.GetComponentRef<IsPointer>();
                if (pointer.selectedReference == default)
                {
                    pointer.selectedReference = entity.AddReference(value);
                }
                else
                {
                    entity.SetReference(pointer.selectedReference, value);
                }
            }
        }

        public readonly ref Vector2 Scroll => ref entity.GetComponentRef<IsPointer>().scroll;

        readonly uint IEntity.Value => entity.value;
        readonly World IEntity.World => entity.world;
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsPointer>()], []);

#if NET
        [Obsolete("Default constructor not available", true)]
        public Pointer()
        {
            throw new NotSupportedException();
        }
#endif

        public Pointer(World world)
        {
            entity = new(world);
            entity.AddComponent(new IsPointer());
        }
    }
}