using System;
using Unmanaged;

namespace UI.Functions
{
    public unsafe readonly struct TryProcessLabel
    {
        private readonly delegate* unmanaged<Input, Bool> function;

        public TryProcessLabel(delegate* unmanaged<Input, Bool> function)
        {
            this.function = function;
        }

        public readonly bool Invoke(Span<char> originalText, Text result)
        {
            return function(new(originalText, result));
        }

        public readonly struct Input
        {
            private readonly char* input;
            private readonly int inputLength;
            private readonly Text result;

            public readonly ReadOnlySpan<char> OriginalText => new(input, inputLength);

            public Input(Span<char> originalText, Text result)
            {
                this.input = originalText.GetPointer();
                this.inputLength = originalText.Length;
                this.result = result;
            }

            public readonly void SetResult(Span<char> newText)
            {
                result.CopyFrom(newText);
            }
        }
    }
}