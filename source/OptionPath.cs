using System;
using System.Diagnostics;
using Unmanaged;

namespace UI
{
    public unsafe struct OptionPath
    {
        public const int Capacity = 32;

        public const int MaxDepth = 31;

        private fixed ushort path[(int)MaxDepth];
        private byte depth;

        public readonly ushort this[byte depth] => path[depth];

        public readonly ushort this[int depth]
        {
            get
            {
                ThrowIfTooDeep(depth);

                return path[depth];
            }
        }

        /// <summary>
        /// How deep this option is.
        /// </summary>
        public readonly byte Depth => depth;

        public OptionPath(params Span<ushort> path)
        {
            ThrowIfTooDeep(path.Length);

            depth = (byte)path.Length;
            for (int i = 0; i < path.Length; i++)
            {
                this.path[i] = path[i];
            }
        }

        public OptionPath(ASCIIText256 path)
        {
            System.Span<char> buffer = stackalloc char[path.Length];
            path.CopyTo(buffer);
            CopyFrom(buffer);
        }

        public OptionPath(System.Span<char> path)
        {
            CopyFrom(path);
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
            for (int i = 0; i < depth; i++)
            {
                ushort index = path[i];
                length += index.ToString(destination.Slice(length));
                destination[length] = '/';
                length++;
            }

            if (length > 0)
            {
                length--;
            }

            return length;
        }

        public readonly int CopyTo(Span<ushort> path)
        {
            for (int i = 0; i < depth; i++)
            {
                path[i] = this.path[i];
            }

            return depth;
        }

        public void CopyFrom(Span<char> path)
        {
            int index = 0;
            int start = 0;
            depth = 0;
            while (index < path.Length)
            {
                char c = path[index];
                bool atEnd = index == path.Length - 1;
                if (c == '/' || atEnd)
                {
                    int length = atEnd ? path.Length - index : index - start;
                    if (length > 0)
                    {
                        ThrowIfTooDeep(depth);

                        ReadOnlySpan<char> slice = path.Slice(start, length);
                        this.path[depth] = ushort.Parse(slice);
                        depth++;
                    }

                    start = index + 1;
                }

                index++;
            }
        }

        public readonly OptionPath Append(int value)
        {
            ThrowIfTooDeep(depth);
            OptionPath newPath = this;
            newPath.path[depth] = (ushort)value;
            newPath.depth++;
            return newPath;
        }

        public readonly OptionPath Append(OptionPath path)
        {
            ThrowIfTooDeep((ushort)(depth + path.depth));
            OptionPath newPath = this;
            for (uint i = 0; i < path.depth; i++)
            {
                newPath.path[depth + i] = path.path[i];
            }

            newPath.depth += path.depth;
            return newPath;
        }

        public readonly OptionPath Insert(int index, int value)
        {
            if (index >= depth)
            {
                return Append(value);
            }

            ThrowIfTooDeep(depth);
            OptionPath newPath = this;
            for (int i = depth; i > index; i--)
            {
                newPath.path[i] = newPath.path[i - 1];
            }

            newPath.path[index] = (ushort)value;
            newPath.depth++;
            return newPath;
        }

        public readonly OptionPath Slice(byte start)
        {
            if (start >= depth)
            {
                return default;
            }

            OptionPath newPath = this;
            newPath.depth -= start;
            for (int i = 0; i < newPath.depth; i++)
            {
                newPath.path[i] = path[start + i];
            }

            return newPath;
        }

        [Conditional("DEBUG")]
        private static void ThrowIfTooDeep(int index)
        {
            if (index > MaxDepth)
            {
                throw new IndexOutOfRangeException("Menu option path is too deep");
            }
        }

        public static implicit operator OptionPath(string path)
        {
            return new(path.AsSpan());
        }
    }
}