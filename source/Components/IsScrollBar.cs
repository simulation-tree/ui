using System.Numerics;
using Worlds;

namespace UI.Components
{
    public struct IsScrollBar
    {
        public rint scrollHandleReference;
        public Vector2 value;
        public Vector2 axis;

        public IsScrollBar(rint scrollHandleReference, Vector2 axis)
        {
            this.scrollHandleReference = scrollHandleReference;
            this.axis = axis;
        }
    }
}