using UI.Functions;

namespace UI.Components
{
    public struct IsLabelProcessor
    {
        public TryProcessLabel function;

        public IsLabelProcessor(TryProcessLabel function)
        {
            this.function = function;
        }
    }
}