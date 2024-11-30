using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct FontSettings
    {
        public rint fontReference;

        public FontSettings(rint fontReference)
        {
            this.fontReference = fontReference;
        }
    }
}
