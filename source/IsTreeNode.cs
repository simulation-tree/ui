using Simulation;
using Unmanaged;

namespace InteractionKit.Components
{
    public struct IsTreeNode
    {
        public FixedString text;
        public rint labelReference;
        public InteractiveContext context;
        public bool expanded;

        public IsTreeNode(FixedString text, rint labelReference, InteractiveContext context)
        {
            this.text = text;
            this.labelReference = labelReference;
            this.context = context;
            expanded = default;
        }
    }
}