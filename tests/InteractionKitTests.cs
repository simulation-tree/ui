using Automations;
using Cameras;
using Data;
using Fonts;
using InteractionKit.Systems;
using Meshes;
using Rendering;
using Simulation.Tests;
using TextRendering;
using Textures;
using Transforms;
using Types;
using Worlds;

namespace InteractionKit.Tests
{
    public abstract class InteractionKitTests : SimulationTests
    {
        static InteractionKitTests()
        {
            TypeRegistry.Load<RenderingTypeBank>();
            TypeRegistry.Load<InteractionKitTypeBank>();
            TypeRegistry.Load<AutomationsTypeBank>();
            TypeRegistry.Load<TransformsTypeBank>();
            TypeRegistry.Load<MeshesTypeBank>();
            TypeRegistry.Load<DataTypeBank>();
            TypeRegistry.Load<FontsTypeBank>();
            TypeRegistry.Load<TexturesTypeBank>();
            TypeRegistry.Load<CamerasTypeBank>();
            TypeRegistry.Load<TextRenderingTypeBank>();
            TypeRegistry.Load<InteractionKitTestsTypeBank>();
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
            schema.Load<RenderingSchemaBank>();
            schema.Load<InteractionKitSchemaBank>();
            schema.Load<AutomationsSchemaBank>();
            schema.Load<TransformsSchemaBank>();
            schema.Load<MeshesSchemaBank>();
            schema.Load<DataSchemaBank>();
            schema.Load<FontsSchemaBank>();
            schema.Load<TexturesSchemaBank>();
            schema.Load<CamerasSchemaBank>();
            schema.Load<TextRenderingSchemaBank>();
            schema.Load<InteractionKitTestsSchemaBank>();
            return schema;
        }
    }
}
