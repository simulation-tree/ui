using System;

namespace UI.Functions
{
    public unsafe readonly struct DropdownCallback : IEquatable<DropdownCallback>
    {
#if NET
        private readonly delegate* unmanaged<Input, void> function;

        public DropdownCallback(delegate* unmanaged<Input, void> function)
        {
            this.function = function;
        }
#else
        private readonly delegate*<Input, void> function;

        public DropdownCallback(delegate*<Input, void> function)
        {
            this.function = function;
        }
#endif

        public readonly void Invoke(Dropdown dropdown, int previousOption, int currentOption)
        {
            function(new(dropdown, previousOption, currentOption));
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

        public readonly struct Input
        {
            public readonly Dropdown dropdown;
            public readonly int previousOption;
            public readonly int currentOption;

            public Input(Dropdown dropdown, int previousOption, int currentOption)
            {
                this.dropdown = dropdown;
                this.previousOption = previousOption;
                this.currentOption = currentOption;
            }
        }
    }
}