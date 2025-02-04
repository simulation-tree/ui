using System.Numerics;
using Transforms;
using Transforms.Components;
using UI.Components;
using Worlds;

namespace UI
{
    public readonly partial struct View : IEntity
    {
        public readonly ref Vector2 ViewPosition => ref As<UITransform>().Position;
        public readonly ref float X => ref As<UITransform>().X;
        public readonly ref float Y => ref As<UITransform>().Y;
        public readonly ref float Z => ref As<UITransform>().Z;
        public readonly ref Vector2 ViewSize => ref As<UITransform>().Size;
        public readonly ref float Width => ref As<UITransform>().Width;
        public readonly ref float Height => ref As<UITransform>().Height;

        public readonly float Rotation
        {
            get => As<UITransform>().Rotation;
            set => As<UITransform>().Rotation = value;
        }

        public readonly ref Anchor Anchor => ref As<UITransform>().Anchor;
        public readonly ref Vector3 Pivot => ref As<UITransform>().Pivot;

        public readonly UITransform Content
        {
            get
            {
                rint contentReference = GetComponent<IsView>().contentReference;
                uint contentEntity = GetReference(contentReference);
                return new Entity(world, contentEntity).As<UITransform>();
            }
        }

        public readonly ref Vector2 ContentPosition => ref Content.Position;
        public readonly ref Vector2 ContentSize => ref Content.Size;

        public View(World world, Canvas canvas)
        {
            this.world = world;
            Transform transform = new(world);
            value = transform.value;
            transform.LocalPosition = new(0f, 0f, Settings.ZScale);

            SetParent(canvas);
            AddComponent(new Anchor());
            AddComponent(new Pivot());

            Transform content = new(world);
            content.SetParent(transform);
            content.AddComponent(new Anchor());

            rint contentReference = transform.AddReference(content);
            AddComponent(new IsView(contentReference));
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsView>();
        }

        public readonly void SetScrollBar(ScrollBar scrollBar)
        {
            ref ViewScrollBarLink link = ref TryGetComponent<ViewScrollBarLink>(out bool contains);
            if (!contains)
            {
                rint scrollBarReference = AddReference(scrollBar);
                AddComponent(new ViewScrollBarLink(scrollBarReference));
            }
            else
            {
                if (link.scrollBarReference != default)
                {
                    SetReference(link.scrollBarReference, scrollBar);
                }
                else
                {
                    rint scrollBarReference = AddReference(scrollBar);
                    link.scrollBarReference = scrollBarReference;
                }
            }
        }

        public static implicit operator Transform(View view)
        {
            return view.As<Transform>();
        }

        public static implicit operator UITransform(View view)
        {
            return view.As<UITransform>();
        }
    }
}