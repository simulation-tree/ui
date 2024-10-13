using InteractionKit.Components;
using InteractionKit.Events;
using Simulation;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unmanaged;
using Unmanaged.Collections;

namespace InteractionKit.Systems
{
    public unsafe class ComponentMixingSystem : SystemBase
    {
        private readonly ComponentQuery<ComponentMix> query;
        private readonly UnmanagedList<Request> requests;
        private readonly UnmanagedArray<MixFunction> functions;

        public unsafe ComponentMixingSystem(World world) : base(world)
        {
            query = new();
            requests = new();
            functions = new(13);
            functions[(byte)ComponentMix.Operation.UnsignedAdd] = new(&UnsignedAdd);
            functions[(byte)ComponentMix.Operation.UnsignedSubtract] = new(&UnsignedSubtract);
            functions[(byte)ComponentMix.Operation.UnsignedMultiply] = new(&UnsignedMultiply);
            functions[(byte)ComponentMix.Operation.UnsignedDivide] = new(&UnsignedDivide);
            functions[(byte)ComponentMix.Operation.SignedAdd] = new(&SignedAdd);
            functions[(byte)ComponentMix.Operation.SignedSubtract] = new(&SignedSubtract);
            functions[(byte)ComponentMix.Operation.SignedMultiply] = new(&SignedMultiply);
            functions[(byte)ComponentMix.Operation.SignedDivide] = new(&SignedDivide);
            functions[(byte)ComponentMix.Operation.FloatingAdd] = new(&FloatingAdd);
            functions[(byte)ComponentMix.Operation.FloatingSubtract] = new(&FloatingSubtract);
            functions[(byte)ComponentMix.Operation.FloatingMultiply] = new(&FloatingMultiply);
            functions[(byte)ComponentMix.Operation.FloatingDivide] = new(&FloatingDivide);
            Subscribe<MixingUpdate>(Update);
        }

        public override void Dispose()
        {
            Unsubscribe<MixingUpdate>();
            functions.Dispose();
            requests.Dispose();
            query.Dispose();
            base.Dispose();
        }

        private void Update(MixingUpdate update)
        {
            query.Update(world);
            foreach (var x in query)
            {
                uint entity = x.entity;
                ComponentMix mix = x.Component1;
                requests.Add(new(entity, mix));
            }

            MixComponents(requests.AsSpan());
            requests.Clear();
        }

        private void MixComponents(USpan<Request> requests)
        {
            foreach (var request in requests)
            {
                uint entity = request.entity;
                ComponentMix mix = request.mix;
                RuntimeType leftType = mix.left;
                RuntimeType rightType = mix.right;
                RuntimeType outputType = mix.output;
                ThrowIfComponentIsMissing(entity, leftType);
                ThrowIfComponentIsMissing(entity, rightType);
                ThrowIfComponentSizesDontMatch(leftType, rightType);
                ThrowIfComponentSizesDontMatch(leftType, outputType);
                if (!world.ContainsComponent(entity, outputType))
                {
                    world.AddComponent(entity, outputType);
                }

                USpan<byte> leftBytes = world.GetComponentBytes(entity, leftType);
                USpan<byte> rightBytes = world.GetComponentBytes(entity, rightType);
                USpan<byte> outputBytes = world.GetComponentBytes(entity, outputType);
                byte partCount = mix.vectorLength;
                uint partSize = (uint)(leftType.Size / partCount);
                for (uint i = 0; i < partCount; i++)
                {
                    USpan<byte> leftPart = leftBytes.Slice(i * partSize, partSize);
                    USpan<byte> rightPart = rightBytes.Slice(i * partSize, partSize);
                    USpan<byte> outputPart = outputBytes.Slice(i * partSize, partSize);
                    MixFunction function = functions[(byte)mix.operation];
                    function.Invoke(leftPart, rightPart, outputPart);
                }
            }
        }

