using UI.Functions;
using Worlds;

namespace UI.Components
{
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