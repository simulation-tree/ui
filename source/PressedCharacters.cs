using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UI
{
    public struct PressedCharacters : IEquatable<PressedCharacters>
    {
        public const int MaxPressedCharacters = 32;

        private PressedCharactersBuffer buffer;
        private byte length;

        public readonly int Length => length;

        public readonly char this[int index]
        {
            get
            {
                ThrowIfOutOfRange(index);

                return buffer[index];
            }
        }

        public void Clear()
        {
            length = 0;
        }

        public void SetPressedCharacters(ReadOnlySpan<char> characters)
        {
            ThrowIfGreaterThanCapacity(characters.Length);

            for (int i = 0; i < characters.Length; i++)
            {
                buffer[i] = characters[i];
            }

            length = (byte)characters.Length;
        }

        public readonly bool Contains(char character)
        {
            for (int i = 0; i < length; i++)
            {
                if (buffer[i] == character)
                {
                    return true;
                }
            }

            return false;
        }

        public void Press(char character)
        {
            ThrowIfGreaterThanCapacity(length + 1);

            buffer[length] = character;
            length++;
        }

        public readonly void CopyPressedCharactersTo(Span<char> destination)
        {
            ThrowIfGreaterThanCapacity(destination.Length);

            for (int i = 0; i < length; i++)
            {
                destination[i] = buffer[i];
            }
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfOutOfRange(int index)
        {
            if (index >= length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Value cannot be greater than or equal to {length}");
            }
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is PressedCharacters characters && Equals(characters);
        }

        public readonly bool Equals(PressedCharacters other)
        {
            if (length != other.length)
            {
                return false;
            }

            for (int i = 0; i < length; i++)
            {
                if (buffer[i] != other.buffer[i])
                {
                    return false;
                }
            }

            return true;
        }

        public readonly override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < length; i++)
                {
                    hash = hash * 23 + buffer[i];
                }

                return hash;
            }
        }

        [Conditional("DEBUG")]
        private static void ThrowIfGreaterThanCapacity(int count)
        {
            if (count > MaxPressedCharacters)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, $"Value cannot be greater than {MaxPressedCharacters}");
            }
        }

        public static bool operator ==(PressedCharacters left, PressedCharacters right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PressedCharacters left, PressedCharacters right)
        {
            return !(left == right);
        }

#if NET
        [InlineArray(MaxPressedCharacters)]
        private struct PressedCharactersBuffer
        {
            private char element0;
        }
#else
        private unsafe struct PressedCharactersBuffer
        {
            private fixed char buffer[MaxPressedCharacters];

            public char this[int index]
            {
                get => buffer[index];
                set => buffer[index] = value;
            }
        }
#endif
    }
}