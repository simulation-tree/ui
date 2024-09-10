using Simulation;
using System.Numerics;

namespace InteractionKit.Components
{
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