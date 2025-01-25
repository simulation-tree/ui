using InteractionKit.Systems;
using Simulation.Tests;
using Types;
using Worlds;

namespace InteractionKit.Tests
{
    public abstract class InteractionKitTests : SimulationTests
    {
        static InteractionKitTests()
        {
            TypeRegistry.Load<Rendering.Core.TypeBank>();
            TypeRegistry.Load<InteractionKit.TypeBank>();
            TypeRegistry.Load<Automations.TypeBank>();
            TypeRegistry.Load<Transforms.TypeBank>();
            TypeRegistry.Load<Meshes.TypeBank>();
            TypeRegistry.Load<Data.Core.TypeBank>();
            TypeRegistry.Load<Fonts.TypeBank>();
            TypeRegistry.Load<Textures.TypeBank>();
            TypeRegistry.Load<Cameras.TypeBank>();
            TypeRegistry.Load<TextRendering.TypeBank>();
            TypeRegistry.Load<InteractionKit.Tests.TypeBank>();
        }
        protected override void SetUp()
        {
            base.SetUp();
            simulator.AddSystem<ComponentMixingSystem>();
            simulator.AddSystem<InvokeTriggersSystem>();
        }

        protected override Schema CreateSchema()
        {
            Schema schema = base.CreateSchema();
            schema.Load<Rendering.Core.SchemaBank>();
            schema.Load<InteractionKit.SchemaBank>();
            schema.Load<Automations.SchemaBank>();
            schema.Load<Transforms.SchemaBank>();
            schema.Load<Meshes.SchemaBank>();
            schema.Load<Data.Core.SchemaBank>();
            schema.Load<Fonts.SchemaBank>();
            schema.Load<Textures.SchemaBank>();
            schema.Load<Cameras.SchemaBank>();
            schema.Load<TextRendering.SchemaBank>();
            schema.Load<InteractionKit.Tests.SchemaBank>();
            return schema;
        }
    }
}
