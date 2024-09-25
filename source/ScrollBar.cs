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
        public readonly Image background;

        public readonly Entity Parent
        {
            get => background.Parent;
            set => background.Parent = value;
        }

        public readonly Vector2 Position
        {
            get => background.Position;
            set => background.Position = value;
        }

        public readonly Vector2 Size
        {
            get => background.Size;
            set => background.Size = value;
        }

        public readonly ref Anchor Anchor => ref background.Anchor;
        public readonly ref Vector3 Pivot => ref background.Pivot;

        public readonly ref Color BackgroundColor => ref background.Color;

        public readonly ref Color ScrollHandleColor
        {
            get
            {
                rint scrollHandleReference = background.AsEntity().GetComponent<IsScrollBar>().scrollHandleReference;
                uint scrollHandleEntity = background.GetReference(scrollHandleReference);
                Image scrollHandle = new(background.GetWorld(), scrollHandleEntity);
                return ref scrollHandle.Color;
            }
        }

        public readonly ref Vector2 Axis => ref background.AsEntity().GetComponentRef<IsScrollBar>().axis;

        public readonly Vector2 Value
        {
            get
            {
                return background.AsEntity().GetComponent<IsScrollBar>().value;
            }
            set
            {
                background.AsEntity().GetComponentRef<IsScrollBar>().value = value;
            }
        }

        public readonly float HandlePercentageSize
        {
            get
            {
                IsScrollBar component = background.AsEntity().GetComponent<IsScrollBar>();
                rint scrollHandleReference = component.scrollHandleReference;
                uint scrollHandleEntity = background.GetReference(scrollHandleReference);
                Image scrollHandle = new(background.GetWorld(), scrollHandleEntity);
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
                IsScrollBar component = background.AsEntity().GetComponent<IsScrollBar>();
                rint scrollHandleReference = component.scrollHandleReference;
                uint scrollHandleEntity = background.GetReference(scrollHandleReference);
                Image scrollHandle = new(background.GetWorld(), scrollHandleEntity);
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

        readonly uint IEntity.Value => background.GetEntityValue();
        readonly World IEntity.World => background.GetWorld();
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsSelectable>(), RuntimeType.Get<IsScrollBar>()], []);

        public ScrollBar(World world, Canvas canvas, Vector2 axis, float handlePercentageSize)
        {
            background = new Image(world, canvas);

            Transform scrollRegion = new(world);
            scrollRegion.Parent = background;
            scrollRegion.AddComponent(new Anchor(new(4, true), new(4, true), default, new(4, true), new(4, true), default));
            scrollRegion.AddComponent(new IsSelectable());

            Image scrollHandle = new(world, canvas);
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

            scrollHandle.AddComponent(new IsSelectable());

            rint scrollHandleReference = background.AddReference(scrollHandle);
            background.AddComponent(new IsScrollBar(scrollHandleReference, axis));
        }
    }
}