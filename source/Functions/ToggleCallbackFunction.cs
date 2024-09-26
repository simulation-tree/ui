using System;

namespace InteractionKit.Functions
{
    public unsafe readonly struct ToggleCallbackFunction : IEquatable<ToggleCallbackFunction>
    {
#if NET
        private readonly delegate* unmanaged<Toggle, byte, void> function;

        public ToggleCallbackFunction(delegate* unmanaged<Toggle, byte, void> function)
        {
            this.function = function;
        }

#else
        private readonly delegate*<Toggle, byte, void> function;

        public ToggleCallbackFunction(delegate*<Toggle, byte, void> function)
        {
            this.function = function;
        }
#endif

        public readonly void Invoke(Toggle toggle, bool value)
        {
            function(toggle, value ? (byte)1 : (byte)0);
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