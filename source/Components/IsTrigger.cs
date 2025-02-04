using UI.Functions;
using System;
using Worlds;

namespace UI.Components
{
    [Component]
    public struct IsTrigger
    {
        public TriggerFilter filter;
        public TriggerCallback callback;
        public ulong userData;

#if NET
        [Obsolete("Default constructor not available", true)]
        public IsTrigger()
        {

        }
#endif

        public IsTrigger(TriggerFilter filter, TriggerCallback callback, ulong userData = default)
        {
            this.filter = filter;
            this.callback = callback;
            this.userData = userData;
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