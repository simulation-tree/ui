using InteractionKit.Functions;
using System;

namespace InteractionKit.Components
{
    public struct Trigger
    {
        public FilterFunction filter;
        public CallbackFunction callback;

#if NET
        [Obsolete("Default constructor not available", true)]
        public Trigger()
        {

        }
#endif

        public Trigger(FilterFunction filter, CallbackFunction callback)
        {
            this.filter = filter;
            this.callback = callback;
        }
    }
}