using Rendering;
using System.Numerics;
using Transforms.Components;
using UI.Components;
using Worlds;

namespace UI
{
    public readonly partial struct Resizable : IEntity
    {
        private const float PositiveDistance = 12f;
        private const float NegativeDistance = -2f;

        public readonly ref IsResizable.EdgeMask Boundary => ref GetComponent<IsResizable>().edgeMask;
        public readonly ref LayerMask SelectionMask => ref GetComponent<IsResizable>().selectionMask;

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsResizable>();
            archetype.AddComponentType<LocalToWorld>();
        }

        public readonly IsResizable.EdgeMask GetBoundary(Vector2 pointerPosition)
        {
            LocalToWorld ltw = GetComponent<LocalToWorld>();
            Vector3 position = ltw.Position;
            Vector3 scale = ltw.Scale;
            ref WorldRotation worldRotationComponent = ref TryGetComponent<WorldRotation>(out bool hasWorldRotation);
            if (hasWorldRotation)
            {
                scale = Vector3.Transform(scale, worldRotationComponent.value);
            }

            Vector2 offset = new Vector2(position.X, position.Y) + new Vector2(scale.X, scale.Y);
            Vector2 min = Vector2.Min(new(position.X, position.Y), offset);
            Vector2 max = Vector2.Max(new(position.X, position.Y), offset);
            UIBounds right = new(new(max.X + NegativeDistance, min.Y), new(max.X + PositiveDistance, max.Y));
            UIBounds left = new(new(min.X - PositiveDistance, min.Y), new(min.X - NegativeDistance, max.Y));
            UIBounds top = new(new(min.X, max.Y + NegativeDistance), new(max.X, max.Y + PositiveDistance));
            UIBounds bottom = new(new(min.X, min.Y - PositiveDistance), new(max.X, min.Y - NegativeDistance));
            UIBounds topRight = new(new(max.X + NegativeDistance, max.Y + NegativeDistance), new(max.X + PositiveDistance, max.Y + PositiveDistance));
            UIBounds topLeft = new(new(min.X - PositiveDistance, max.Y + NegativeDistance), new(min.X - NegativeDistance, max.Y + PositiveDistance));
            UIBounds bottomRight = new(new(max.X + NegativeDistance, min.Y - PositiveDistance), new(max.X + PositiveDistance, min.Y - NegativeDistance));
            UIBounds bottomLeft = new(new(min.X - PositiveDistance, min.Y - PositiveDistance), new(min.X - NegativeDistance, min.Y - NegativeDistance));
            IsResizable.EdgeMask boundary = default;

            if (right.Contains(pointerPosition))
            {
                boundary |= IsResizable.EdgeMask.Right;
            }
            else if (left.Contains(pointerPosition))
            {
                boundary |= IsResizable.EdgeMask.Left;
            }

            if (top.Contains(pointerPosition))
            {
                boundary |= IsResizable.EdgeMask.Top;
            }
            else if (bottom.Contains(pointerPosition))
            {
                boundary |= IsResizable.EdgeMask.Bottom;
            }

            else if (topRight.Contains(pointerPosition))
            {
                boundary = IsResizable.EdgeMask.TopRight;
            }
            else if (topLeft.Contains(pointerPosition))
            {
                boundary = IsResizable.EdgeMask.TopLeft;
            }
            else if (bottomRight.Contains(pointerPosition))
            {
                boundary = IsResizable.EdgeMask.BottomRight;
            }
            else if (bottomLeft.Contains(pointerPosition))
            {
                boundary = IsResizable.EdgeMask.BottomLeft;
            }

            return boundary;
        }
    }
}