using InteractionKit.Functions;

namespace InteractionKit.Components
{
    public struct IsMenu
    {
        public MenuCallbackFunction callback;

        public IsMenu(MenuCallbackFunction callback)
        {
            this.callback = callback;
        }
    }
}