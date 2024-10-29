using InteractionKit.Components;
using Rendering;
using Simulation;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct View : IEntity
    {
        public readonly Transform transform;

        public readonly Entity Parent
        {
            get => transform.Parent;
            set => transform.Parent = value;
        }

        public readonly Vector2 ViewPosition
        {
            get
            {
                Vector3 position = transform.LocalPosition;
                return new(position.X, position.Y);
            }
            set
            {
                Vector3 position = transform.LocalPosition;
                position.X = value.X;
                position.Y = value.Y;
                transform.LocalPosition = position;
            }
        }

        public readonly Vector2 ViewSize
        {
            get
            {
                Vector3 scale = transform.LocalScale;
                return new(scale.X, scale.Y);
            }
            set
            {
                Vector3 scale = transform.LocalScale;
                scale.X = value.X;
                scale.Y = value.Y;
                transform.LocalScale = scale;
            }
        }

        public readonly Entity Content
        {
            get
            {
                rint contentReference = transform.AsEntity().GetComponent<IsView>().contentReference;
                uint contentEntity = transform.GetReference(contentReference);
                return new(transform.GetWorld(), contentEntity);
            }
        }

        public readonly Vector2 ContentPosition
        {
            get
            {
                Transform content = Content.As<Transform>();
                Vector3 position = content.LocalPosition;
                return new(position.X, position.Y);
            }
            set
            {
                Transform content = Content.As<Transform>();
                Vector3 position = content.LocalPosition;
                position.X = value.X;
                position.Y = value.Y;
                content.LocalPosition = position;
            }
        }

        public readonly Vector2 ContentSize
        {
            get
            {
                Transform content = Content.As<Transform>();
                Vector3 scale = content.LocalScale;
                return new(scale.X, scale.Y);
            }
            set
            {
                Transform content = Content.As<Transform>();
                Vector3 scale = content.LocalScale;
                scale.X = value.X;
                scale.Y = value.Y;
                content.LocalScale = scale;
            }
        }

        public readonly ref Anchor Anchor => ref transform.AsEntity().GetComponentRef<Anchor>();
        public readonly ref Vector3 Pivot => ref transform.AsEntity().GetComponentRef<Pivot>().value;

        readonly uint IEntity.Value => transform.GetEntityValue();
        readonly World IEntity.World => transform.GetWorld();
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsView>()], []);

        public View(World world, Canvas canvas)
        {
            transform = new Transform(world);
            transform.LocalPosition = new(0f, 0f, 0.1f);
            transform.Parent = canvas;
            transform.AddComponent(new Anchor());
            transform.AddComponent(new Pivot());

            Transform content = new(world);
            content.Parent = transform;
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
            ref ViewScrollBarLink link = ref transform.AsEntity().TryGetComponentRef<ViewScrollBarLink>(out bool contains);
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
    }
}