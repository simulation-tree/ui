using System;
using Unmanaged;

namespace UI
{
    public struct TextSelection : IEquatable<TextSelection>
    {
        public uint start;
        public uint end;
        public uint index;

        public readonly uint Length
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

        public TextSelection(uint start, uint end, uint index)
        {
            this.start = start;
            this.end = end;
            this.index = index;
        }

        public readonly override string ToString()
        {
            USpan<char> buffer = stackalloc char[64];
            uint length = ToString(buffer);
            return buffer.GetSpan(length).ToString();
        }

        public readonly uint ToString(USpan<char> buffer)
        {
            uint length = 0;
            buffer[length++] = '[';
            length += start.ToString(buffer.Slice(length));
            buffer[length++] = ',';
            buffer[length++] = ' ';
            length += end.ToString(buffer.Slice(length));
            buffer[length++] = ',';
            buffer[length++] = ' ';
            length += index.ToString(buffer.Slice(length));
            buffer[length++] = ']';
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