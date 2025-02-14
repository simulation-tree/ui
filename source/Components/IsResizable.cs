using Rendering;
using System;

namespace UI.Components
{
    public struct IsResizable
    {
        public EdgeMask edgeMask;
        public LayerMask selectionMask;

#if NET
        [Obsolete("Default constructor not supported", true)]
        public IsResizable()
        {
            throw new NotSupportedException();
        }
#endif

        public IsResizable(EdgeMask edgeMask, LayerMask selectionMask)
        {
            this.edgeMask = edgeMask;
            this.selectionMask = selectionMask;
        }

        public IsResizable(EdgeMask edgeMask)
        {
            this.edgeMask = edgeMask;
            this.selectionMask = LayerMask.All;
        }

        [Flags]
        public enum EdgeMask : byte
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