using Simulation;
using Unmanaged;

namespace InteractionKit.Functions
{
    public unsafe readonly struct FilterFunction
    {
#if NET
        private readonly delegate* unmanaged<Input, void> function;

        public FilterFunction(delegate* unmanaged<Input, void> function)
        {
            this.function = function;
        }
#else
        private readonly delegate*<Input, void> function;

        public SystemFunction(delegate*<Input, void> function)
        {
            this.function = function;
        }
#endif

        public readonly void Invoke(World world, USpan<uint> entities, ulong identifier)
        {
            Input input = new(world, entities, identifier);
            function(input);
        }

        public readonly override int GetHashCode()
        {
            return ((nint)function).GetHashCode();
        }

        public readonly struct Input
        {
            public readonly World world;
            public readonly ulong identifier;

            private readonly void* entities;
            private readonly uint length;

            /// <summary>
            /// All entities containing the same filter, callback and identifier combinations.
            /// </summary>
            public readonly USpan<uint> Entities => new(entities, length);

            public Input(World world, USpan<uint> entities, ulong identifier)
            {
                this.world = world;
                this.identifier = identifier;
                this.entities = entities.pointer;
                length = entities.Length;
            }
        }
    }
}