using Unmanaged;
using Worlds;

namespace UI.Components
{
    [ArrayElement]
    public struct IsMenuOption
    {
        public FixedString text;
        public rint buttonReference;
        public rint buttonLabelReference;
        public rint childMenuReference;
        public bool expanded;

        public readonly bool HasChildMenu => childMenuReference != default;

        public IsMenuOption(FixedString label, rint buttonReference, rint buttonLabelReference, rint childMenuReference)
        {
            this.text = label;
            this.buttonReference = buttonReference;
            this.buttonLabelReference = buttonLabelReference;
            this.childMenuReference = childMenuReference;
            this.expanded = default;
        }
    }
}