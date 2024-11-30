using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct IsDraggable
    {
        public rint targetReference;

        public IsDraggable(rint targetReference)
        {
            this.targetReference = targetReference;
        }
    }
}