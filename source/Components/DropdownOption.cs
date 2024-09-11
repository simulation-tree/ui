using Simulation;
using Unmanaged;

namespace InteractionKit.Components
{
    public struct DropdownOption
    {
        public FixedString text;
        public rint buttonReference;
        public rint buttonLabelReference;

        public DropdownOption(FixedString label, rint buttonReference, rint buttonLabelReference)
        {
            this.text = label;
            this.buttonReference = buttonReference;
            this.buttonLabelReference = buttonLabelReference;
        }
    }
}