using InteractionKit.Components;
using InteractionKit.Events;
using Simulation;
using System.Diagnostics;
using Unmanaged;
using Unmanaged.Collections;

namespace InteractionKit.Systems
{
    public unsafe class ComponentMixingSystem : SystemBase
    {
        private readonly ComponentQuery<ComponentMix> query;
        private readonly UnmanagedList<Request> requests;

        public ComponentMixingSystem(World world) : base(world)
        {
            query = new();
            requests = new();
            Subscribe<MixingUpdate>(Update);
        }

        public override void Dispose()
        {
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
                byte partCount = mix.partCount;
                uint partSize = (uint)(leftType.Size / partCount);
                for (uint i = 0; i < partCount; i++)
                {
                    USpan<byte> leftPart = leftBytes.Slice(i * partSize, partSize);
                    USpan<byte> rightPart = rightBytes.Slice(i * partSize, partSize);
                    USpan<byte> outputPart = outputBytes.Slice(i * partSize, partSize);
                    switch (mix.operation)
                    {
                        case ComponentMix.Operation.UnsignedAdd:
                            UnsignedAdd(leftPart, rightPart, outputPart);
                            break;
                        case ComponentMix.Operation.UnsignedSubtract:
                            UnsignedSubtract(leftPart, rightPart, outputPart);
                            break;
                        case ComponentMix.Operation.UnsignedMultiply:
                            UnsignedMultiply(leftPart, rightPart, outputPart);
                            break;
                        case ComponentMix.Operation.UnsignedDivide:
                            UnsignedDivide(leftPart, rightPart, outputPart);
                            break;
                        case ComponentMix.Operation.SignedAdd:
                            SignedAdd(leftPart, rightPart, outputPart);
                            break;
                        case ComponentMix.Operation.SignedSubtract:
                            SignedSubtract(leftPart, rightPart, outputPart);
                            break;
                        case ComponentMix.Operation.SignedMultiply:
                            SignedMultiply(leftPart, rightPart, outputPart);
                            break;
                        case ComponentMix.Operation.SignedDivide:
                            SignedDivide(leftPart, rightPart, outputPart);
                            break;
                        case ComponentMix.Operation.FloatingAdd:
                            FloatingAdd(leftPart, rightPart, outputPart);
                            break;
                        case ComponentMix.Operation.FloatingSubtract:
                            FloatingSubtract(leftPart, rightPart, outputPart);
                            break;
                        case ComponentMix.Operation.FloatingMultiply:
                            FloatingMultiply(leftPart, rightPart, outputPart);
                            break;
                        case ComponentMix.Operation.FloatingDivide:
                            FloatingDivide(leftPart, rightPart, outputPart);
                            break;
                        default:
                            throw new System.Exception($"Operation `{mix.operation}` is not supported");
                    }
                }
            }
        }

        private static void UnsignedAdd(USpan<byte> leftPart, USpan<byte> rightPart, USpan<byte> outputPart)
        {
            uint partSize = outputPart.Length;
            unchecked
            {
                if (partSize == 1)
                {
                    byte leftValue = leftPart[0];
                    byte rightValue = rightPart[0];
                    byte outputValue = (byte)(leftValue + rightValue);
                    outputPart[0] = outputValue;
                }
                else if (partSize == 2)
                {
                    ushort leftValue = *(ushort*)leftPart.pointer;
                    ushort rightValue = *(ushort*)rightPart.pointer;
                    ushort outputValue = (ushort)(leftValue + rightValue);
                    *(ushort*)outputPart.pointer = outputValue;
                }
                else if (partSize == 4)
                {
                    uint leftValue = *(uint*)leftPart.pointer;
                    uint rightValue = *(uint*)rightPart.pointer;
                    uint outputValue = leftValue + rightValue;
                    *(uint*)outputPart.pointer = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{partSize}` is not supported");
                }
            }
        }

        private static void SignedAdd(USpan<byte> leftPart, USpan<byte> rightPart, USpan<byte> outputPart)
        {
            uint partSize = outputPart.Length;
            unchecked
            {
                if (partSize == 1)
                {
                    sbyte leftValue = (sbyte)leftPart[0];
                    sbyte rightValue = (sbyte)rightPart[0];
                    sbyte outputValue = (sbyte)(leftValue + rightValue);
                    outputPart[0] = (byte)outputValue;
                }
                else if (partSize == 2)
                {
                    short leftValue = *(short*)leftPart.pointer;
                    short rightValue = *(short*)rightPart.pointer;
                    short outputValue = (short)(leftValue + rightValue);
                    *(short*)outputPart.pointer = outputValue;
                }
                else if (partSize == 4)
                {
                    int leftValue = *(int*)leftPart.pointer;
                    int rightValue = *(int*)rightPart.pointer;
                    int outputValue = leftValue + rightValue;
                    *(int*)outputPart.pointer = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{partSize}` is not supported");
                }
            }
        }

