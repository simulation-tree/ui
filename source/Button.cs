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
        public readonly ref Color Color => ref box.Color;

        readonly uint IEntity.Value => box.GetEntityValue();
        readonly World IEntity.World => box.GetWorld();
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsTrigger>(), RuntimeType.Get<IsSelectable>()], []);

#if NET
        [Obsolete("Default constructor not available", true)]
        public Button()
        {
            throw new NotSupportedException();
        }
#endif

        public unsafe Button(World world, CallbackFunction callback, InteractiveContext context)
        {
            box = new(world, context);
            box.AsEntity().AddComponent(new IsTrigger(new(&Filter), callback));
            box.AsEntity().AddComponent(new IsSelectable());
        }

        public readonly Entity AsEntity()
        {
            return box.AsEntity();
        }

        [UnmanagedCallersOnly]
        private static void Filter(FilterFunction.Input input)
        {
            World world = input.world;
            foreach (ref uint entity in input.Entities)
            {
                //todo: efficiency: doing individual calls within a filter function
                IsSelectable component = world.GetComponent<IsSelectable>(entity);
                bool pressed = (component.state & IsSelectable.State.WasPrimaryInteractedWith) != 0;
                if (!pressed)
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