using Worlds;

namespace UI.Components
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