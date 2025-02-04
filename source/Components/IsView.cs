using System.Numerics;
using Worlds;

namespace UI.Components
{
    [Component]
    public struct IsView
    {
        public rint contentReference;
        public Vector2 value;

        public IsView(rint contentReference)
        {
            this.contentReference = contentReference;
        }
    }
}