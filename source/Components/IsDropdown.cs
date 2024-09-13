using InteractionKit.Functions;
using Simulation;

namespace InteractionKit.Components
{
    public struct IsDropdown
    {
        public rint labelReference;
        public rint triangleReference;
        public rint menuReference;
        public bool expanded;
        public MenuOptionPath selectedOption;
        public DropdownCallbackFunction callback;

        public IsDropdown(rint labelReference, rint triangleReference, rint menuReference, DropdownCallbackFunction callback)
        {
            this.labelReference = labelReference;
            this.triangleReference = triangleReference;
            this.menuReference = menuReference;
            this.expanded = false;
            this.selectedOption = default;
            this.callback = callback;
        }
    }
}