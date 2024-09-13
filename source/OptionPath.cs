using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unmanaged;

namespace InteractionKit
{
    [StructLayout(LayoutKind.Sequential, Size = (int)MaxDepth * sizeof(ushort))]
    public unsafe struct OptionPath
    {
        public const uint MaxDepth = 32;

        private fixed ushort path[(int)MaxDepth];
        private byte length;

        public readonly ushort this[uint index] => path[index];
        public readonly byte Length => length;

        public OptionPath(USpan<ushort> path)
        {
            ThrowIfTooDeep((ushort)path.length);
            length = (byte)path.length;
            for (uint i = 0; i < path.length; i++)
            {
                this.path[i] = path[i];
            }
        }

        public readonly override string ToString()
        {
            USpan<char> buffer = stackalloc char[64];
            uint length = ToString(buffer);
            return new(buffer.pointer, 0, (int)length);
        }

        public readonly uint ToString(USpan<char> buffer)
        {
            uint length = 0;
            for (uint i = 0; i < this.length; i++)
            {
                ushort index = path[i];
                length += index.ToString(buffer.Slice(length));
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
            for (uint i = 0; i < length; i++)
            {
                path[i] = this.path[i];
            }

            return length;
        }

        public readonly OptionPath Append(uint value)
        {
            ThrowIfTooDeep(length);
            OptionPath newPath = this;
            newPath.path[length] = (ushort)value;
            newPath.length++;
            return newPath;
        }

        public readonly OptionPath Append(OptionPath path)
        {
            ThrowIfTooDeep((ushort)(length + path.length));
            OptionPath newPath = this;
            for (uint i = 0; i < path.length; i++)
            {
                newPath.path[length + i] = path.path[i];
            }

            newPath.length += path.length;
            return newPath;
        }

        public readonly OptionPath Insert(uint index, uint value)
        {
            if (index >= length)
            {
                return Append(value);
            }

            ThrowIfTooDeep(length);
            OptionPath newPath = this;
            for (uint i = length; i > index; i--)
            {
                newPath.path[i] = newPath.path[i - 1];
            }

            newPath.path[index] = (ushort)value;
            newPath.length++;
            return newPath;
        }

        public readonly OptionPath Slice(uint start)
        {
            if (start >= length)
            {
                return default;
            }

            OptionPath newPath = this;
            newPath.length -= (byte)start;
            for (uint i = 0; i < newPath.length; i++)
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
                throw new IndexOutOfRangeException("Menu option path is too deep.");
            }
        }
    }
}