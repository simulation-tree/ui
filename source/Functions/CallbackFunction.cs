using Simulation;
using System;

namespace InteractionKit.Functions
{
    public unsafe readonly struct CallbackFunction : IEquatable<CallbackFunction>
    {
#if NET
        private readonly delegate* unmanaged<Entity, void> function;

        public CallbackFunction(delegate* unmanaged<Entity, void> function)
        {
            this.function = function;
        }
#else
        private readonly delegate*<Entity, void> function;

        public TriggerFunction(delegate*<Entity, void> function)
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
            return obj is CallbackFunction function && Equals(function);
        }

        public readonly bool Equals(CallbackFunction other)
        {
            return GetHashCode() == other.GetHashCode();
        }

        public static bool operator ==(CallbackFunction left, CallbackFunction right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CallbackFunction left, CallbackFunction right)
        {
            return !(left == right);
        }
    }
}