using UI.Components;
using UI.Functions;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms;
using Transforms.Components;
using Worlds;

namespace UI
{
    public readonly partial struct Button : IEntity, ISelectable
    {
        public readonly ref Vector2 Position => ref As<Image>().Position;
        public readonly ref Vector2 Size => ref As<Image>().Size;
        public readonly ref float Z => ref As<Image>().Z;
        public readonly ref Anchor Anchor => ref As<Image>().Anchor;
        public readonly ref Vector3 Pivot => ref As<Image>().Pivot;
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

        public static implicit operator Transform(Button button)
        {
            return button.As<Transform>();
        }
    }
}