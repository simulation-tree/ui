using System.Numerics;
using System.Runtime.InteropServices;
using Transforms;
using Transforms.Components;
using UI.Components;
using UI.Functions;
using Worlds;

namespace UI
{
    public readonly partial struct Button : ISelectable
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
        public readonly ref Vector4 Color => ref As<Image>().Color;

        public unsafe Button(TriggerCallback callback, Canvas canvas)
        {
            world = canvas.world;
            Image image = new(canvas);
            value = image.value;

            image.AddComponent(new IsTrigger(new(&Filter), callback));
            image.AddComponent(new IsSelectable(canvas.SelectionMask));
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsTrigger>();
            archetype.AddComponentType<IsSelectable>();
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

        public static implicit operator Image(Button button)
        {
            return button.As<Image>();
        }

        public static implicit operator UITransform(Button button)
        {
            return button.As<UITransform>();
        }

        public static implicit operator Transform(Button button)
        {
            return button.As<Transform>();
        }
    }
}