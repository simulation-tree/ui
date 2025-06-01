using System;
using Worlds;

namespace UI.Functions
{
    public unsafe readonly struct TriggerFilter
    {
#if NET
        private readonly delegate* unmanaged<Input, void> function;

        public TriggerFilter(delegate* unmanaged<Input, void> function)
        {
            this.function = function;
        }
#else
        private readonly delegate*<Input, void> function;

        public TriggerFilter(delegate*<Input, void> function)
        {
            this.function = function;
        }
#endif

        public readonly void Invoke(World world, Span<uint> entities, ulong userData)
        {
            function(new(world, entities, userData));
        }

        public readonly override int GetHashCode()
        {
            return ((nint)function).GetHashCode();
        }

        public readonly struct Input
        {
            public readonly World world;
            public readonly ulong userData;

            private readonly uint* entities;
            private readonly int length;

            /// <summary>
            /// All entities containing the same filter, callback and identifier combinations.
            /// </summary>
            public readonly Span<uint> Entities => new(entities, length);

            public Input(World world, Span<uint> entities, ulong userData)
            {
                this.world = world;
                this.userData = userData;
                this.entities = entities.GetPointer();
                length = entities.Length;
            }
        }
    }
}