        //todo: efficiency: the branches for different sizes can be optimized away by having more functions
        [UnmanagedCallersOnly]
        private static void UnsignedAdd(MixFunction.Input input)
        {
            unchecked
            {
                byte length = input.size;
                if (length == 1)
                {
                    ref byte leftPart = ref input.GetLeft<byte>();
                    ref byte rightPart = ref input.GetRight<byte>();
                    ref byte outputPart = ref input.GetOutput<byte>();
                    outputPart = (byte)(leftPart + rightPart);
                }
                else if (length == 2)
                {
                    ref ushort leftPart = ref input.GetLeft<ushort>();
                    ref ushort rightPart = ref input.GetRight<ushort>();
                    ref ushort outputPart = ref input.GetOutput<ushort>();
                    outputPart = (ushort)(leftPart + rightPart);
                }
                else if (length == 4)
                {
                    ref uint leftPart = ref input.GetLeft<uint>();
                    ref uint rightPart = ref input.GetRight<uint>();
                    ref uint outputPart = ref input.GetOutput<uint>();
                    outputPart = leftPart + rightPart;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void SignedAdd(MixFunction.Input input)
        {
            unchecked
            {
                byte length = input.size;
                if (length == 1)
                {
                    ref sbyte leftPart = ref input.GetLeft<sbyte>();
                    ref sbyte rightPart = ref input.GetRight<sbyte>();
                    ref sbyte outputPart = ref input.GetOutput<sbyte>();
                    outputPart = (sbyte)(leftPart + rightPart);
                }
                else if (length == 2)
                {
                    ref short leftPart = ref input.GetLeft<short>();
                    ref short rightPart = ref input.GetRight<short>();
                    ref short outputPart = ref input.GetOutput<short>();
                    outputPart = (short)(leftPart + rightPart);
                }
                else if (length == 4)
                {
                    ref int leftPart = ref input.GetLeft<int>();
                    ref int rightPart = ref input.GetRight<int>();
                    ref int outputPart = ref input.GetOutput<int>();
                    outputPart = leftPart + rightPart;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void UnsignedSubtract(MixFunction.Input input)
        {
            unchecked
            {
                byte length = input.size;
                if (length == 1)
                {
                    ref byte leftPart = ref input.GetLeft<byte>();
                    ref byte rightPart = ref input.GetRight<byte>();
                    ref byte outputPart = ref input.GetOutput<byte>();
                    outputPart = (byte)(leftPart - rightPart);
                }
                else if (length == 2)
                {
                    ref ushort leftPart = ref input.GetLeft<ushort>();
                    ref ushort rightPart = ref input.GetRight<ushort>();
                    ref ushort outputPart = ref input.GetOutput<ushort>();
                    outputPart = (ushort)(leftPart - rightPart);
                }
                else if (length == 4)
                {
                    ref uint leftPart = ref input.GetLeft<uint>();
                    ref uint rightPart = ref input.GetRight<uint>();
                    ref uint outputPart = ref input.GetOutput<uint>();
                    outputPart = leftPart - rightPart;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void SignedSubtract(MixFunction.Input input)
        {
            unchecked
            {
                byte length = input.size;
                if (length == 1)
                {
                    ref sbyte leftPart = ref input.GetLeft<sbyte>();
                    ref sbyte rightPart = ref input.GetRight<sbyte>();
                    ref sbyte outputPart = ref input.GetOutput<sbyte>();
                    outputPart = (sbyte)(leftPart - rightPart);
                }
                else if (length == 2)
                {
                    ref short leftPart = ref input.GetLeft<short>();
                    ref short rightPart = ref input.GetRight<short>();
                    ref short outputPart = ref input.GetOutput<short>();
                    outputPart = (short)(leftPart - rightPart);
                }
                else if (length == 4)
                {
                    ref int leftPart = ref input.GetLeft<int>();
                    ref int rightPart = ref input.GetRight<int>();
                    ref int outputPart = ref input.GetOutput<int>();
                    outputPart = leftPart - rightPart;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void UnsignedMultiply(MixFunction.Input input)
        {
            unchecked
            {
                byte length = input.size;
                if (length == 1)
                {
                    ref byte leftPart = ref input.GetLeft<byte>();
                    ref byte rightPart = ref input.GetRight<byte>();
                    ref byte outputPart = ref input.GetOutput<byte>();
                    outputPart = (byte)(leftPart * rightPart);
                }
                else if (length == 2)
                {
                    ref ushort leftPart = ref input.GetLeft<ushort>();
                    ref ushort rightPart = ref input.GetRight<ushort>();
                    ref ushort outputPart = ref input.GetOutput<ushort>();
                    outputPart = (ushort)(leftPart * rightPart);
                }
                else if (length == 4)
                {
                    ref uint leftPart = ref input.GetLeft<uint>();
                    ref uint rightPart = ref input.GetRight<uint>();
                    ref uint outputPart = ref input.GetOutput<uint>();
                    outputPart = leftPart * rightPart;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void SignedMultiply(MixFunction.Input input)
        {
            unchecked
            {
                byte length = input.size;
                if (length == 1)
                {
                    ref sbyte leftPart = ref input.GetLeft<sbyte>();
                    ref sbyte rightPart = ref input.GetRight<sbyte>();
                    ref sbyte outputPart = ref input.GetOutput<sbyte>();
                    outputPart = (sbyte)(leftPart * rightPart);
                }
                else if (length == 2)
                {
                    ref short leftPart = ref input.GetLeft<short>();
                    ref short rightPart = ref input.GetRight<short>();
                    ref short outputPart = ref input.GetOutput<short>();
                    outputPart = (short)(leftPart * rightPart);
                }
                else if (length == 4)
                {
                    ref int leftPart = ref input.GetLeft<int>();
                    ref int rightPart = ref input.GetRight<int>();
                    ref int outputPart = ref input.GetOutput<int>();
                    outputPart = leftPart * rightPart;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void UnsignedDivide(MixFunction.Input input)
        {
            unchecked
            {
                byte length = input.size;
                if (length == 1)
                {
                    ref byte leftPart = ref input.GetLeft<byte>();
                    ref byte rightPart = ref input.GetRight<byte>();
                    ref byte outputPart = ref input.GetOutput<byte>();
                    outputPart = (byte)(leftPart / rightPart);
                }
                else if (length == 2)
                {
                    ref ushort leftPart = ref input.GetLeft<ushort>();
                    ref ushort rightPart = ref input.GetRight<ushort>();
                    ref ushort outputPart = ref input.GetOutput<ushort>();
                    outputPart = (ushort)(leftPart / rightPart);
                }
                else if (length == 4)
                {
                    ref uint leftPart = ref input.GetLeft<uint>();
                    ref uint rightPart = ref input.GetRight<uint>();
                    ref uint outputPart = ref input.GetOutput<uint>();
                    outputPart = leftPart / rightPart;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void SignedDivide(MixFunction.Input input)
        {
            unchecked
            {
                byte length = input.size;
                if (length == 1)
                {
                    ref sbyte leftPart = ref input.GetLeft<sbyte>();
                    ref sbyte rightPart = ref input.GetRight<sbyte>();
                    ref sbyte outputPart = ref input.GetOutput<sbyte>();
                    outputPart = (sbyte)(leftPart / rightPart);
                }
                else if (length == 2)
                {
                    ref short leftPart = ref input.GetLeft<short>();
                    ref short rightPart = ref input.GetRight<short>();
                    ref short outputPart = ref input.GetOutput<short>();
                    outputPart = (short)(leftPart / rightPart);
                }
                else if (length == 4)
                {
                    ref int leftPart = ref input.GetLeft<int>();
                    ref int rightPart = ref input.GetRight<int>();
                    ref int outputPart = ref input.GetOutput<int>();
                    outputPart = leftPart / rightPart;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void FloatingAdd(MixFunction.Input input)
        {
            unchecked
            {
                byte length = input.size;
                if (length == 4)
                {
                    ref float leftPart = ref input.GetLeft<float>();
                    ref float rightPart = ref input.GetRight<float>();
                    ref float outputPart = ref input.GetOutput<float>();
                    outputPart = leftPart + rightPart;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void FloatingSubtract(MixFunction.Input input)
        {
            unchecked
            {
                byte length = input.size;
                if (length == 4)
                {
                    ref float leftPart = ref input.GetLeft<float>();
                    ref float rightPart = ref input.GetRight<float>();
                    ref float outputPart = ref input.GetOutput<float>();
                    outputPart = leftPart - rightPart;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void FloatingMultiply(MixFunction.Input input)
        {
            unchecked
            {
                byte length = input.size;
                if (length == 4)
                {
                    ref float leftPart = ref input.GetLeft<float>();
                    ref float rightPart = ref input.GetRight<float>();
                    ref float outputPart = ref input.GetOutput<float>();
                    outputPart = leftPart * rightPart;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void FloatingDivide(MixFunction.Input input)
        {
            unchecked
            {
                byte length = input.size;
                if (length == 4)
                {
                    ref float leftPart = ref input.GetLeft<float>();
                    ref float rightPart = ref input.GetRight<float>();
                    ref float outputPart = ref input.GetOutput<float>();
                    outputPart = leftPart / rightPart;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [Conditional("DEBUG")]
        private void ThrowIfComponentIsMissing(uint entity, RuntimeType componentType)
        {
            if (!world.ContainsComponent(entity, componentType))
            {
                throw new System.Exception($"Entity `{entity}` is missing expected component `{componentType}`");
            }
        }

        [Conditional("DEBUG")]
        private void ThrowIfComponentSizesDontMatch(RuntimeType left, RuntimeType right)
        {
            if (left.Size != right.Size)
            {
                throw new System.Exception($"Components `{left}` and `{right}` don't match in size");
            }
        }

        public readonly struct Request
        {
            public readonly uint entity;
            public readonly ComponentMix mix;

            public Request(uint entity, ComponentMix mix)
            {
                this.entity = entity;
                this.mix = mix;
            }
        }
    }

    public readonly unsafe struct MixFunction
    {
        private readonly delegate* unmanaged<Input, void> function;

        public MixFunction(delegate* unmanaged<Input, void> function)
        {
            this.function = function;
        }

        public readonly void Invoke(USpan<byte> left, USpan<byte> right, USpan<byte> output)
        {
            function(new(left, right, output));
        }

        public readonly struct Input
        {
            public readonly byte size;

            private readonly nint left;
            private readonly nint right;
            private readonly nint output;

            public Input(USpan<byte> left, USpan<byte> right, USpan<byte> output)
            {
                size = (byte)output.Length;
                this.left = left.Address;
                this.right = right.Address;
                this.output = output.Address;
            }

            public readonly ref T GetLeft<T>() where T : unmanaged
            {
                return ref *(T*)left;
            }

            public readonly ref T GetRight<T>() where T : unmanaged
            {
                return ref *(T*)right;
            }

            public readonly ref T GetOutput<T>() where T : unmanaged
            {
                return ref *(T*)output;
            }
        }
    }
}