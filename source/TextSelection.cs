using Unmanaged;

namespace InteractionKit
{
    public struct TextSelection
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
            return buffer.Slice(0, length).ToString();
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
    }
}