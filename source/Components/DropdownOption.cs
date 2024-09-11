using Simulation;
using Unmanaged;

namespace InteractionKit.Components
{
    public struct DropdownOption
    {
        public FixedString label;
        public rint buttonReference;
        public rint buttonLabelReference;

        public DropdownOption(FixedString label, rint buttonReference, rint buttonLabelReference)
        {
            this.label = label;
            this.buttonReference = buttonReference;
            this.buttonLabelReference = buttonLabelReference;
        }
    }
}