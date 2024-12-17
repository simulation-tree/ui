using System;
using Unmanaged;

namespace InteractionKit.Functions
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

        public readonly void Invoke(USpan<char> oldText, ref Allocation newText, ref uint newLength)
        {
            using Allocation container = Allocation.Create<Input.Text>();
            ref Input.Text text = ref container.Read<Input.Text>();
            text.allocation = newText;
            text.length = newLength;
            function(new(oldText, container));
            newText = text.allocation;
            newLength = text.length;
        }

        public static bool operator ==(TextValidation left, TextValidation right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextValidation left, TextValidation right)
        {
            return !(left == right);
        }

        public readonly ref struct Input
        {
            private readonly void* oldText;
            private readonly uint oldLength;
            private readonly Allocation container;

            public readonly USpan<char> PreviousText => new(oldText, oldLength);

            public readonly USpan<char> NewText
            {
                get
                {
                    ref Text text = ref container.Read<Text>();
                    return text.allocation.AsSpan<char>(0, text.length);
                }
            }

            public Input(USpan<char> oldText, Allocation container)
            {
                this.oldText = oldText.Pointer;
                this.oldLength = oldText.Length;
                this.container = container;
            }

            public readonly void SetText(USpan<char> value)
            {
                ref Text text = ref container.Read<Text>();
                text.length = value.Length;
                Allocation.Resize(ref text.allocation, text.length * TypeInfo<char>.size);
                text.allocation.Write(0, value);
            }

            public struct Text
            {
                public Allocation allocation;
                public uint length;
            }
        }
    }
}