using Data;
using InteractionKit.Components;
using InteractionKit.Functions;
using Simulation;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct VirtualWindow : IEntity
    {
        public readonly Image background;

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

        public readonly Entity Container
        {
            get
            {
                IsVirtualWindow component = background.AsEntity().GetComponent<IsVirtualWindow>();
                rint viewReference = component.viewReference;
                uint viewEntity = background.GetReference(viewReference);
                View view = new Entity(background.GetWorld(), viewEntity).As<View>();
                return view.Content;
            }
        }

        public readonly Entity Header
        {
            get
            {
                IsVirtualWindow component = background.AsEntity().GetComponent<IsVirtualWindow>();
                rint headerReference = component.headerReference;
                uint headerEntity = background.GetReference(headerReference);
                return new(background.GetWorld(), headerEntity);
            }
        }

        public readonly ref Color BackgroundColor => ref background.Color;

        readonly uint IEntity.Value => background.GetEntityValue();
        readonly World IEntity.World => background.GetWorld();
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsVirtualWindow>()], []);

        public VirtualWindow(World world, uint existingEntity)
        {
            background = new(world, existingEntity);
        }

        private unsafe VirtualWindow(World world, Canvas canvas, FixedString titleText, VirtualWindowCloseFunction closeCallback)
        {
            background = new(world, canvas);

            Image header = new(world, canvas);
            header.Anchor = new(new(0f, false), new(1f, false), default, new(1f, false), new(1f, false), default);
            header.Pivot = new(0f, 1f, 0f);
            header.Size = new(1f, 26f);
            header.Color = new Color(0.2f, 0.2f, 0.2f);
            header.transform.LocalPosition = new(0f, 0f, 1f);
            header.Parent = background;
            header.AddComponent(new IsSelectable());

            rint targetReference = header.AddReference(background);
            header.AddComponent(new IsDraggable(targetReference));

            Label title = new(world, canvas, titleText);
            title.Parent = header;
            title.Anchor = Anchor.TopLeft;
            title.Color = Color.White;
            title.Position = new(4f, -4f);
            title.Pivot = new(0f, 1f, 0f);

            Button closeButton = new(world, new(&PressedWindowCloseButton), canvas);
            closeButton.Parent = header;
            closeButton.Color = Color.Red;
            closeButton.Anchor = Anchor.TopRight;
            closeButton.image.transform.LocalPosition = new(-4f, -4f, 0.1f);
            closeButton.Size = new(18f, 18f);
            closeButton.Pivot = new(1f, 1f, 0f);

            ScrollBar scrollBar = new(world, canvas, Vector2.UnitY, 0.5f);
            scrollBar.Parent = background;
            scrollBar.Size = new(24f, 1f);
            scrollBar.Anchor = new(new(1f, false), new(0f, false), default, new(1f, false), new(26f, true), default);
            scrollBar.Pivot = new(1f, 0f, 0f);
            scrollBar.BackgroundColor = new(0.2f, 0.2f, 0.2f);
            scrollBar.ScrollHandleColor = Color.White;
            scrollBar.Value = new(0f, 1f);

            View view = new(world, canvas);
            view.Parent = background;
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
        }

        public readonly void Dispose()
        {
            background.Dispose();
        }

        [UnmanagedCallersOnly]
        private static void PressedWindowCloseButton(Entity closeButtonEntity)
        {
            Entity headerEntity = closeButtonEntity.Parent;
            Entity windowEntity = headerEntity.Parent;
            VirtualWindow window = windowEntity.As<VirtualWindow>();
            IsVirtualWindow component = windowEntity.GetComponent<IsVirtualWindow>();
            component.closeCallback.Invoke(window);
        }

        public unsafe static VirtualWindow Create<T>(World world, Canvas canvas) where T : unmanaged, IVirtualWindow
        {
            FixedString title = default(T).Title;
            VirtualWindowCloseFunction closeCallback = default(T).CloseCallback;
            VirtualWindow window = new(world, canvas, title, closeCallback);
            default(T).OnCreated(window, canvas);
            return window;
        }
    }
}