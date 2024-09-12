using Simulation;
using Unmanaged;

namespace InteractionKit.Components
{
    public struct MenuOption
    {
        public FixedString text;
        public rint buttonReference;
        public rint buttonLabelReference;
        public rint childMenuReference;

        public MenuOption(FixedString label, rint buttonReference, rint buttonLabelReference, rint childMenuReference)
        {
            this.text = label;
            this.buttonReference = buttonReference;
            this.buttonLabelReference = buttonLabelReference;
            this.childMenuReference = childMenuReference;
        }
    }
}