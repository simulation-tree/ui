using InteractionKit.Components;
using InteractionKit.Functions;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms;
using Transforms.Components;
using Worlds;

namespace InteractionKit
{
    public readonly struct Button : IEntity, ISelectable
    {
        private readonly Image image;

        public readonly ref Vector2 Position => ref image.Position;
        public readonly ref Vector2 Size => ref image.Size;
        public readonly ref float Z => ref image.Z;
        public readonly ref Anchor Anchor => ref image.Anchor;
        public readonly ref Vector3 Pivot => ref image.Pivot;
        public readonly ref Vector4 Color => ref image.Color;

        readonly uint IEntity.Value => image.GetEntityValue();
        readonly World IEntity.World => image.GetWorld();

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsTrigger>();
            archetype.AddComponentType<IsSelectable>();
        }

#if NET
        [Obsolete("Default constructor not available", true)]
        public Button()
        {
            throw new NotSupportedException();
        }
#endif

        public unsafe Button(TriggerCallback callback, Canvas canvas)
        {
            image = new(canvas);
            image.AddComponent(new IsTrigger(new(&Filter), callback));
            image.AddComponent(new IsSelectable(canvas.SelectionMask));
        }

        public readonly void Dispose()
        {
            image.Dispose();
        }

        [UnmanagedCallersOnly]
        private static void Filter(TriggerFilter.Input input)
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

        public static implicit operator Image(Button button)
        {
            return button.image;
        }

        public static implicit operator Transform(Button button)
        {
            return button.image;
        }
    }
}