using Data;
using InteractionKit.Components;
using Simulation;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct ScrollBar : ISelectable
    {
        public readonly Image box;

        public readonly Entity Parent
        {
            get => box.Parent;
            set => box.Parent = value;
        }

        public readonly Vector2 Position
        {
            get => box.Position;
            set => box.Position = value;
        }

        public readonly Vector2 Size
        {
            get => box.Size;
            set => box.Size = value;
        }

        public readonly ref Anchor Anchor => ref box.Anchor;
        public readonly ref Vector3 Pivot => ref box.Pivot;

        public readonly ref Color BackgroundColor => ref box.Color;

        public readonly ref Color ScrollHandleColor
        {
            get
            {
                rint scrollHandleReference = box.AsEntity().GetComponent<IsScrollBar>().scrollHandleReference;
                uint scrollHandleEntity = box.GetReference(scrollHandleReference);
                Image scrollHandle = new(box.GetWorld(), scrollHandleEntity);
                return ref scrollHandle.Color;
            }
        }

        public readonly ref Vector2 Axis => ref box.AsEntity().GetComponentRef<IsScrollBar>().axis;

        public readonly Vector2 Value
        {
            get
            {
                return box.AsEntity().GetComponent<IsScrollBar>().value;
            }
        }

        public readonly float HandlePercentageSize
        {
            get
            {
                IsScrollBar component = box.AsEntity().GetComponent<IsScrollBar>();
                rint scrollHandleReference = component.scrollHandleReference;
                uint scrollHandleEntity = box.GetReference(scrollHandleReference);
                Image scrollHandle = new(box.GetWorld(), scrollHandleEntity);
                if (component.axis.Y > component.axis.X)
                {
                    return scrollHandle.Size.Y;
                }
                else if (component.axis.X > component.axis.Y)
                {
                    return scrollHandle.Size.X;
                }
                else
                {
                    return scrollHandle.Size.X;
                }
            }
            set
            {
                IsScrollBar component = box.AsEntity().GetComponent<IsScrollBar>();
                rint scrollHandleReference = component.scrollHandleReference;
                uint scrollHandleEntity = box.GetReference(scrollHandleReference);
                Image scrollHandle = new(box.GetWorld(), scrollHandleEntity);
                if (component.axis.Y > component.axis.X)
                {
                    scrollHandle.Size = new(1, value);
                }
                else if (component.axis.X > component.axis.Y)
                {
                    scrollHandle.Size = new(value, 1);
                }
                else
                {
                    scrollHandle.Size = new(value);
                }
            }
        }

        readonly uint IEntity.Value => box.GetEntityValue();
        readonly World IEntity.World => box.GetWorld();
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsSelectable>(), RuntimeType.Get<IsScrollBar>()], []);

        public ScrollBar(World world, InteractiveContext context, Vector2 axis, float handlePercentageSize)
        {
            box = new Image(world, context);
            box.transform.LocalPosition = new(0f, 0f, 0.1f);

            Transform scrollRegion = new(world);
            scrollRegion.Parent = box;
            scrollRegion.AsEntity().AddComponent(new Anchor(new(4, true), new(4, true), default, new(4, true), new(4, true), default));

            Image scrollHandle = new(world, context);
            scrollHandle.Parent = scrollRegion;
            scrollHandle.Color = Color.Black;
            if (axis.Y > axis.X)
            {
                scrollHandle.Anchor = new(new(0f, false), default, default, new(1f, false), default, default);
                scrollHandle.Size = new(1, handlePercentageSize);
            }
            else if (axis.X > axis.Y)
            {
                scrollHandle.Anchor = new(default, new(0f, false), default, default, new(1f, false), default);
                scrollHandle.Size = new(handlePercentageSize, 1f);
            }
            else
            {
                scrollHandle.Anchor = default;
                scrollHandle.Size = new(handlePercentageSize);
            }

            scrollHandle.AsEntity().AddComponent(new IsSelectable());

            rint scrollHandleReference = box.AddReference(scrollHandle);
            box.transform.entity.AddComponent(new IsScrollBar(scrollHandleReference, axis));
        }
    }
}