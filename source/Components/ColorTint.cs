using System.Numerics;
using Worlds;

namespace UI.Components
{
    [Component]
    public struct ColorTint
    {
        public Vector4 value;

        public ColorTint(Vector4 value)
        {
            this.value = value;
        }
    }
}