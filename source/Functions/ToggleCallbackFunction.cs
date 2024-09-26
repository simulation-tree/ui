using System;

namespace InteractionKit.Functions
{
    public unsafe readonly struct ToggleCallbackFunction : IEquatable<ToggleCallbackFunction>
    {
#if NET
        private readonly delegate* unmanaged<Toggle, bool, void> function;

        public ToggleCallbackFunction(delegate* unmanaged<Toggle, bool, void> function)
        {
            this.function = function;
        }

#else
        private readonly delegate*<Toggle, bool, void> function;

        public ToggleCallbackFunction(delegate*<Toggle, bool, void> function)
        {
            this.function = function;
        }
#endif

        public readonly void Invoke(Toggle toggle, bool value)
        {
            function(toggle, value);
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is ToggleCallbackFunction function && Equals(function);
        }

        public readonly bool Equals(ToggleCallbackFunction other)
        {
            return other.GetHashCode() == GetHashCode();
        }

        public readonly override int GetHashCode()
        {
            return ((nint)function).GetHashCode();
        }

        public static bool operator ==(ToggleCallbackFunction left, ToggleCallbackFunction right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ToggleCallbackFunction left, ToggleCallbackFunction right)
        {
            return !(left == right);
        }
    }
}