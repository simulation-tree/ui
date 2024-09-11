using System;

namespace InteractionKit.Functions
{
    public unsafe readonly struct DropdownCallbackFunction : IEquatable<DropdownCallbackFunction>
    {
#if NET
        private readonly delegate* unmanaged<Dropdown, uint, uint, void> function;

        public DropdownCallbackFunction(delegate* unmanaged<Dropdown, uint, uint, void> function)
        {
            this.function = function;
        }
#else
        private readonly delegate*<Dropdown, uint, uint, void> function;

        public DropdownCallbackFunction(delegate*<Dropdown, uint, uint, void> function)
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
            return obj is DropdownCallbackFunction function && Equals(function);
        }

        public readonly bool Equals(DropdownCallbackFunction other)
        {
            return ((nint)function) == ((nint)other.function);
        }

        public static bool operator ==(DropdownCallbackFunction left, DropdownCallbackFunction right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DropdownCallbackFunction left, DropdownCallbackFunction right)
        {
            return !(left == right);
        }
    }
}