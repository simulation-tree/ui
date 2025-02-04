using UI.Components;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Worlds;

namespace UI
{
    public readonly partial struct View : IEntity
    {
        public unsafe readonly ref Vector2 ViewPosition
        {
            get
            {
                ref Vector3 localPosition = ref As<Transform>().LocalPosition;
                fixed (Vector3* pLocalPosition = &localPosition)
                {
                    return ref *(Vector2*)pLocalPosition;
                }
            }
        }

        public unsafe readonly ref Vector2 ViewSize
        {
            get
            {
                ref Vector3 localScale = ref As<Transform>().LocalScale;
                fixed (Vector3* pLocalScale = &localScale)
                {
                    return ref *(Vector2*)pLocalScale;
                }
            }
        }

        public readonly Transform Content
        {
            get
            {
                rint contentReference = GetComponent<IsView>().contentReference;
                uint contentEntity = GetReference(contentReference);
                return new Entity(world, contentEntity).As<Transform>();
            }
        }

        public unsafe readonly ref Vector2 ContentPosition
        {
            get
            {
                Transform content = Content;
                ref Vector3 localPosition = ref content.LocalPosition;
                fixed (Vector3* pLocalPosition = &localPosition)
                {
                    return ref *(Vector2*)pLocalPosition;
                }
            }
        }

        public unsafe readonly ref Vector2 ContentSize
        {
            get
            {
                Transform content = Content;
                ref Vector3 localScale = ref content.LocalScale;
                fixed (Vector3* pLocalScale = &localScale)
                {
                    return ref *(Vector2*)pLocalScale;
                }
            }
        }

        public readonly ref Anchor Anchor => ref GetComponent<Anchor>();
        public readonly ref Vector3 Pivot => ref GetComponent<Pivot>().value;

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
    }
}