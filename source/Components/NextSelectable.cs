using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct NextSelectable
    {
        public rint reference;

        public NextSelectable(rint reference)
        {
            this.reference = reference;
        }
    }
}