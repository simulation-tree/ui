using InteractionKit.Functions;
using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct IsMenu
    {
        public MenuCallback callback;

        public IsMenu(MenuCallback callback)
        {
            this.callback = callback;
        }
    }
}