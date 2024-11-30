using Worlds;

namespace InteractionKit.Components
{
    [Array]
    public struct TreeNodeOption
    {
        public rint childNodeReference;

        public TreeNodeOption(rint childNodeReference)
        {
            this.childNodeReference = childNodeReference;
        }
    }
}