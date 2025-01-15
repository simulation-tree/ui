using InteractionKit.Components;
using Simulation.Tests;
using Types;

namespace InteractionKit.Tests
{
    public abstract class InteractionKitTests : SimulationTests
    {
        static InteractionKitTests()
        {
            TypeLayout.Register<ComponentMix>();
            TypeLayout.Register<First>();
            TypeLayout.Register<Second>();
            TypeLayout.Register<Result>();
            TypeLayout.Register<FirstFloat>();
            TypeLayout.Register<SecondFloat>();
            TypeLayout.Register<ResultFloat>();
            TypeLayout.Register<FirstVector>();
            TypeLayout.Register<SecondVector>();
            TypeLayout.Register<ResultVector>();
        }

        protected override void SetUp()
        {
            base.SetUp();
            world.Schema.RegisterComponent<ComponentMix>();
            world.Schema.RegisterComponent<First>();
            world.Schema.RegisterComponent<Second>();
            world.Schema.RegisterComponent<Result>();
            world.Schema.RegisterComponent<FirstFloat>();
            world.Schema.RegisterComponent<SecondFloat>();
            world.Schema.RegisterComponent<ResultFloat>();
            world.Schema.RegisterComponent<FirstVector>();
            world.Schema.RegisterComponent<SecondVector>();
            world.Schema.RegisterComponent<ResultVector>();
        }
    }
}
