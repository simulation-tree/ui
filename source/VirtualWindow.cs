using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms;
using Transforms.Components;
using UI.Components;
using UI.Functions;
using Unmanaged;
using Worlds;

namespace UI
{
    public readonly partial struct VirtualWindow : IEntity
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

        public readonly UITransform Container
        {
            get
            {
                IsVirtualWindow component = GetComponent<IsVirtualWindow>();
                rint viewReference = component.viewReference;
                uint viewEntity = GetReference(viewReference);
                View view = new Entity(world, viewEntity).As<View>();
                return view.Content;
            }
        }

        public readonly Entity Header
        {
            get
            {
                IsVirtualWindow component = GetComponent<IsVirtualWindow>();
                rint headerReference = component.headerReference;
                uint headerEntity = GetReference(headerReference);
                return new(world, headerEntity);
            }
        }

        private unsafe VirtualWindow(World world, Canvas canvas, FixedString titleText, VirtualWindowClose closeCallback)
        {
            this.world = world;
            Image background = new(canvas);
            value = background.value;

            background.AddComponent(new IsResizable(IsResizable.Boundary.All));

            Image header = new(canvas);
            header.Anchor = new(new(0f, false), new(1f, false), default, new(1f, false), new(1f, false), default);
            header.Pivot = new(0f, 1f, 0f);
            header.Size = new(1f, 26f);
            header.Color = new(0.2f, 0.2f, 0.2f, 1);
            header.Position = new(0f, 0f);
            header.SetParent(background);
            header.AddComponent(new IsSelectable(canvas.SelectionMask));

            rint targetReference = header.AddReference(background);
            header.AddComponent(new IsDraggable(targetReference));

            Label title = new(canvas, titleText);
            title.SetParent(header);
            title.Anchor = Anchor.TopLeft;
            title.Color = new(1, 1, 1, 1);
            title.Position = new(4f, -4f);
            title.Pivot = new(0f, 1f, 0f);

            Button closeButton = new(new(&PressedWindowCloseButton), canvas);
            closeButton.SetParent(header);
            closeButton.Color = new(1, 0, 0, 1);
            closeButton.Anchor = Anchor.TopRight;
            closeButton.Size = new(18f, 18f);
            closeButton.Pivot = new(1f, 1f, 0f);
            closeButton.Position = new(-4f, -4f);

            //close button x shape
            {
                Vector4 lineColor = Vector4.Lerp(new(0, 0, 0, 1), new(1, 0, 0, 1), 0.3f);

                Image line1 = new(canvas);
                line1.SetParent(closeButton);
                line1.Color = lineColor;
                line1.Size = new(18f, 2.5f);
                line1.Rotation = MathF.PI * 0.25f;
                line1.Position = new(3f, 2f);

                Image line2 = new(canvas);
                line2.SetParent(closeButton);
                line2.Color = lineColor;
                line2.Size = new(18f, 2f);
                line2.Rotation = MathF.PI * -0.25f;
                line2.Position = new(2f, 15f);
            }

            ScrollBar scrollBar = new(canvas, Vector2.UnitY, 0.5f);
            scrollBar.SetParent(background);
            scrollBar.Size = new(24f, 1f);
            scrollBar.Anchor = new(new(1f, false), new(0f, false), default, new(1f, false), new(26f, true), default);
            scrollBar.Pivot = new(1f, 0f, 0f);
            scrollBar.BackgroundColor = new(0.2f, 0.2f, 0.2f, 1);
            scrollBar.ScrollHandleColor = new(1, 1, 1, 1);
            scrollBar.Value = new(0f, 1f);

            View view = new(world, canvas);
            view.SetParent(background);
            view.ViewPosition = new(0f, 0f);
            view.Anchor = new(new(0f, false), new(0, false), default, new(24f, true), new(26f, true), default);
            view.ContentSize = new(100f, 100f);
            view.SetScrollBar(scrollBar);

            rint headerReference = background.AddReference(header);
            rint titleLabelReference = background.AddReference(title);
            rint closeButtonReference = background.AddReference(closeButton);
            rint scrollBarReference = background.AddReference(scrollBar);
            rint viewReference = background.AddReference(view);
            background.AddComponent(new IsVirtualWindow(headerReference, titleLabelReference, closeButtonReference, scrollBarReference, viewReference, closeCallback));

            DropShadow dropShadow = new(canvas, background);
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsVirtualWindow>();
        }

        [UnmanagedCallersOnly]
        private static void PressedWindowCloseButton(Entity closeButtonEntity)
        {
            Entity headerEntity = closeButtonEntity.Parent;
            Entity windowEntity = headerEntity.Parent;
            VirtualWindow virtualWindow = windowEntity.As<VirtualWindow>();
            ref IsVirtualWindow component = ref windowEntity.GetComponent<IsVirtualWindow>();
            if (component.closeCallback != default)
            {
                component.closeCallback.Invoke(virtualWindow);
            }
            else
            {
                virtualWindow.Dispose();
            }
        }

        public unsafe static VirtualWindow Create<T>(World world, Canvas canvas) where T : unmanaged, IVirtualWindow
        {
            FixedString title = default(T).Title;
            VirtualWindowClose closeCallback = default(T).CloseCallback;
            VirtualWindow window = new(world, canvas, title, closeCallback);
            default(T).OnCreated(window.Container, canvas);
            return window;
        }

        public static implicit operator Image(VirtualWindow window)
        {
            return window.As<Image>();
        }

        public static implicit operator Transform(VirtualWindow window)
        {
            return window.As<Transform>();
        }
    }
}