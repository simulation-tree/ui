using Data;
using InteractionKit.Components;
using InteractionKit.Functions;
using Simulation;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct Button : ISelectable
    {
        public readonly Image image;

        public readonly Entity Parent
        {
            get => image.Parent;
            set => image.Parent = value;
        }

        public readonly Vector2 Position
        {
            get => image.Position;
            set => image.Position = value;
        }

        public readonly Vector2 Size
        {
            get => image.Size;
            set => image.Size = value;
        }

        public readonly ref Anchor Anchor => ref image.Anchor;
        public readonly ref Vector3 Pivot => ref image.Pivot;
        public readonly ref Color Color => ref image.Color;

        readonly uint IEntity.Value => image.GetEntityValue();
        readonly World IEntity.World => image.GetWorld();
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsTrigger>(), RuntimeType.Get<IsSelectable>()], []);

#if NET
        [Obsolete("Default constructor not available", true)]
        public Button()
        {
            throw new NotSupportedException();
        }
#endif

        public unsafe Button(World world, CallbackFunction callback, Canvas canvas)
        {
            image = new(world, canvas);
            image.AddComponent(new IsTrigger(new(&Filter), callback));
            image.AddComponent(new IsSelectable());
        }

        public readonly void Dispose()
        {
            image.Dispose();
        }

        [UnmanagedCallersOnly]
        private static void Filter(FilterFunction.Input input)
        {
            foreach (ref Entity entity in input.Entities)
            {
                //todo: efficiency: doing individual calls within a filter function
                IsSelectable component = entity.GetComponent<IsSelectable>();
                if (!component.WasPrimaryInteractedWith || !component.IsSelected)
                {
                    entity = default;
                }
            }
        }

        public static implicit operator Entity(Button button)
        {
            return button.AsEntity();
        }
    }
}