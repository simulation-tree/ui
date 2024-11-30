using InteractionKit.Functions;
using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct IsToggle
    {
        public rint checkmarkReference;
        public bool value;
        public ToggleCallbackFunction callback;

        public IsToggle(rint checkmarkReference, bool value, ToggleCallbackFunction callback)
        {
            this.checkmarkReference = checkmarkReference;
            this.value = value;
        }
    }
}