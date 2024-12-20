using System;
using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct IsResizable
    {
        public Boundary resize;

        public IsResizable(Boundary resize)
        {
            this.resize = resize;
        }

        [Flags]
        public enum Boundary : byte
        {
            None = 0,
            Right = 1,
            Left = 2,
            Top = 4,
            Bottom = 8,
            TopLeft = Top | Left,
            TopRight = Top | Right,
            BottomLeft = Bottom | Left,
            BottomRight = Bottom | Right,
            All = Top | Bottom | Left | Right
        }
    }
}