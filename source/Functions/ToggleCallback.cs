using System;

namespace InteractionKit.Functions
{
    public unsafe readonly struct ToggleCallback : IEquatable<ToggleCallback>
    {
#if NET
        private readonly delegate* unmanaged<Toggle, Boolean, void> function;

        public ToggleCallback(delegate* unmanaged<Toggle, Boolean, void> function)
        {
            this.function = function;
        }

#else
        private readonly delegate*<Toggle, Boolean, void> function;

        public ToggleCallback(delegate*<Toggle, Boolean, void> function)
        {
            this.function = function;
        }
#endif

        public readonly void Invoke(Toggle toggle, bool newValue)
        {
            function(toggle, newValue);
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is ToggleCallback function && Equals(function);
        }

        public readonly bool Equals(ToggleCallback other)
        {
            return other.GetHashCode() == GetHashCode();
        }

        public readonly override int GetHashCode()
        {
            return ((nint)function).GetHashCode();
        }

        public static bool operator ==(ToggleCallback left, ToggleCallback right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ToggleCallback left, ToggleCallback right)
        {
            return !(left == right);
        }
    }
}