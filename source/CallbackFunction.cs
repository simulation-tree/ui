using Simulation;

namespace InteractionKit.Functions
{
    public unsafe readonly struct CallbackFunction
    {
#if NET
        private readonly delegate* unmanaged<World, eint, void> function;

        public CallbackFunction(delegate* unmanaged<World, eint, void> function)
        {
            this.function = function;
        }
#else
        private readonly delegate*<World, eint, uint, void> function;

        public TriggerFunction(delegate*<World, eint, uint, void> function)
        {
            this.function = function;
        }   
#endif

        public readonly void Invoke(World world, eint entity)
        {
            function(world, entity);
        }
    }
}