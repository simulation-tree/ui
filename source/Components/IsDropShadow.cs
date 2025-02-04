using Worlds;

namespace UI.Components
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