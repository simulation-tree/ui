using Unmanaged;

namespace InteractionKit.Functions
{
    public unsafe readonly struct TryProcessLabel
    {
        private readonly delegate* unmanaged<Input, Boolean> function;

        public TryProcessLabel(delegate* unmanaged<Input, Boolean> function)
        {
            this.function = function;
        }

        public readonly bool Invoke(USpan<char> originalText, Text result)
        {
            return function(new(originalText, result));
        }

        public readonly struct Input
        {
            private readonly char* input;
            private readonly uint inputLength;
            private readonly Text result;

            public readonly USpan<char> OriginalText => new(input, inputLength);

            public Input(USpan<char> originalText, Text result)
            {
                this.input = originalText.Pointer;
                this.inputLength = originalText.Length;
                this.result = result;
            }

            public readonly void SetResult(USpan<char> newText)
            {
                result.CopyFrom(newText);
            }
        }
    }
}