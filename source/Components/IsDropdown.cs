using InteractionKit.Functions;
using Simulation;

namespace InteractionKit.Components
{
    public struct IsDropdown
    {
        public rint labelReference;
        public rint triangleReference;
        public bool expanded;
        public uint selectedOption;
        public DropdownCallbackFunction callback;

        public IsDropdown(rint labelReference, rint triangleReference, DropdownCallbackFunction callback)
        {
            this.labelReference = labelReference;
            this.triangleReference = triangleReference;
            this.expanded = false;
            this.selectedOption = 0;
            this.callback = callback;
        }
    }
}