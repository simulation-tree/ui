using Worlds;

namespace UI.Components
{
    public struct ComponentMix
    {
        public DataType left;
        public DataType right;
        public DataType output;
        public byte vectorLength;
        public Operation operation;

        public ComponentMix(DataType left, DataType right, DataType output, Operation operation, byte vectorLength = 1)
        {
            this.left = left;
            this.right = right;
            this.output = output;
            this.operation = operation;
            this.vectorLength = vectorLength;
        }

        public static ComponentMix Create<L, R, O>(Operation operation, byte vectorLength, Schema schema) where L : unmanaged where R : unmanaged where O : unmanaged
        {
            DataType left = schema.GetComponentDataType<L>();
            DataType right = schema.GetComponentDataType<R>();
            DataType output = schema.GetComponentDataType<O>();
            return new ComponentMix(left, right, output, operation, vectorLength);
        }

        public enum Operation : byte
        {
            Unknown,
            UnsignedAdd,
            UnsignedSubtract,
            UnsignedMultiply,
            UnsignedDivide,
            SignedAdd,
            SignedSubtract,
            SignedMultiply,
            SignedDivide,
            FloatingAdd,
            FloatingSubtract,
            FloatingMultiply,
            FloatingDivide
        }
    }
}