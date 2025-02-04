using UI.Functions;
using Worlds;

namespace UI.Components
{
    [Component]
    public struct IsDropdown
    {
        public rint labelReference;
        public rint triangleReference;
        public rint menuReference;
        public bool expanded;
        public OptionPath selectedOption;
        public DropdownCallback callback;

        public IsDropdown(rint labelReference, rint triangleReference, rint menuReference, DropdownCallback callback)
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