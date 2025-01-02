using InteractionKit.Components;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Worlds;

namespace InteractionKit
{
    public readonly struct View : IEntity
    {
        private readonly Transform transform;

        public unsafe readonly ref Vector2 ViewPosition
        {
            get
            {
                ref Vector3 localPosition = ref transform.LocalPosition;
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
                ref Vector3 localScale = ref transform.LocalScale;
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
                rint contentReference = transform.AsEntity().GetComponent<IsView>().contentReference;
                uint contentEntity = transform.GetReference(contentReference);
                return new(transform.GetWorld(), contentEntity);
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

        public readonly ref Anchor Anchor => ref transform.AsEntity().GetComponent<Anchor>();
        public readonly ref Vector3 Pivot => ref transform.AsEntity().GetComponent<Pivot>().value;

        readonly uint IEntity.Value => transform.GetEntityValue();
        readonly World IEntity.World => transform.GetWorld();

        readonly Definition IEntity.GetDefinition(Schema schema)
        {
            return new Definition().AddComponentType<IsView>(schema);
        }

        public View(World world, Canvas canvas)
        {
            transform = new Transform(world);
            transform.LocalPosition = new(0f, 0f, Settings.ZScale);
            transform.SetParent(canvas);
            transform.AddComponent(new Anchor());
            transform.AddComponent(new Pivot());

            Transform content = new(world);
            content.SetParent(transform);
            content.AddComponent(new Anchor());
            rint contentReference = transform.AddReference(content);
            transform.AddComponent(new IsView(contentReference));
        }

        public readonly void Dispose()
        {
            transform.Dispose();
        }

        public readonly void SetScrollBar(ScrollBar scrollBar)
        {
            ref ViewScrollBarLink link = ref transform.AsEntity().TryGetComponent<ViewScrollBarLink>(out bool contains);
            if (!contains)
            {
                rint scrollBarReference = transform.AddReference(scrollBar);
                transform.AddComponent(new ViewScrollBarLink(scrollBarReference));
            }
            else
            {
                if (link.scrollBarReference != default)
                {
                    transform.SetReference(link.scrollBarReference, scrollBar);
                }
                else
                {
                    rint scrollBarReference = transform.AddReference(scrollBar);
                    link.scrollBarReference = scrollBarReference;
                }
            }
        }

        public static implicit operator Entity(View view)
        {
            return view.transform;
        }

        public static implicit operator Transform(View view)
        {
            return view.transform;
        }
    }
}