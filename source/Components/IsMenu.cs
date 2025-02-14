using System.Numerics;
using UI.Functions;

namespace UI.Components
{
    public struct IsMenu
    {
        public Vector2 optionSize;
        public MenuCallback callback;

        public IsMenu(Vector2 optionSize, MenuCallback callback)
        {
            this.optionSize = optionSize;
            this.callback = callback;
        }
    }
}