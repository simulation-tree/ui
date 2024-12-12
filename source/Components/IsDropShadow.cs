using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public readonly struct IsDropShadow
    {
        public readonly rint meshReference;
        public readonly rint foregroundReference;

        public IsDropShadow(rint meshReference, rint foregroundReference)
        {
            this.meshReference = meshReference;
            this.foregroundReference = foregroundReference;
        }
    }
}