        private static void UnsignedSubtract(USpan<byte> leftPart, USpan<byte> rightPart, USpan<byte> outputPart)
        {
            uint partSize = outputPart.Length;
            unchecked
            {
                if (partSize == 1)
                {
                    byte leftValue = leftPart[0];
                    byte rightValue = rightPart[0];
                    byte outputValue = (byte)(leftValue - rightValue);
                    outputPart[0] = outputValue;
                }
                else if (partSize == 2)
                {
                    ushort leftValue = *(ushort*)leftPart.pointer;
                    ushort rightValue = *(ushort*)rightPart.pointer;
                    ushort outputValue = (ushort)(leftValue - rightValue);
                    *(ushort*)outputPart.pointer = outputValue;
                }
                else if (partSize == 4)
                {
                    uint leftValue = *(uint*)leftPart.pointer;
                    uint rightValue = *(uint*)rightPart.pointer;
                    uint outputValue = leftValue - rightValue;
                    *(uint*)outputPart.pointer = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{partSize}` is not supported");
                }
            }
        }

        private static void SignedSubtract(USpan<byte> leftPart, USpan<byte> rightPart, USpan<byte> outputPart)
        {
            uint partSize = outputPart.Length;
            unchecked
            {
                if (partSize == 1)
                {
                    sbyte leftValue = (sbyte)leftPart[0];
                    sbyte rightValue = (sbyte)rightPart[0];
                    sbyte outputValue = (sbyte)(leftValue - rightValue);
                    outputPart[0] = (byte)outputValue;
                }
                else if (partSize == 2)
                {
                    short leftValue = *(short*)leftPart.pointer;
                    short rightValue = *(short*)rightPart.pointer;
                    short outputValue = (short)(leftValue - rightValue);
                    *(short*)outputPart.pointer = outputValue;
                }
                else if (partSize == 4)
                {
                    int leftValue = *(int*)leftPart.pointer;
                    int rightValue = *(int*)rightPart.pointer;
                    int outputValue = leftValue - rightValue;
                    *(int*)outputPart.pointer = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{partSize}` is not supported");
                }
            }
        }

        private static void UnsignedMultiply(USpan<byte> leftPart, USpan<byte> rightPart, USpan<byte> outputPart)
        {
            uint partSize = outputPart.Length;
            unchecked
            {
                if (partSize == 1)
                {
                    byte leftValue = leftPart[0];
                    byte rightValue = rightPart[0];
                    byte outputValue = (byte)(leftValue * rightValue);
                    outputPart[0] = outputValue;
                }
                else if (partSize == 2)
                {
                    ushort leftValue = *(ushort*)leftPart.pointer;
                    ushort rightValue = *(ushort*)rightPart.pointer;
                    ushort outputValue = (ushort)(leftValue * rightValue);
                    *(ushort*)outputPart.pointer = outputValue;
                }
                else if (partSize == 4)
                {
                    uint leftValue = *(uint*)leftPart.pointer;
                    uint rightValue = *(uint*)rightPart.pointer;
                    uint outputValue = leftValue * rightValue;
                    *(uint*)outputPart.pointer = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{partSize}` is not supported");
                }
            }
        }

        private static void SignedMultiply(USpan<byte> leftPart, USpan<byte> rightPart, USpan<byte> outputPart)
        {
            uint partSize = outputPart.Length;
            unchecked
            {
                if (partSize == 1)
                {
                    sbyte leftValue = (sbyte)leftPart[0];
                    sbyte rightValue = (sbyte)rightPart[0];
                    sbyte outputValue = (sbyte)(leftValue * rightValue);
                    outputPart[0] = (byte)outputValue;
                }
                else if (partSize == 2)
                {
                    short leftValue = *(short*)leftPart.pointer;
                    short rightValue = *(short*)rightPart.pointer;
                    short outputValue = (short)(leftValue * rightValue);
                    *(short*)outputPart.pointer = outputValue;
                }
                else if (partSize == 4)
                {
                    int leftValue = *(int*)leftPart.pointer;
                    int rightValue = *(int*)rightPart.pointer;
                    int outputValue = leftValue * rightValue;
                    *(int*)outputPart.pointer = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{partSize}` is not supported");
                }
            }
        }

