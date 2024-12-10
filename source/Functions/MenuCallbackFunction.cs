using System;

namespace InteractionKit.Functions
{
    public unsafe readonly struct MenuCallbackFunction : IEquatable<MenuCallbackFunction>
    {
#if NET
        private readonly delegate* unmanaged<MenuOption, void> function;

        public MenuCallbackFunction(delegate* unmanaged<MenuOption, void> function)
        {
            this.function = function;
        }
#else
        private readonly delegate*<MenuOption, void> function;

        public MenuCallbackFunction(delegate*<MenuOption, void> function)
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
            return obj is MenuCallbackFunction function && Equals(function);
        }

        public readonly bool Equals(MenuCallbackFunction other)
        {
            return ((nint)function) == ((nint)other.function);
        }

        public static bool operator ==(MenuCallbackFunction left, MenuCallbackFunction right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MenuCallbackFunction left, MenuCallbackFunction right)
        {
            return !(left == right);
        }
    }
}