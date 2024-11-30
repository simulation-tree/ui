using Worlds;

namespace InteractionKit.Components
{
    [Array]
    public struct MaterialSettings
    {
        public rint cameraReference;
        public rint squareMaterialReference;
        public rint triangleMaterialReference;
        public rint textMaterialReference;

        public MaterialSettings(rint cameraReference, rint squareMaterialReference, rint triangleMaterialReference, rint textMaterialReference)
        {
            this.cameraReference = cameraReference;
            this.squareMaterialReference = squareMaterialReference;
            this.triangleMaterialReference = triangleMaterialReference;
            this.textMaterialReference = textMaterialReference;
        }
    }
}
