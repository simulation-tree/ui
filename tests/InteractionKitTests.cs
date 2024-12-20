using InteractionKit.Components;
using Simulation.Tests;
using Worlds;

namespace InteractionKit.Tests
{
    public abstract class InteractionKitTests : SimulationTests
    {
        protected override void SetUp()
        {
            base.SetUp();
            ComponentType.Register<ComponentMix>();
            ComponentType.Register<First>();
            ComponentType.Register<Second>();
            ComponentType.Register<Result>();
            ComponentType.Register<FirstFloat>();
            ComponentType.Register<SecondFloat>();
            ComponentType.Register<ResultFloat>();
            ComponentType.Register<FirstVector>();
            ComponentType.Register<SecondVector>();
            ComponentType.Register<ResultVector>();
        }
    }
}
