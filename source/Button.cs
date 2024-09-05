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
        public readonly Selectable selectable;

        public readonly Vector2 Size
        {
            get => selectable.Size;
            set => selectable.Size = value;
        }

        public readonly Vector2 Position
        {
            get => selectable.Position;
            set => selectable.Position = value;
        }

        public readonly ref Anchor Anchor => ref selectable.Anchor;
        public readonly ref Vector3 Pivot => ref selectable.Pivot;
        public readonly ref Color Color => ref selectable.Color;

        readonly uint IEntity.Value => selectable.transform.entity.value;
        readonly World IEntity.World => selectable.transform.entity.world;
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsSelectable>(), RuntimeType.Get<IsTrigger>()], []);

#if NET
        [Obsolete("Default constructor not available", true)]
        public Button()
        {
            throw new NotSupportedException();
        }
#endif

        public unsafe Button(World world, CallbackFunction callback, InteractiveContext context)
        {
            selectable = new(world, context);
            selectable.transform.entity.AddComponent(new IsTrigger(new(&Filter), callback));
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
    }
}