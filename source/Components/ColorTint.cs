using Data;
using System.Numerics;
using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct ColorTint
    {
        public Color value;

        public ColorTint(Color value)
        {
            this.value = value;
        }

        public ColorTint(Vector4 value)
        {
            this.value = new(value);
        }
    }
}