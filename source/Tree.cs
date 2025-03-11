using System;
using System.Diagnostics;
using System.Numerics;
using Transforms;
using Transforms.Components;
using UI.Components;
using Unmanaged;
using Worlds;

namespace UI
{
    public readonly partial struct Tree : IEntity
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
        public readonly Span<SelectedLeaf> Selected => GetArray<SelectedLeaf>().AsSpan();
        public readonly Span<TreeNodeOption> Nodes => GetArray<TreeNodeOption>().AsSpan();

        public Tree(Canvas canvas)
        {
            world = canvas.world;
            Transform transform = new(world);
            value = transform.value;

            transform.LocalPosition = new(0, 0, Settings.ZScale);
            SetParent(canvas);
            AddComponent(new Anchor());
            AddComponent(new Pivot());
            AddTag<IsTree>();
            CreateArray<SelectedLeaf>();
            CreateArray<TreeNodeOption>();
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddTagType<IsTree>();
            archetype.AddArrayType<SelectedLeaf>();
            archetype.AddArrayType<TreeNodeOption>();
        }

        public unsafe readonly TreeNode AddLeaf(ASCIIText256 text)
        {
            Vector2 size = Size;
            Values<TreeNodeOption> options = GetArray<TreeNodeOption>();
            int nodeCount = options.Length;
            TreeNode node = new(text, this.GetCanvas());
            node.SetParent(value);
            node.Position = new(0, -nodeCount * size.Y);
            node.Size = size;
            rint nodeReference = AddReference(node);
            options.Add(new(nodeReference));
            return node;
        }

        public readonly void UpdatePositions()
        {
            Vector2 size = Size;
            Span<TreeNodeOption> nodes = Nodes;
            float y = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                rint nodeReference = nodes[i].childNodeReference;
                uint nodeEntity = GetReference(nodeReference);
                TreeNode node = new Entity(world, nodeEntity).As<TreeNode>();
                node.Position = new(0, -y);
                node.Size = size;
                if (node.IsExpanded)
                {
                    y += node.UpdatePositions();
                }
                else
                {
                    y += size.Y;
                }
            }
        }

        public readonly bool IsSelected(TreeNode node)
        {
            Span<SelectedLeaf> selected = Selected;
            for (int i = 0; i < selected.Length; i++)
            {
                rint nodeReference = selected[i].nodeReference;
                uint nodeEntity = GetReference(nodeReference);
                if (nodeEntity == node.value)
                {
                    return true;
                }
            }

            return false;
        }

        public readonly void SetSelected(TreeNode node, bool state)
        {
            Values<SelectedLeaf> selected = GetArray<SelectedLeaf>();
            int selectedCount = selected.Length;
            if (state)
            {
                ThrowIfSelected(node);

                rint nodeReference = AddReference(node);
                selected.Add(new(nodeReference));
                node.BackgroundColor = new(0, 0.5f, 1, 1);
                node.Label.Color = new(1, 1, 1, 1);
            }
            else
            {
                ThrowIfNotSelected(node);

                int index = 0;
                for (int i = 0; i < selected.Length; i++)
                {
                    rint nodeReference = selected[i].nodeReference;
                    uint nodeEntity = GetReference(nodeReference);
                    if (nodeEntity == node.value)
                    {
                        index = i;
                        break;
                    }
                }

                //shift back
                selected.RemoveAt(index);
                node.BackgroundColor = new(1, 1, 1, 1);
                node.Label.Color = new(0, 0, 0, 1);
            }
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfSelected(TreeNode node)
        {
            if (IsSelected(node))
            {
                throw new System.InvalidOperationException();
            }
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfNotSelected(TreeNode node)
        {
            if (!IsSelected(node))
            {
                throw new System.InvalidOperationException();
            }
        }

        public static implicit operator Transform(Tree tree)
        {
            return tree.As<Transform>();
        }
    }
}