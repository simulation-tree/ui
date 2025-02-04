using Worlds;

namespace UI.Components
{
    [ArrayElement]
    public struct MaterialSettings
    {
        public rint cameraReference;
        public rint squareMaterialReference;
        public rint triangleMaterialReference;
        public rint textMaterialReference;
        public rint dropShadowMaterialReference;

        public MaterialSettings(rint cameraReference, rint squareMaterialReference, rint triangleMaterialReference, rint textMaterialReference, rint dropShadowMaterialReference)
        {
            this.cameraReference = cameraReference;
            this.squareMaterialReference = squareMaterialReference;
            this.triangleMaterialReference = triangleMaterialReference;
            this.textMaterialReference = textMaterialReference;
            this.dropShadowMaterialReference = dropShadowMaterialReference;
        }
    }
}
