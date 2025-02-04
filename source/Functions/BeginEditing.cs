using System;
using Worlds;

namespace UI.Functions
{
    public unsafe readonly struct BeginEditing : IEquatable<BeginEditing>
    {
        private readonly delegate* unmanaged<Entity, void> function;

        public BeginEditing(delegate* unmanaged<Entity, void> function)
        {
            this.function = function;
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is BeginEditing editing && Equals(editing);
        }

        public readonly bool Equals(BeginEditing other)
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

        public static bool operator ==(BeginEditing left, BeginEditing right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BeginEditing left, BeginEditing right)
        {
            return !(left == right);
        }
    }
}