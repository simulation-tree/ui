using InteractionKit.Functions;
using System;

namespace InteractionKit.Components
{
    public struct Trigger
    {
        public FilterFunction filter;
        public CallbackFunction callback;
        public ulong identifier;

#if NET
        [Obsolete("Default constructor not available", true)]
        public Trigger()
        {

        }
#endif

        public Trigger(FilterFunction filter, CallbackFunction callback, ulong identifier = default)
        {
            this.filter = filter;
            this.callback = callback;
            this.identifier = identifier;
        }
    }
}