using System;

namespace InteractionKit.Functions
{
    public unsafe readonly struct DropdownCallback : IEquatable<DropdownCallback>
    {
#if NET
        private readonly delegate* unmanaged<Dropdown, uint, uint, void> function;

        public DropdownCallback(delegate* unmanaged<Dropdown, uint, uint, void> function)
        {
            this.function = function;
        }
#else
        private readonly delegate*<Dropdown, uint, uint, void> function;

        public DropdownCallback(delegate*<Dropdown, uint, uint, void> function)
        {
            this.function = function;
        }
#endif

        public readonly void Invoke(Dropdown dropdown, uint previousOption, uint currentOption)
        {
            function(dropdown, previousOption, currentOption);
        }

        public readonly override int GetHashCode()
        {
            return ((nint)function).GetHashCode();
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is DropdownCallback function && Equals(function);
        }

        public readonly bool Equals(DropdownCallback other)
        {
            return ((nint)function) == ((nint)other.function);
        }

        public static bool operator ==(DropdownCallback left, DropdownCallback right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DropdownCallback left, DropdownCallback right)
        {
            return !(left == right);
        }
    }
}