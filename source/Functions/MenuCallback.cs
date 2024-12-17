using System;

namespace InteractionKit.Functions
{
    public unsafe readonly struct MenuCallback : IEquatable<MenuCallback>
    {
#if NET
        private readonly delegate* unmanaged<MenuOption, void> function;

        public MenuCallback(delegate* unmanaged<MenuOption, void> function)
        {
            this.function = function;
        }
#else
        private readonly delegate*<MenuOption, void> function;

        public MenuCallback(delegate*<MenuOption, void> function)
        {
            this.function = function;
        }
#endif

        public readonly void Invoke(MenuOption option)
        {
            function(option);
        }

        public readonly override int GetHashCode()
        {
            return ((nint)function).GetHashCode();
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is MenuCallback function && Equals(function);
        }

        public readonly bool Equals(MenuCallback other)
        {
            return ((nint)function) == ((nint)other.function);
        }

        public static bool operator ==(MenuCallback left, MenuCallback right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MenuCallback left, MenuCallback right)
        {
            return !(left == right);
        }
    }
}