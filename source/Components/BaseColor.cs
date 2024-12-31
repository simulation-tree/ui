using System.Numerics;
using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct BaseColor
    {
        public Vector4 value;

        public BaseColor(Vector4 value)
        {
            this.value = value;
        }
    }
}