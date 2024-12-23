using InteractionKit.Functions;
using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct IsLabelProcessor
    {
        public TryProcessLabel function;

        public IsLabelProcessor(TryProcessLabel function)
        {
            this.function = function;
        }
    }
}