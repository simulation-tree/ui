using Automations;
using Cameras;
using Data;
using Fonts;
using Materials;
using Meshes;
using Rendering;
using TextRendering;
using Textures;
using Transforms;
using Types;
using Worlds;
using Worlds.Tests;

namespace UI.Tests
{
    public abstract class UITests : WorldTests
    {
        static UITests()
        {
            MetadataRegistry.Load<RenderingTypeBank>();
            MetadataRegistry.Load<MaterialsTypeBank>();
            MetadataRegistry.Load<UITypeBank>();
            MetadataRegistry.Load<AutomationsTypeBank>();
            MetadataRegistry.Load<TransformsTypeBank>();
            MetadataRegistry.Load<MeshesTypeBank>();
            MetadataRegistry.Load<DataTypeBank>();
            MetadataRegistry.Load<FontsTypeBank>();
            MetadataRegistry.Load<TexturesTypeBank>();
            MetadataRegistry.Load<CamerasTypeBank>();
            MetadataRegistry.Load<TextRenderingTypeBank>();
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
            return schema;
        }
    }
}
