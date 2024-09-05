using Simulation;

namespace InteractionKit.Functions
{
    public unsafe readonly struct CallbackFunction
    {
#if NET
        private readonly delegate* unmanaged<World, uint, void> function;

        public CallbackFunction(delegate* unmanaged<World, uint, void> function)
        {
            this.function = function;
        }
#else
        private readonly delegate*<World, uint, uint, void> function;

        public TriggerFunction(delegate*<World, uint, uint, void> function)
        {
            this.function = function;
        }   
#endif

        public readonly void Invoke(World world, uint entity)
        {
            function(world, entity);
        }

        public readonly override int GetHashCode()
        {
            return ((nint)function).GetHashCode();
        }
    }
}