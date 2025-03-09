using System;

namespace UI
{
    public struct TextSelection : IEquatable<TextSelection>
    {
        public int start;
        public int end;
        public int index;

        public readonly int Length
        {
            get
            {
                if (start > end)
                {
                    return start - end;
                }
                else
                {
                    return end - start;
                }
            }
        }

        public TextSelection(int start, int end, int index)
        {
            this.start = start;
            this.end = end;
            this.index = index;
        }

        public readonly override string ToString()
        {
            Span<char> buffer = stackalloc char[64];
            int length = ToString(buffer);
            return buffer.Slice(0, length).ToString();
        }

        public readonly int ToString(Span<char> destination)
        {
            int length = 0;
            destination[length++] = '[';
            length += start.ToString(destination.Slice(length));
            destination[length++] = ',';
            destination[length++] = ' ';
            length += end.ToString(destination.Slice(length));
            destination[length++] = ',';
            destination[length++] = ' ';
            length += index.ToString(destination.Slice(length));
            destination[length++] = ']';
            return length;
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is TextSelection selection && Equals(selection);
        }

        public readonly bool Equals(TextSelection other)
        {
            return start == other.start && end == other.end && index == other.index;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(start, end, index);
        }

        public static bool operator ==(TextSelection left, TextSelection right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextSelection left, TextSelection right)
        {
            return !(left == right);
        }
    }
}