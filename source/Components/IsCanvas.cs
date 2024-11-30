using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct IsCanvas
    {
        public rint cameraReference;

        public IsCanvas(rint cameraReference)
        {
            this.cameraReference = cameraReference;
        }
    }
}