        private static void UnsignedDivide(USpan<byte> leftPart, USpan<byte> rightPart, USpan<byte> outputPart)
        {
            uint partSize = outputPart.Length;
            unchecked
            {
                if (partSize == 1)
                {
                    byte leftValue = leftPart[0];
                    byte rightValue = rightPart[0];
                    byte outputValue = (byte)(leftValue / rightValue);
                    outputPart[0] = outputValue;
                }
                else if (partSize == 2)
                {
                    ushort leftValue = *(ushort*)leftPart.pointer;
                    ushort rightValue = *(ushort*)rightPart.pointer;
                    ushort outputValue = (ushort)(leftValue / rightValue);
                    *(ushort*)outputPart.pointer = outputValue;
                }
                else if (partSize == 4)
                {
                    uint leftValue = *(uint*)leftPart.pointer;
                    uint rightValue = *(uint*)rightPart.pointer;
                    uint outputValue = leftValue / rightValue;
                    *(uint*)outputPart.pointer = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{partSize}` is not supported");
                }
            }
        }

        private static void SignedDivide(USpan<byte> leftPart, USpan<byte> rightPart, USpan<byte> outputPart)
        {
            uint partSize = outputPart.Length;
            unchecked
            {
                if (partSize == 1)
                {
                    sbyte leftValue = (sbyte)leftPart[0];
                    sbyte rightValue = (sbyte)rightPart[0];
                    sbyte outputValue = (sbyte)(leftValue / rightValue);
                    outputPart[0] = (byte)outputValue;
                }
                else if (partSize == 2)
                {
                    short leftValue = *(short*)leftPart.pointer;
                    short rightValue = *(short*)rightPart.pointer;
                    short outputValue = (short)(leftValue / rightValue);
                    *(short*)outputPart.pointer = outputValue;
                }
                else if (partSize == 4)
                {
                    int leftValue = *(int*)leftPart.pointer;
                    int rightValue = *(int*)rightPart.pointer;
                    int outputValue = leftValue / rightValue;
                    *(int*)outputPart.pointer = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{partSize}` is not supported");
                }
            }
        }

        private static void FloatingAdd(USpan<byte> leftPart, USpan<byte> rightPart, USpan<byte> outputPart)
        {
            unchecked
            {
                if (outputPart.Length == 4)
                {
                    float leftValue = *(float*)leftPart.pointer;
                    float rightValue = *(float*)rightPart.pointer;
                    float outputValue = leftValue + rightValue;
                    *(float*)outputPart.pointer = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{outputPart.Length}` is not supported");
                }
            }
        }

        private static void FloatingSubtract(USpan<byte> leftPart, USpan<byte> rightPart, USpan<byte> outputPart)
        {
            unchecked
            {
                if (outputPart.Length == 4)
                {
                    float leftValue = *(float*)leftPart.pointer;
                    float rightValue = *(float*)rightPart.pointer;
                    float outputValue = leftValue - rightValue;
                    *(float*)outputPart.pointer = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{outputPart.Length}` is not supported");
                }
            }
        }

        private static void FloatingMultiply(USpan<byte> leftPart, USpan<byte> rightPart, USpan<byte> outputPart)
        {
            unchecked
            {
                if (outputPart.Length == 4)
                {
                    float leftValue = *(float*)leftPart.pointer;
                    float rightValue = *(float*)rightPart.pointer;
                    float outputValue = leftValue * rightValue;
                    *(float*)outputPart.pointer = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{outputPart.Length}` is not supported");
                }
            }
        }

        private static void FloatingDivide(USpan<byte> leftPart, USpan<byte> rightPart, USpan<byte> outputPart)
        {
            unchecked
            {
                if (outputPart.Length == 4)
                {
                    float leftValue = *(float*)leftPart.pointer;
                    float rightValue = *(float*)rightPart.pointer;
                    float outputValue = leftValue / rightValue;
                    *(float*)outputPart.pointer = outputValue;
                }
                else
                {
                    throw new System.Exception($"Part size `{outputPart.Length}` is not supported");
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
}