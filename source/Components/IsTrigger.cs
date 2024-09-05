using InteractionKit.Functions;
using System;

namespace InteractionKit.Components
{
    public struct IsTrigger
    {
        public FilterFunction filter;
        public CallbackFunction callback;
        public ulong identifier;

#if NET
        [Obsolete("Default constructor not available", true)]
        public IsTrigger()
        {

        }
#endif

        public IsTrigger(FilterFunction filter, CallbackFunction callback, ulong identifier = default)
        {
            this.filter = filter;
            this.callback = callback;
            this.identifier = identifier;
        }

        public readonly override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + filter.GetHashCode();
                hash = hash * 23 + callback.GetHashCode();
                return hash;
            }
        }
    }
}