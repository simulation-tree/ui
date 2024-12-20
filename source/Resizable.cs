using InteractionKit.Components;
using InteractionKit.Systems;
using System.Numerics;
using Transforms.Components;
using Worlds;

namespace InteractionKit
{
    public readonly struct Resizable : IEntity
    {
        private const float PositiveDistance = 12f;
        private const float NegativeDistance = -2f;

        private readonly Entity entity;

        public readonly ref IsResizable.Boundary Boundary => ref entity.GetComponent<IsResizable>().resize;

        readonly uint IEntity.Value => entity.GetEntityValue();
        readonly World IEntity.World => entity.GetWorld();
        readonly Definition IEntity.Definition => new Definition().AddComponentTypes<IsResizable, LocalToWorld>();

        public Resizable(World world, uint existingEntity)
        {
            entity = new Entity(world, existingEntity);
        }

        public readonly void Dispose()
        {
            entity.Dispose();
        }

        public readonly IsResizable.Boundary GetBoundary(Vector2 pointerPosition)
        {
            LocalToWorld ltw = entity.GetComponent<LocalToWorld>();
            Vector3 position = ltw.Position;
            Vector3 scale = ltw.Scale;
            ref WorldRotation worldRotationComponent = ref entity.TryGetComponent<WorldRotation>(out bool hasWorldRotation);
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
            IsResizable.Boundary boundary = default;

            if (right.Contains(pointerPosition))
            {
                boundary |= IsResizable.Boundary.Right;
            }
            else if (left.Contains(pointerPosition))
            {
                boundary |= IsResizable.Boundary.Left;
            }

            if (top.Contains(pointerPosition))
            {
                boundary |= IsResizable.Boundary.Top;
            }
            else if (bottom.Contains(pointerPosition))
            {
                boundary |= IsResizable.Boundary.Bottom;
            }

            else if (topRight.Contains(pointerPosition))
            {
                boundary = IsResizable.Boundary.TopRight;
            }
            else if (topLeft.Contains(pointerPosition))
            {
                boundary = IsResizable.Boundary.TopLeft;
            }
            else if (bottomRight.Contains(pointerPosition))
            {
                boundary = IsResizable.Boundary.BottomRight;
            }
            else if (bottomLeft.Contains(pointerPosition))
            {
                boundary = IsResizable.Boundary.BottomLeft;
            }

            return boundary;
        }

        public static implicit operator Entity(Resizable resizable)
        {
            return resizable.entity;
        }
    }
}