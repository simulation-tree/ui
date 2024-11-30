using Unmanaged;
using Worlds;

namespace InteractionKit.Components
{
    [Array]
    public struct MenuOption
    {
        public FixedString text;
        public rint buttonReference;
        public rint buttonLabelReference;
        public rint childMenuReference;
        public bool expanded;

        public MenuOption(FixedString label, rint buttonReference, rint buttonLabelReference, rint childMenuReference)
        {
            this.text = label;
            this.buttonReference = buttonReference;
            this.buttonLabelReference = buttonLabelReference;
            this.childMenuReference = childMenuReference;
            this.expanded = default;
        }
    }
}