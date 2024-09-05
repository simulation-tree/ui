using Unmanaged;

namespace InteractionKit.Components
{
    public struct ComponentMix
    {
        public RuntimeType left;
        public RuntimeType right;
        public RuntimeType output;
        public byte partCount;
        public Operation operation;

        public ComponentMix(RuntimeType left, RuntimeType right, RuntimeType output, Operation operation, byte partCount = 1)
        {
            this.left = left;
            this.right = right;
            this.output = output;
            this.operation = operation;
            this.partCount = partCount;
        }

        public static ComponentMix Create<L, R, O>(Operation operation, byte partCount = 1) where L : unmanaged where R : unmanaged where O : unmanaged
        {
            return new ComponentMix(RuntimeType.Get<L>(), RuntimeType.Get<R>(), RuntimeType.Get<O>(), operation, partCount);
        }

        public enum Operation : byte
        {
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