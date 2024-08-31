using Simulation;
using System;

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

        public readonly void Invoke(World world, Span<uint> entities)
        {
            Input input = new(world, entities);
            function(input);
        }

        public readonly struct Input
        {
            public readonly World world;

            private readonly nint address;
            private readonly int length;

            public readonly Span<uint> Entities => new((uint*)address, length);

            public Input(World world, Span<uint> entities)
            {
                this.world = world;
                fixed (uint* entity = entities)
                {
                    address = (nint)entity;
                }

                length = entities.Length;
            }
        }
    }
}