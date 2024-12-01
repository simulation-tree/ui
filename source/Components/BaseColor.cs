using Data;
using System.Numerics;
using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct BaseColor
    {
        public Color value;

        public BaseColor(Color value)
        {
            this.value = value;
        }

        public BaseColor(Vector4 value)
        {
            this.value = new(value);
        }
    }
}