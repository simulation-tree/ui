using InteractionKit.Functions;
using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct IsToggle
    {
        public rint checkmarkReference;
        public bool value;
        public ToggleCallback callback;

        public IsToggle(rint checkmarkReference, bool value, ToggleCallback callback)
        {
            this.checkmarkReference = checkmarkReference;
            this.value = value;
        }
    }
}