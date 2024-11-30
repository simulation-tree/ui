using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct MeshSettings
    {
        public rint quadMeshReference;

        public MeshSettings(rint quadMeshReference)
        {
            this.quadMeshReference = quadMeshReference;
        }
    }
}
