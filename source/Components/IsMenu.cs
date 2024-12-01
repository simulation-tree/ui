using InteractionKit.Functions;
using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct IsMenu
    {
        public MenuCallbackFunction callback;

        public IsMenu(MenuCallbackFunction callback)
        {
            this.callback = callback;
        }
    }
}