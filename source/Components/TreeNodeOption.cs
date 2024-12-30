using Worlds;

namespace InteractionKit.Components
{
    [ArrayElement]
    public struct TreeNodeOption
    {
        public rint childNodeReference;

        public TreeNodeOption(rint childNodeReference)
        {
            this.childNodeReference = childNodeReference;
        }
    }
}