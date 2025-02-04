using Automations;
using Cameras;
using Data;
using Fonts;
using Materials;
using Meshes;
using Rendering;
using Simulation.Tests;
using TextRendering;
using Textures;
using Transforms;
using Types;
using UI.Systems;
using Worlds;

namespace UI.Tests
{
    public abstract class UITests : SimulationTests
    {
        static UITests()
        {
            TypeRegistry.Load<RenderingTypeBank>();
            TypeRegistry.Load<MaterialsTypeBank>();
            TypeRegistry.Load<UITypeBank>();
            TypeRegistry.Load<AutomationsTypeBank>();
            TypeRegistry.Load<TransformsTypeBank>();
            TypeRegistry.Load<MeshesTypeBank>();
            TypeRegistry.Load<DataTypeBank>();
            TypeRegistry.Load<FontsTypeBank>();
            TypeRegistry.Load<TexturesTypeBank>();
            TypeRegistry.Load<CamerasTypeBank>();
            TypeRegistry.Load<TextRenderingTypeBank>();
            TypeRegistry.Load<UITestsTypeBank>();
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
            schema.Load<MaterialsSchemaBank>();
            schema.Load<UISchemaBank>();
            schema.Load<AutomationsSchemaBank>();
            schema.Load<TransformsSchemaBank>();
            schema.Load<MeshesSchemaBank>();
            schema.Load<DataSchemaBank>();
            schema.Load<FontsSchemaBank>();
            schema.Load<TexturesSchemaBank>();
            schema.Load<CamerasSchemaBank>();
            schema.Load<TextRenderingSchemaBank>();
            schema.Load<UITestsSchemaBank>();
            return schema;
        }
    }
}
