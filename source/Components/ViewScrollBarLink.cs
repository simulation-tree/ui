using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct ViewScrollBarLink
    {
        public rint scrollBarReference;

        public ViewScrollBarLink(rint scrollBarReference)
        {
            this.scrollBarReference = scrollBarReference;
        }
    }
}