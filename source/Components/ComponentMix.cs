using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct ComponentMix
    {
        public ComponentType left;
        public ComponentType right;
        public ComponentType output;
        public byte vectorLength;
        public Operation operation;

        public ComponentMix(ComponentType left, ComponentType right, ComponentType output, Operation operation, byte vectorLength = 1)
        {
            this.left = left;
            this.right = right;
            this.output = output;
            this.operation = operation;
            this.vectorLength = vectorLength;
        }

        public static ComponentMix Create<L, R, O>(Operation operation, byte vectorLength = 1) where L : unmanaged where R : unmanaged where O : unmanaged
        {
            return new ComponentMix(ComponentType.Get<L>(), ComponentType.Get<R>(), ComponentType.Get<O>(), operation, vectorLength);
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