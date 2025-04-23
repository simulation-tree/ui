using System;
using Unmanaged;

namespace UI.Components
{
    public struct IsToken
    {
        public long hash;

        public IsToken(ReadOnlySpan<char> value)
        {
            hash = value.GetLongHashCode();
        }

        public void Set(ReadOnlySpan<char> value)
        {
            hash = value.GetLongHashCode();
        }
    }
}