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

        [UnmanagedCallersOnly]
        private static void UnsignedAdd(byte* leftPart, byte* rightPart, byte* outputPart, byte length)
        {
            unchecked
            {
                if (length == 1)
                {
                    byte leftValue = leftPart[0];
                    byte rightValue = rightPart[0];
                    byte outputValue = (byte)(leftValue + rightValue);
                    outputPart[0] = outputValue;
                }
                else if (length == 2)
                {
                    ushort leftValue = *(ushort*)leftPart;
                    ushort rightValue = *(ushort*)rightPart;
                    ushort outputValue = (ushort)(leftValue + rightValue);
                    *(ushort*)outputPart = outputValue;
                }
                else if (length == 4)
                {
                    uint leftValue = *(uint*)leftPart;
                    uint rightValue = *(uint*)rightPart;
                    uint outputValue = leftValue + rightValue;
                    *(uint*)outputPart = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void SignedAdd(byte* leftPart, byte* rightPart, byte* outputPart, byte length)
        {
            unchecked
            {
                if (length == 1)
                {
                    sbyte leftValue = (sbyte)leftPart[0];
                    sbyte rightValue = (sbyte)rightPart[0];
                    sbyte outputValue = (sbyte)(leftValue + rightValue);
                    outputPart[0] = (byte)outputValue;
                }
                else if (length == 2)
                {
                    short leftValue = *(short*)leftPart;
                    short rightValue = *(short*)rightPart;
                    short outputValue = (short)(leftValue + rightValue);
                    *(short*)outputPart = outputValue;
                }
                else if (length == 4)
                {
                    int leftValue = *(int*)leftPart;
                    int rightValue = *(int*)rightPart;
                    int outputValue = leftValue + rightValue;
                    *(int*)outputPart = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void UnsignedSubtract(byte* leftPart, byte* rightPart, byte* outputPart, byte length)
        {
            unchecked
            {
                if (length == 1)
                {
                    byte leftValue = leftPart[0];
                    byte rightValue = rightPart[0];
                    byte outputValue = (byte)(leftValue - rightValue);
                    outputPart[0] = outputValue;
                }
                else if (length == 2)
                {
                    ushort leftValue = *(ushort*)leftPart;
                    ushort rightValue = *(ushort*)rightPart;
                    ushort outputValue = (ushort)(leftValue - rightValue);
                    *(ushort*)outputPart = outputValue;
                }
                else if (length == 4)
                {
                    uint leftValue = *(uint*)leftPart;
                    uint rightValue = *(uint*)rightPart;
                    uint outputValue = leftValue - rightValue;
                    *(uint*)outputPart = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void SignedSubtract(byte* leftPart, byte* rightPart, byte* outputPart, byte length)
        {
            unchecked
            {
                if (length == 1)
                {
                    sbyte leftValue = (sbyte)leftPart[0];
                    sbyte rightValue = (sbyte)rightPart[0];
                    sbyte outputValue = (sbyte)(leftValue - rightValue);
                    outputPart[0] = (byte)outputValue;
                }
                else if (length == 2)
                {
                    short leftValue = *(short*)leftPart;
                    short rightValue = *(short*)rightPart;
                    short outputValue = (short)(leftValue - rightValue);
                    *(short*)outputPart = outputValue;
                }
                else if (length == 4)
                {
                    int leftValue = *(int*)leftPart;
                    int rightValue = *(int*)rightPart;
                    int outputValue = leftValue - rightValue;
                    *(int*)outputPart = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void UnsignedMultiply(byte* leftPart, byte* rightPart, byte* outputPart, byte length)
        {
            unchecked
            {
                if (length == 1)
                {
                    byte leftValue = leftPart[0];
                    byte rightValue = rightPart[0];
                    byte outputValue = (byte)(leftValue * rightValue);
                    outputPart[0] = outputValue;
                }
                else if (length == 2)
                {
                    ushort leftValue = *(ushort*)leftPart;
                    ushort rightValue = *(ushort*)rightPart;
                    ushort outputValue = (ushort)(leftValue * rightValue);
                    *(ushort*)outputPart = outputValue;
                }
                else if (length == 4)
                {
                    uint leftValue = *(uint*)leftPart;
                    uint rightValue = *(uint*)rightPart;
                    uint outputValue = leftValue * rightValue;
                    *(uint*)outputPart = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void SignedMultiply(byte* leftPart, byte* rightPart, byte* outputPart, byte length)
        {
            unchecked
            {
                if (length == 1)
                {
                    sbyte leftValue = (sbyte)leftPart[0];
                    sbyte rightValue = (sbyte)rightPart[0];
                    sbyte outputValue = (sbyte)(leftValue * rightValue);
                    outputPart[0] = (byte)outputValue;
                }
                else if (length == 2)
                {
                    short leftValue = *(short*)leftPart;
                    short rightValue = *(short*)rightPart;
                    short outputValue = (short)(leftValue * rightValue);
                    *(short*)outputPart = outputValue;
                }
                else if (length == 4)
                {
                    int leftValue = *(int*)leftPart;
                    int rightValue = *(int*)rightPart;
                    int outputValue = leftValue * rightValue;
                    *(int*)outputPart = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void UnsignedDivide(byte* leftPart, byte* rightPart, byte* outputPart, byte length)
        {
            unchecked
            {
                if (length == 1)
                {
                    byte leftValue = leftPart[0];
                    byte rightValue = rightPart[0];
                    byte outputValue = (byte)(leftValue / rightValue);
                    outputPart[0] = outputValue;
                }
                else if (length == 2)
                {
                    ushort leftValue = *(ushort*)leftPart;
                    ushort rightValue = *(ushort*)rightPart;
                    ushort outputValue = (ushort)(leftValue / rightValue);
                    *(ushort*)outputPart = outputValue;
                }
                else if (length == 4)
                {
                    uint leftValue = *(uint*)leftPart;
                    uint rightValue = *(uint*)rightPart;
                    uint outputValue = leftValue / rightValue;
                    *(uint*)outputPart = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void SignedDivide(byte* leftPart, byte* rightPart, byte* outputPart, byte length)
        {
            unchecked
            {
                if (length == 1)
                {
                    sbyte leftValue = (sbyte)leftPart[0];
                    sbyte rightValue = (sbyte)rightPart[0];
                    sbyte outputValue = (sbyte)(leftValue / rightValue);
                    outputPart[0] = (byte)outputValue;
                }
                else if (length == 2)
                {
                    short leftValue = *(short*)leftPart;
                    short rightValue = *(short*)rightPart;
                    short outputValue = (short)(leftValue / rightValue);
                    *(short*)outputPart = outputValue;
                }
                else if (length == 4)
                {
                    int leftValue = *(int*)leftPart;
                    int rightValue = *(int*)rightPart;
                    int outputValue = leftValue / rightValue;
                    *(int*)outputPart = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void FloatingAdd(byte* leftPart, byte* rightPart, byte* outputPart, byte length)
        {
            unchecked
            {
                if (length == 4)
                {
                    float leftValue = *(float*)leftPart;
                    float rightValue = *(float*)rightPart;
                    float outputValue = leftValue + rightValue;
                    *(float*)outputPart = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void FloatingSubtract(byte* leftPart, byte* rightPart, byte* outputPart, byte length)
        {
            unchecked
            {
                if (length == 4)
                {
                    float leftValue = *(float*)leftPart;
                    float rightValue = *(float*)rightPart;
                    float outputValue = leftValue - rightValue;
                    *(float*)outputPart = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void FloatingMultiply(byte* leftPart, byte* rightPart, byte* outputPart, byte length)
        {
            unchecked
            {
                if (length == 4)
                {
                    float leftValue = *(float*)leftPart;
                    float rightValue = *(float*)rightPart;
                    float outputValue = leftValue * rightValue;
                    *(float*)outputPart = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{length}` is not supported");
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void FloatingDivide(byte* leftPart, byte* rightPart, byte* outputPart, byte length)
        {
            unchecked
            {
                if (length == 4)
                {
                    float leftValue = *(float*)leftPart;
                    float rightValue = *(float*)rightPart;
                    float outputValue = leftValue / rightValue;
                    *(float*)outputPart = outputValue;
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
        private readonly delegate* unmanaged<byte*, byte*, byte*, byte, void> function;

        public MixFunction(delegate* unmanaged<byte*, byte*, byte*, byte, void> function)
        {
            this.function = function;
        }

        public readonly void Invoke(USpan<byte> left, USpan<byte> right, USpan<byte> output)
        {
            function(left.pointer, right.pointer, output.pointer, (byte)output.Length);
        }
    }
}