using System;
using System.Numerics;
using Unmanaged;

namespace UI
{
    public struct UIBounds : IEquatable<UIBounds>
    {
        public Vector2 min;
        public Vector2 max;

        public Vector2 Center
        {
            readonly get => (min + max) * 0.5f;
            set
            {
                Vector2 halfSize = Size * 0.5f;
                min = value - halfSize;
                max = value + halfSize;
            }
        }

        public Vector2 Size
        {
            readonly get => max - min;
            set => max = min + value;
        }

        public UIBounds(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;
        }

        public readonly bool Contains(Vector2 point)
        {
            return point.X >= min.X && point.X <= max.X && point.Y >= min.Y && point.Y <= max.Y;
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
            length += min.ToString(buffer.Slice(length));
            buffer[length++] = ',';
            buffer[length++] = ' ';
            length += max.ToString(buffer.Slice(length));
            buffer[length++] = ']';
            return length;
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is UIBounds bounds && Equals(bounds);
        }

        public readonly bool Equals(UIBounds other)
        {
            return min.Equals(other.min) && max.Equals(other.max);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(min, max);
        }

        public static UIBounds CreateFromCenterSize(Vector2 center, Vector2 size)
        {
            Vector2 halfSize = size * 0.5f;
            return new UIBounds(center - halfSize, center + halfSize);
        }

        public static bool operator ==(UIBounds left, UIBounds right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UIBounds left, UIBounds right)
        {
            return !(left == right);
        }
    }
}