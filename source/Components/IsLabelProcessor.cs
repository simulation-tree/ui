using UI.Functions;
using Worlds;

namespace UI.Components
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