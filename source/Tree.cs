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
        public readonly USpan<SelectedLeaf> Selected => GetArray<SelectedLeaf>();
        public readonly USpan<TreeNodeOption> Nodes => GetArray<TreeNodeOption>();

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

        public unsafe readonly TreeNode AddLeaf(FixedString text)
        {
            Vector2 size = Size;
            uint nodeCount = GetArrayLength<TreeNodeOption>();
            TreeNode node = new(text, this.GetCanvas());
            node.SetParent(value);
            node.Position = new(0, -nodeCount * size.Y);
            node.Size = size;
            rint nodeReference = AddReference(node);
            USpan<TreeNodeOption> nodes = ResizeArray<TreeNodeOption>(nodeCount + 1);
            nodes[nodeCount] = new(nodeReference);
            return node;
        }

        public readonly void UpdatePositions()
        {
            Vector2 size = Size;
            USpan<TreeNodeOption> nodes = Nodes;
            float y = 0;
            for (uint i = 0; i < nodes.Length; i++)
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
            USpan<SelectedLeaf> selected = Selected;
            for (uint i = 0; i < selected.Length; i++)
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
            uint selectedCount = GetArrayLength<SelectedLeaf>();
            if (state)
            {
                ThrowIfSelected(node);

                rint nodeReference = AddReference(node);
                USpan<SelectedLeaf> selected = ResizeArray<SelectedLeaf>(selectedCount + 1);
                selected[selectedCount] = new(nodeReference);
                node.BackgroundColor = new(0, 0.5f, 1, 1);
                node.Label.Color = new(1, 1, 1, 1);
            }
            else
            {
                ThrowIfNotSelected(node);

                USpan<SelectedLeaf> selected = GetArray<SelectedLeaf>();
                uint index = 0;
                for (uint i = 0; i < selected.Length; i++)
                {
                    rint nodeReference = selected[i].nodeReference;
                    uint nodeEntity = GetReference(nodeReference);
                    if (nodeEntity == node.value)
                    {
                        index = i;
                        break;
                    }
                }

                for (uint i = index; i < selected.Length - 1; i++)
                {
                    selected[i] = selected[i + 1];
                }

                ResizeArray<SelectedLeaf>(selectedCount - 1);
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