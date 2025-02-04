using UI.Functions;
using System.Numerics;
using Worlds;

namespace UI.Components
{
    [Component]
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