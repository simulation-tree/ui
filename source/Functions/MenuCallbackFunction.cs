using System;

namespace InteractionKit.Functions
{
    public unsafe readonly struct MenuCallbackFunction : IEquatable<MenuCallbackFunction>
    {
#if NET
        private readonly delegate* unmanaged<Menu, uint, void> function;

        public MenuCallbackFunction(delegate* unmanaged<Menu, uint, void> function)
        {
            this.function = function;
        }
#else
        private readonly delegate*<Menu, uint, void> function;

        public MenuCallbackFunction(delegate*<Menu, uint, void> function)
        {
            this.function = function;
        }
#endif

        public readonly void Invoke(Menu menu, uint selected)
        {
            function(menu, selected);
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