using System.Numerics;
using Transforms;
using Transforms.Components;
using UI.Components;
using Worlds;

namespace UI
{
    public readonly partial struct ScrollBar : ISelectable
    {
        public readonly ref Vector2 Position => ref As<UITransform>().Position;
        public readonly ref float X => ref As<UITransform>().X;
        public readonly ref float Y => ref As<UITransform>().Y;
        public readonly ref float Z => ref As<UITransform>().Z;
        public readonly ref Vector2 Size => ref As<UITransform>().Size;
        public readonly ref float Width => ref As<UITransform>().Width;
        public readonly ref float Height => ref As<UITransform>().Height;

        public readonly float Rotation
        {
            get => As<UITransform>().Rotation;
            set => As<UITransform>().Rotation = value;
        }

        public readonly ref Anchor Anchor => ref As<UITransform>().Anchor;
        public readonly ref Vector3 Pivot => ref As<UITransform>().Pivot;
        public readonly ref Vector4 BackgroundColor => ref As<Image>().Color;

        public readonly ref Vector4 ScrollHandleColor
        {
            get
            {
                rint scrollHandleReference = GetComponent<IsScrollBar>().scrollHandleReference;
                uint scrollHandleEntity = GetReference(scrollHandleReference);
                Image scrollHandle = new Entity(world, scrollHandleEntity).As<Image>();
                return ref scrollHandle.Color;
            }
        }

        public readonly ref Vector2 Axis => ref GetComponent<IsScrollBar>().axis;
        public readonly ref Vector2 Value => ref GetComponent<IsScrollBar>().value;

        public readonly float HandlePercentageSize
        {
            get
            {
                IsScrollBar component = GetComponent<IsScrollBar>();
                rint scrollHandleReference = component.scrollHandleReference;
                uint scrollHandleEntity = GetReference(scrollHandleReference);
                Image scrollHandle = new Entity(world, scrollHandleEntity).As<Image>();

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
                IsScrollBar component = GetComponent<IsScrollBar>();
                rint scrollHandleReference = component.scrollHandleReference;
                uint scrollHandleEntity = GetReference(scrollHandleReference);
                Image scrollHandle = new Entity(world, scrollHandleEntity).As<Image>();

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

        public ScrollBar(Canvas canvas, Vector2 axis, float handlePercentageSize)
        {
            world = canvas.world;
            Image background = new(canvas);
            value = background.value;

            Transform scrollRegion = new(world);
            scrollRegion.SetParent(background);
            scrollRegion.AddComponent(new Anchor(4, 4, default, 4, 4, default, Anchor.Relativeness.X | Anchor.Relativeness.Y));
            scrollRegion.AddComponent(new IsSelectable(canvas.SelectionMask));

            Image scrollHandle = new(canvas);
            scrollHandle.SetParent(scrollRegion);
            scrollHandle.Color = new(0, 0, 0, 1);
            if (axis.Y > axis.X)
            {
                scrollHandle.Anchor = new(0f, default, default, 1f, default, default);
                scrollHandle.Size = new(1, handlePercentageSize);
            }
            else if (axis.X > axis.Y)
            {
                scrollHandle.Anchor = new(default, 0f, default, default, 1f, default);
                scrollHandle.Size = new(handlePercentageSize, 1f);
            }
            else
            {
                scrollHandle.Anchor = default;
                scrollHandle.Size = new(handlePercentageSize);
            }

            scrollHandle.AddComponent(new IsSelectable(canvas.SelectionMask));

            rint scrollHandleReference = background.AddReference(scrollHandle);
            background.AddComponent(new IsScrollBar(scrollHandleReference, axis));
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsScrollBar>();
            archetype.AddComponentType<IsSelectable>();
        }

        public static implicit operator Image(ScrollBar scrollBar)
        {
            return scrollBar.As<Image>();
        }

        public static implicit operator UITransform(ScrollBar scrollBar)
        {
            return scrollBar.As<UITransform>();
        }

        public static implicit operator Transform(ScrollBar scrollBar)
        {
            return scrollBar.As<Transform>();
        }
    }
}