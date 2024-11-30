using InteractionKit.Functions;
using System;
using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct IsTrigger
    {
        public TriggerFilter filter;
        public TriggerCallback callback;
        public ulong identifier;

#if NET
        [Obsolete("Default constructor not available", true)]
        public IsTrigger()
        {

        }
#endif

        public IsTrigger(TriggerFilter filter, TriggerCallback callback, ulong identifier = default)
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