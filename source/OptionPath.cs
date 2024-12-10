using System;
using System.Diagnostics;
using Unmanaged;

namespace InteractionKit
{
    public unsafe struct OptionPath
    {
        public const uint MaxDepth = 32;

        private fixed ushort path[(int)MaxDepth];
        private byte depth;

        public readonly ushort this[byte depth] => path[depth];

        /// <summary>
        /// How deep this option is.
        /// </summary>
        public readonly byte Depth => depth;

        public OptionPath(USpan<ushort> path)
        {
            ThrowIfTooDeep((ushort)path.Length);
            depth = (byte)path.Length;
            for (uint i = 0; i < path.Length; i++)
            {
                this.path[i] = path[i];
            }
        }

        public readonly override string ToString()
        {
            USpan<char> buffer = stackalloc char[64];
            uint length = ToString(buffer);
            return buffer.Slice(0, length).ToString();
        }

        public readonly byte ToString(USpan<char> buffer)
        {
            byte length = 0;
            for (uint i = 0; i < depth; i++)
            {
                ushort index = path[i];
                length += (byte)index.ToString(buffer.Slice(length));
                buffer[length] = '/';
                length++;
            }

            if (length > 0)
            {
                length--;
            }

            return length;
        }

        public readonly uint CopyTo(USpan<ushort> path)
        {
            for (uint i = 0; i < depth; i++)
            {
                path[i] = this.path[i];
            }

            return depth;
        }

        public readonly OptionPath Append(uint value)
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

        public readonly OptionPath Insert(byte index, uint value)
        {
            if (index >= depth)
            {
                return Append(value);
            }

            ThrowIfTooDeep(depth);
            OptionPath newPath = this;
            for (uint i = depth; i > index; i--)
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
            for (uint i = 0; i < newPath.depth; i++)
            {
                newPath.path[i] = path[start + i];
            }

            return newPath;
        }

        [Conditional("DEBUG")]
        private static void ThrowIfTooDeep(ushort index)
        {
            if (index >= MaxDepth)
            {
                throw new IndexOutOfRangeException("Menu option path is too deep");
            }
        }
    }
}