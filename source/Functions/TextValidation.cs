using System;
using Unmanaged;

namespace UI.Functions
{
    public unsafe readonly struct TextValidation : IEquatable<TextValidation>
    {
        private readonly delegate* unmanaged<Input, void> function;

        public TextValidation(delegate* unmanaged<Input, void> function)
        {
            this.function = function;
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is TextValidation validation && Equals(validation);
        }

        public readonly bool Equals(TextValidation other)
        {
            return (nint)function == (nint)other.function;
        }

        public readonly override int GetHashCode()
        {
            return ((nint)function).GetHashCode();
        }

        public readonly void Invoke(USpan<char> oldText, Text newText)
        {
            function(new(oldText, newText));
        }

        public static bool operator ==(TextValidation left, TextValidation right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextValidation left, TextValidation right)
        {
            return !(left == right);
        }

        public readonly struct Input
        {
            private readonly char* oldText;
            private readonly uint oldLength;
            private readonly Text newText;

            public readonly USpan<char> PreviousText => new(oldText, oldLength);
            public readonly USpan<char> NewText => newText.AsSpan();

            public Input(USpan<char> oldText, Text newText)
            {
                this.oldText = oldText.Pointer;
                this.oldLength = oldText.Length;
                this.newText = newText;
            }

            public readonly void SetNewText(USpan<char> newText)
            {
                this.newText.SetLength(newText.Length);
                newText.CopyTo(this.newText.AsSpan());
            }
        }
    }
}