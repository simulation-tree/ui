using System;
using Worlds;

namespace InteractionKit.Functions
{
    public unsafe readonly struct TriggerCallback : IEquatable<TriggerCallback>
    {
#if NET
        private readonly delegate* unmanaged<Entity, void> function;

        public TriggerCallback(delegate* unmanaged<Entity, void> function)
        {
            this.function = function;
        }
#else
        private readonly delegate*<Entity, void> function;

        public TriggerCallback(delegate*<Entity, void> function)
        {
            this.function = function;
        }   
#endif

        public readonly void Invoke(Entity entity)
        {
            function(entity);
        }

        public readonly override int GetHashCode()
        {
            return ((nint)function).GetHashCode();
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is TriggerCallback function && Equals(function);
        }

        public readonly bool Equals(TriggerCallback other)
        {
            return GetHashCode() == other.GetHashCode();
        }

        public static bool operator ==(TriggerCallback left, TriggerCallback right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TriggerCallback left, TriggerCallback right)
        {
            return !(left == right);
        }
    }
}