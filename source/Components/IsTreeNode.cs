using Unmanaged;
using Worlds;

namespace UI.Components
{
    public struct IsTreeNode
    {
        public ASCIIText256 text;
        public rint boxReference;
        public rint labelReference;
        public bool expanded;

        public IsTreeNode(ASCIIText256 text, rint boxReference, rint labelReference)
        {
            this.text = text;
            this.boxReference = boxReference;
            this.labelReference = labelReference;
            expanded = default;
        }
    }
}