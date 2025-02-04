namespace UI
{
    public readonly struct Boolean : System.IEquatable<Boolean>
    {
        private readonly byte value;

        public Boolean(bool value)
        {
            this.value = value ? (byte)1 : (byte)0;
        }

        public readonly override string ToString()
        {
            return value == 1 ? "True" : "False";
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is Boolean boolean && Equals(boolean);
        }

        public readonly bool Equals(Boolean other)
        {
            return value == other.value;
        }

        public readonly override int GetHashCode()
        {
            return value;
        }

        public static bool operator ==(Boolean left, Boolean right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Boolean left, Boolean right)
        {
            return !(left == right);
        }

        public static implicit operator bool(Boolean boolean)
        {
            return boolean.value == 1;
        }

        public static implicit operator Boolean(bool value)
        {
            return new(value);
        }
    }
}