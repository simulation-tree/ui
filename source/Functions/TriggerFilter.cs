using Unmanaged;
using Worlds;

namespace InteractionKit.Functions
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

        public readonly void Invoke(USpan<Entity> entities, ulong userData)
        {
            Input input = new(entities, userData);
            function(input);
        }

        public readonly override int GetHashCode()
        {
            return ((nint)function).GetHashCode();
        }

        public readonly struct Input
        {
            public readonly ulong userData;

            private readonly nint address;
            private readonly uint length;

            /// <summary>
            /// All entities containing the same filter, callback and identifier combinations.
            /// </summary>
            public readonly USpan<Entity> Entities => new(address, length);

            public Input(USpan<Entity> entities, ulong userData)
            {
                this.userData = userData;
                this.address = entities.Address;
                length = entities.Length;
            }
        }
    }
}