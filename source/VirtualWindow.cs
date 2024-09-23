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
        public readonly Image box;

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

        public readonly Entity Container
        {
            get
            {
                IsVirtualWindow component = box.AsEntity().GetComponent<IsVirtualWindow>();
                rint viewReference = component.viewReference;
                uint viewEntity = box.GetReference(viewReference);
                View view = new Entity(box.GetWorld(), viewEntity).As<View>();
                return view.Content;
            }
        }

        public readonly Entity Header
        {
            get
            {
                IsVirtualWindow component = box.AsEntity().GetComponent<IsVirtualWindow>();
                rint headerReference = component.headerReference;
                uint headerEntity = box.GetReference(headerReference);
                return new(box.GetWorld(), headerEntity);
            }
        }

        public readonly ref Color BackgroundColor => ref box.Color;

        readonly uint IEntity.Value => box.GetEntityValue();
        readonly World IEntity.World => box.GetWorld();
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsVirtualWindow>()], []);

        public VirtualWindow(World world, uint existingEntity)
        {
            box = new(world, existingEntity);
        }

        private unsafe VirtualWindow(World world, InteractiveContext context, FixedString titleText, VirtualWindowCloseFunction closeCallback)
        {
            box = new(world, context);

            Image header = new(world, context);
            header.Anchor = new(new(0f, false), new(1f, false), default, new(1f, false), new(1f, false), default);
            header.Pivot = new(0f, 1f, 0f);
            header.Size = new(1f, 26f);
            header.Color = new Color(0.2f, 0.2f, 0.2f);
            header.transform.LocalPosition = new(0f, 0f, 1f);
            header.Parent = box;
            header.AddComponent(new IsSelectable());

            rint targetReference = header.AddReference(box);
            header.AddComponent(new IsDraggable(targetReference));

            Label title = new(world, context, titleText);
            title.Parent = header;
            title.Anchor = Anchor.TopLeft;
            title.Color = Color.White;
            title.Position = new(4f, -4f);
            title.Pivot = new(0f, 1f, 0f);

            Button closeButton = new(world, new(&PressedWindowCloseButton), context);
            closeButton.Parent = header;
            closeButton.Color = Color.Red;
            closeButton.Anchor = Anchor.TopRight;
            closeButton.image.transform.LocalPosition = new(-4f, -4f, 0.1f);
            closeButton.Size = new(18f, 18f);
            closeButton.Pivot = new(1f, 1f, 0f);

            ScrollBar scrollBar = new(world, context, Vector2.UnitY, 0.5f);
            scrollBar.Parent = box;
            scrollBar.Size = new(24f, 1f);
            scrollBar.Anchor = new(new(1f, false), new(0f, false), default, new(1f, false), new(26f, true), default);
            scrollBar.Pivot = new(1f, 0f, 0f);
            scrollBar.BackgroundColor = new(0.2f, 0.2f, 0.2f);
            scrollBar.ScrollHandleColor = Color.White;
            scrollBar.Value = new(0f, 1f);

            View view = new(world, context);
            view.Parent = box;
            view.ViewPosition = new(0f, 0f);
            view.Anchor = new(new(0f, false), new(0, false), default, new(24f, true), new(26f, true), default);
            view.ContentSize = new(100f, 100f);
            view.SetScrollBar(scrollBar);

            rint headerReference = box.AddReference(header);
            rint titleLabelReference = box.AddReference(title);
            rint closeButtonReference = box.AddReference(closeButton);
            rint scrollBarReference = box.AddReference(scrollBar);
            rint viewReference = box.AddReference(view);
            box.AddComponent(new IsVirtualWindow(headerReference, titleLabelReference, closeButtonReference, scrollBarReference, viewReference, closeCallback));
        }

        [UnmanagedCallersOnly]
        private static void PressedWindowCloseButton(World world, uint closeButtonEntity)
        {
            uint headerEntity = world.GetParent(closeButtonEntity);
            uint windowEntity = world.GetParent(headerEntity);
            VirtualWindow window = new(world, windowEntity);
            IsVirtualWindow component = world.GetComponent<IsVirtualWindow>(windowEntity);
            component.closeCallback.Invoke(window);
        }

        public unsafe static VirtualWindow Create<T>(World world, InteractiveContext context) where T : unmanaged, IVirtualWindow
        {
            FixedString title = default(T).Title;
            VirtualWindowCloseFunction closeCallback = default(T).CloseCallback;
            VirtualWindow window = new(world, context, title, closeCallback);
            default(T).OnCreated(window, context);
            return window;
        }
    }
}