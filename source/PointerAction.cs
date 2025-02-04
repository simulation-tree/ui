using System;

namespace UI
{
    [Flags]
    public enum PointerAction : byte
    {
        None = 0,
        Primary = 1,
        Secondary = 2
    }
}