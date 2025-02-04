using System;
using Worlds;

namespace UI.Functions
{
    public unsafe readonly struct Cancel : IEquatable<Cancel>
    {
        private readonly delegate* unmanaged<Entity, void> function;

        public Cancel(delegate* unmanaged<Entity, void> function)
        {
            this.function = function;
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is Cancel cancel && Equals(cancel);
        }

        public readonly bool Equals(Cancel other)
        {
            return (nint)function == (nint)other.function;
        }

        public readonly override int GetHashCode()
        {
            return ((nint)function).GetHashCode();
        }

        public readonly void Invoke(Entity entity)
        {
            function(entity);
        }

        public static bool operator ==(Cancel left, Cancel right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Cancel left, Cancel right)
        {
            return !(left == right);
        }
    }
}