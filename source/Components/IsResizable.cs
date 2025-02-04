using Rendering;
using System;
using Worlds;

namespace UI.Components
{
    [Component]
    public struct IsResizable
    {
        public Boundary resize;
        public LayerMask selectionMask;

#if NET
        [Obsolete("Default constructor not supported", true)]
        public IsResizable()
        {
            throw new NotSupportedException();
        }
#endif

        public IsResizable(Boundary resize, LayerMask selectionMask)
        {
            this.resize = resize;
            this.selectionMask = selectionMask;
        }

        public IsResizable(Boundary resize)
        {
            this.resize = resize;
            this.selectionMask = LayerMask.All;
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