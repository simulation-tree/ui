using System;

namespace UI.Functions
{
    public unsafe readonly struct VirtualWindowClose : IEquatable<VirtualWindowClose>
    {
#if NET
        private readonly delegate* unmanaged<VirtualWindow, void> function;

        public VirtualWindowClose(delegate* unmanaged<VirtualWindow, void> function)
        {
            this.function = function;
        }
#else
        private readonly delegate*<VirtualWindow, void> function;

        public VirtualWindowClose(delegate*<VirtualWindow, void> function)
        {
            this.function = function;
        }
#endif

        public readonly void Invoke(VirtualWindow window)
        {
            function(window);
        }

        public readonly override int GetHashCode()
        {
            return ((nint)function).GetHashCode();
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is VirtualWindowClose close && Equals(close);
        }

        public readonly bool Equals(VirtualWindowClose other)
        {
            return (nint)function == (nint)other.function;
        }

        public static bool operator ==(VirtualWindowClose left, VirtualWindowClose right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VirtualWindowClose left, VirtualWindowClose right)
        {
            return !(left == right);
        }
    }
}