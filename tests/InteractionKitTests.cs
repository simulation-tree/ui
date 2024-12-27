using InteractionKit.Components;
using Simulation.Tests;
using Worlds;

namespace InteractionKit.Tests
{
    public abstract class InteractionKitTests : SimulationTests
    {
        static InteractionKitTests()
        {
            TypeLayout.Register<ComponentMix>("ComponentMix");
            TypeLayout.Register<First>("First");
            TypeLayout.Register<Second>("Second");
            TypeLayout.Register<Result>("Result");
            TypeLayout.Register<FirstFloat>("FirstFloat");
            TypeLayout.Register<SecondFloat>("SecondFloat");
            TypeLayout.Register<ResultFloat>("ResultFloat");
            TypeLayout.Register<FirstVector>("FirstVector");
            TypeLayout.Register<SecondVector>("SecondVector");
            TypeLayout.Register<ResultVector>("ResultVector");
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
