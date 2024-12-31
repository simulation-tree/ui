using InteractionKit.Components;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Worlds;

namespace InteractionKit
{
    public readonly struct ScrollBar : ISelectable
    {
        private readonly Image background;

        public readonly ref Vector2 Position => ref background.Position;
        public readonly ref Vector2 Size => ref background.Size;
        public readonly ref Anchor Anchor => ref background.Anchor;
        public readonly ref Vector3 Pivot => ref background.Pivot;
        public readonly ref Vector4 BackgroundColor => ref background.Color;

        public readonly ref Vector4 ScrollHandleColor
        {
            get
            {
                rint scrollHandleReference = background.AsEntity().GetComponent<IsScrollBar>().scrollHandleReference;
                uint scrollHandleEntity = background.GetReference(scrollHandleReference);
                Image scrollHandle = new(background.GetWorld(), scrollHandleEntity);
                return ref scrollHandle.Color;
            }
        }

        public readonly ref Vector2 Axis => ref background.AsEntity().GetComponent<IsScrollBar>().axis;

        public readonly Vector2 Value
        {
            get
            {
                return background.AsEntity().GetComponent<IsScrollBar>().value;
            }
            set
            {
                background.AsEntity().GetComponent<IsScrollBar>().value = value;
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

        readonly Definition IEntity.GetDefinition(Schema schema)
        {
            return new Definition().AddComponentTypes<IsScrollBar, IsSelectable>(schema);
        }

        public ScrollBar(Canvas canvas, Vector2 axis, float handlePercentageSize)
        {
            World world = canvas.GetWorld();
            background = new Image(canvas);

            Transform scrollRegion = new(world);
            scrollRegion.SetParent(background);
            scrollRegion.AddComponent(new Anchor(new(4, true), new(4, true), default, new(4, true), new(4, true), default));
            scrollRegion.AddComponent(new IsSelectable());

            Image scrollHandle = new(canvas);
            scrollHandle.SetParent(scrollRegion);
            scrollHandle.Color = new(0, 0, 0, 1);
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

        public readonly void Dispose()
        {
            background.Dispose();
        }

        public static implicit operator Entity(ScrollBar scrollBar)
        {
            return scrollBar.background;
        }

        public static implicit operator Image(ScrollBar scrollBar)
        {
            return scrollBar.background;
        }

        public static implicit operator Transform(ScrollBar scrollBar)
        {
            return scrollBar.background;
        }
    }
}