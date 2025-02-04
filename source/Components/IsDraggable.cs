using Worlds;

namespace UI.Components
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