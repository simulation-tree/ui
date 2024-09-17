using Data;
using InteractionKit.Components;
using InteractionKit.Functions;
using Simulation;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms;
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
                rint containerReference = component.containerReference;
                uint containerEntity = box.AsEntity().GetReference(containerReference);
                return new(box.GetWorld(), containerEntity);
            }
        }

        public readonly Entity Header
        {
            get
            {
                IsVirtualWindow component = box.AsEntity().GetComponent<IsVirtualWindow>();
                rint headerReference = component.headerReference;
                uint headerEntity = box.AsEntity().GetReference(headerReference);
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
            header.transform.LocalPosition = new(0f, 0f, 0.1f);
            header.Parent = box.AsEntity();
            header.AsEntity().AddComponent<IsSelectable>();

            rint targetReference = header.AsEntity().AddReference(box);
            header.AsEntity().AddComponent(new IsDraggable(targetReference));

            Transform container = new(world);
            container.entity.AddComponent(new Anchor(new(0f, false), new(0f, false), default, new(1f, false), new(26f, true), default));
            container.LocalPosition = new(0f, 0f, 0.1f);
            container.Parent = box.AsEntity();

            Label title = new(world, context, titleText);
            title.Parent = header.AsEntity();
            title.Anchor = Anchor.TopLeft;
            title.Color = Color.White;
            title.Position = new(4f, -4f);
            title.Pivot = new(0f, 1f, 0f);

            Button closeButton = new(world, new(&PressedWindowCloseButton), context);
            closeButton.Parent = header.AsEntity();
            closeButton.Color = Color.Red;
            closeButton.Anchor = Anchor.TopRight;
            closeButton.box.transform.LocalPosition = new(-4f, -4f, 0.1f);
            closeButton.Size = new(18f, 18f);
            closeButton.Pivot = new(1f, 1f, 0f);

            rint headerReference = box.AsEntity().AddReference(header.AsEntity());
            rint containerReference = box.AsEntity().AddReference(container.AsEntity());
            rint titleLabelReference = box.AsEntity().AddReference(title.AsEntity());
            rint closeButtonReference = box.AsEntity().AddReference(closeButton.AsEntity());
            box.AsEntity().AddComponent(new IsVirtualWindow(headerReference, containerReference, titleLabelReference, closeButtonReference, closeCallback));
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