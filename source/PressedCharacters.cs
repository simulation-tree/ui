using System;
using System.Diagnostics;

namespace UI
{
    public unsafe struct PressedCharacters : IEquatable<PressedCharacters>
    {
        public const int MaxPressedCharacters = 32;

        private fixed ushort pressedCharacters[MaxPressedCharacters];
        private byte length;

        public readonly uint Length => length;

        public readonly char this[int index]
        {
            get
            {
                ThrowIfOutOfRange(index);

                return (char)pressedCharacters[index];
            }
        }

        public void Clear()
        {
            length = 0;
        }

        public void SetPressedCharacters(ReadOnlySpan<char> characters)
        {
            ThrowIfGreaterThanCapacity(characters.Length);

            fixed (ushort* p = pressedCharacters)
            {
                characters.CopyTo(new(p, MaxPressedCharacters));
            }

            length = (byte)characters.Length;
        }

        public readonly bool Contains(char character)
        {
            for (uint i = 0; i < length; i++)
            {
                if (pressedCharacters[i] == character)
                {
                    return true;
                }
            }

            return false;
        }

        public void Press(char character)
        {
            ThrowIfGreaterThanCapacity(length + 1);

            pressedCharacters[length] = character;
            length++;
        }

        public readonly uint CopyPressedCharactersTo(System.Span<char> destination)
        {
            ThrowIfGreaterThanCapacity(destination.Length);

            fixed (ushort* p = pressedCharacters)
            {
                new Span<char>(p, length).CopyTo(destination);
                return length;
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

            for (uint i = 0; i < length; i++)
            {
                if (pressedCharacters[i] != other.pressedCharacters[i])
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
                for (uint i = 0; i < length; i++)
                {
                    hash = hash * 23 + pressedCharacters[i];
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
    }
}