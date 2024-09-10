using Simulation;

namespace InteractionKit.Components
{
    public struct IsToggle
    {
        public rint checkmarkReference;
        public bool value;

        public IsToggle(rint checkmarkReference, bool value)
        {
            this.checkmarkReference = checkmarkReference;
            this.value = value;
        }
    }
}