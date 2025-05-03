using System;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Worlds;

namespace UI
{
    public readonly partial struct UITransform : IEntity
    {
        public unsafe readonly ref Vector2 Position
        {
            get
            {
                ref Vector3 localPosition = ref As<Transform>().LocalPosition;
                fixed (Vector3* positionPointer = &localPosition)
                {
                    return ref *(Vector2*)positionPointer;
                }
            }
        }

        public readonly ref float X => ref As<Transform>().LocalPosition.X;
        public readonly ref float Y => ref As<Transform>().LocalPosition.Y;
        public readonly ref float Z => ref As<Transform>().LocalPosition.Z;

        public unsafe readonly ref Vector2 Size
        {
            get
            {
                ref Vector3 localScale = ref As<Transform>().LocalScale;
                fixed (Vector3* sizePointer = &localScale)
                {
                    return ref *(Vector2*)sizePointer;
                }
            }
        }

        public readonly ref float Width => ref As<Transform>().LocalScale.X;
        public readonly ref float Height => ref As<Transform>().LocalScale.Y;

        /// <summary>
        /// Rotation of the UI transform on the Z axis.
        /// </summary>
        public readonly float Rotation
        {
            get
            {
                Quaternion rot = As<Transform>().LocalRotation;
                float y = 2f * (rot.W * rot.Z + rot.X * rot.Y);
                float x = 1f - 2f * (rot.Y * rot.Y + rot.Z * rot.Z);
                return MathF.Atan2(y, x);
            }
            set
            {
                ref Quaternion rot = ref As<Transform>().LocalRotation;
                rot = Quaternion.CreateFromYawPitchRoll(0f, 0f, value);
            }
        }

        public readonly ref Anchor Anchor => ref GetComponent<Anchor>();
        public readonly ref Vector3 Pivot => ref GetComponent<Pivot>().value;

        public UITransform(World world, Vector2 position, Vector2 size, float rotation = default)
        {
            this.world = world;
            Vector3 worldPosition = new(position, 0);
            Quaternion worldRotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, rotation);
            Vector3 worldScale = new(size, 1);
            value = new Transform(world, worldPosition, worldRotation, worldScale).value;
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsTransform>();
        }

        public static implicit operator Transform(UITransform transform)
        {
            return transform.As<Transform>();
        }
    }
}