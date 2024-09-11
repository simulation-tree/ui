using Simulation;

namespace InteractionKit.Components
{
    public struct IsDropdown
    {
        public rint labelReference;
        public rint triangleReference;
        public bool expanded;
        public uint selectedOption;

        public IsDropdown(rint labelReference, rint triangleReference)
        {
            this.labelReference = labelReference;
            this.triangleReference = triangleReference;
        }
    }
}