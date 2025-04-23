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
            MetadataRegistry.Load<RenderingMetadataBank>();
            MetadataRegistry.Load<MaterialsMetadataBank>();
            MetadataRegistry.Load<UIMetadataBank>();
            MetadataRegistry.Load<AutomationsMetadataBank>();
            MetadataRegistry.Load<TransformsMetadataBank>();
            MetadataRegistry.Load<MeshesMetadataBank>();
            MetadataRegistry.Load<DataMetadataBank>();
            MetadataRegistry.Load<FontsMetadataBank>();
            MetadataRegistry.Load<TexturesMetadataBank>();
            MetadataRegistry.Load<CamerasMetadataBank>();
            MetadataRegistry.Load<TextRenderingMetadataBank>();
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
