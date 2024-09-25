using Unmanaged;

namespace InteractionKit.Components
{
    public struct ComponentMix
    {
        public RuntimeType left;
        public RuntimeType right;
        public RuntimeType output;
        public byte vectorLength;
        public Operation operation;

        public ComponentMix(RuntimeType left, RuntimeType right, RuntimeType output, Operation operation, byte vectorLength = 1)
        {
            this.left = left;
            this.right = right;
            this.output = output;
            this.operation = operation;
            this.vectorLength = vectorLength;
        }

        public static ComponentMix Create<L, R, O>(Operation operation, byte vectorLength = 1) where L : unmanaged where R : unmanaged where O : unmanaged
        {
            return new ComponentMix(RuntimeType.Get<L>(), RuntimeType.Get<R>(), RuntimeType.Get<O>(), operation, vectorLength);
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