using Data;
using InteractionKit.Components;
using System.Diagnostics;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Unmanaged;
using Worlds;

namespace InteractionKit
{
    public readonly struct Tree : ICanvasDescendant
    {
        private readonly Transform transform;

        public readonly Vector2 Position
        {
            get
            {
                Vector3 position = transform.LocalPosition;
                return new(position.X, position.Y);
            }
            set
            {
                Vector3 position = transform.LocalPosition;
                transform.LocalPosition = new(value.X, value.Y, position.Z);
            }
        }

        public readonly Vector2 Size
        {
            get
            {
                Vector3 scale = transform.LocalScale;
                return new(scale.X, scale.Y);
            }
            set
            {
                Vector3 scale = transform.LocalScale;
                transform.LocalScale = new(value.X, value.Y, scale.Z);
            }
        }

        public readonly ref Anchor Anchor => ref transform.AsEntity().GetComponent<Anchor>();
        public readonly ref Vector3 Pivot => ref transform.AsEntity().GetComponent<Pivot>().value;
        public readonly USpan<SelectedLeaf> Selected => transform.AsEntity().GetArray<SelectedLeaf>();
        public readonly USpan<TreeNodeOption> Nodes => transform.AsEntity().GetArray<TreeNodeOption>();

        readonly uint IEntity.Value => transform.GetEntityValue();
        readonly World IEntity.World => transform.GetWorld();
        readonly Definition IEntity.Definition => new Definition().AddComponentType<IsTree>().AddArrayTypes<SelectedLeaf, TreeNodeOption>();

        public Tree(Canvas canvas)
        {
            World world = canvas.GetWorld();
            transform = new(world);
            transform.SetParent(canvas);
            transform.LocalPosition = new(0, 0, Settings.ZScale);
            transform.AddComponent(new Anchor());
            transform.AddComponent(new Pivot());
            transform.AddComponent(new IsTree());
            transform.AsEntity().CreateArray<SelectedLeaf>();
            transform.AsEntity().CreateArray<TreeNodeOption>();
        }

        public readonly void Dispose()
        {
            transform.Dispose();
        }

        public unsafe readonly TreeNode AddLeaf(FixedString text)
        {
            Vector2 size = Size;
            uint nodeCount = transform.AsEntity().GetArrayLength<TreeNodeOption>();
            TreeNode node = new(text, this.GetCanvas());
            node.SetParent(transform);
            node.Position = new(0, -nodeCount * size.Y);
            node.Size = size;
            rint nodeReference = transform.AddReference(node);
            USpan<TreeNodeOption> nodes = transform.AsEntity().ResizeArray<TreeNodeOption>(nodeCount + 1);
            nodes[nodeCount] = new(nodeReference);
            return node;
        }

        public readonly void UpdatePositions()
        {
            World world = transform.GetWorld();
            Vector2 size = Size;
            USpan<TreeNodeOption> nodes = Nodes;
            float y = 0;
            for (uint i = 0; i < nodes.Length; i++)
            {
                rint nodeReference = nodes[i].childNodeReference;
                uint nodeEntity = transform.GetReference(nodeReference);
                TreeNode node = new(world, nodeEntity);
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
                uint nodeEntity = transform.GetReference(nodeReference);
                if (nodeEntity == node.GetEntityValue())
                {
                    return true;
                }
            }

            return false;
        }

        public readonly void SetSelected(TreeNode node, bool state)
        {
            uint selectedCount = transform.AsEntity().GetArrayLength<SelectedLeaf>();
            if (state)
            {
                ThrowIfSelected(node);
                rint nodeReference = transform.AddReference(node);
                USpan<SelectedLeaf> selected = transform.AsEntity().ResizeArray<SelectedLeaf>(selectedCount + 1);
                selected[selectedCount] = new(nodeReference);
                node.BackgroundColor = Color.SkyBlue;
                node.Label.Color = Color.White;
            }
            else
            {
                ThrowIfNotSelected(node);
                USpan<SelectedLeaf> selected = transform.AsEntity().GetArray<SelectedLeaf>();
                uint index = 0;
                for (uint i = 0; i < selected.Length; i++)
                {
                    rint nodeReference = selected[i].nodeReference;
                    uint nodeEntity = transform.GetReference(nodeReference);
                    if (nodeEntity == node.GetEntityValue())
                    {
                        index = i;
                        break;
                    }
                }

                for (uint i = index; i < selected.Length - 1; i++)
                {
                    selected[i] = selected[i + 1];
                }

                transform.AsEntity().ResizeArray<SelectedLeaf>(selectedCount - 1);
                node.BackgroundColor = Color.White;
                node.Label.Color = Color.Black;
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
            return tree.transform;
        }

        public static implicit operator Entity(Tree tree)
        {
            return tree.transform;
        }
    }
}