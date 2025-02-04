using System;
using Worlds;

namespace UI.Functions
{
    public unsafe readonly struct Submit : IEquatable<Submit>
    {
        private readonly delegate* unmanaged<Entity, Settings, Boolean> function;

        public Submit(delegate* unmanaged<Entity, Settings, Boolean> function)
        {
            this.function = function;
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is Submit submit && Equals(submit);
        }

        public readonly bool Equals(Submit other)
        {
            return (nint)function == (nint)other.function;
        }

        public override int GetHashCode()
        {
            return ((nint)function).GetHashCode();
        }

        public readonly Boolean Invoke(Entity entity, Settings settings)
        {
            return function(entity, settings);
        }

        public static bool operator ==(Submit left, Submit right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Submit left, Submit right)
        {
            return !(left == right);
        }
    }
}