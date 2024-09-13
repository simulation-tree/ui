using Data;
using InteractionKit.Components;
using Simulation;
using System.Diagnostics;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct Tree : IEntity
    {
        public readonly Transform transform;

        public readonly Entity Parent
        {
            get => transform.Parent;
            set => transform.Parent = value;
        }

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

        public readonly ref Anchor Anchor => ref transform.entity.GetComponentRef<Anchor>();
        public readonly ref Vector3 Pivot => ref transform.entity.GetComponentRef<Pivot>().value;
        public readonly USpan<SelectedLeaf> Selected => transform.AsEntity().GetArray<SelectedLeaf>();
        public readonly USpan<TreeNodeOption> Nodes => transform.AsEntity().GetArray<TreeNodeOption>();
        public readonly InteractiveContext Context => transform.AsEntity().GetComponent<IsTree>().context;

        readonly uint IEntity.Value => transform.GetEntityValue();
        readonly World IEntity.World => transform.GetWorld();
        readonly Definition IEntity.Definition => new Definition().AddComponentType<IsTree>().AddArrayTypes<SelectedLeaf, TreeNodeOption>();

        public Tree(World world, InteractiveContext context)
        {
            transform = new(world);
            transform.LocalPosition = new(0, 0, 0.1f);
            transform.AsEntity().AddComponent(new Anchor());
            transform.AsEntity().AddComponent(new Pivot());
            transform.AsEntity().AddComponent(new IsTree(context));
            transform.AsEntity().CreateArray<SelectedLeaf>();
            transform.AsEntity().CreateArray<TreeNodeOption>();
        }

        public unsafe readonly TreeNode AddLeaf(FixedString text)
        {
            Vector2 size = Size;
            uint nodeCount = transform.AsEntity().GetArrayLength<TreeNodeOption>();
            TreeNode node = new(transform.GetWorld(), text, Context);
            node.Parent = transform;
            node.Position = new(0, -nodeCount * size.Y);
            node.Size = size;
            rint nodeReference = transform.AddReference(node);
            USpan<TreeNodeOption> nodes = transform.AsEntity().ResizeArray<TreeNodeOption>(nodeCount + 1);
            nodes[nodeCount] = new(nodeReference);
            return node;
        }

        public readonly bool IsSelected(TreeNode node)
        {
            USpan<SelectedLeaf> selected = Selected;
            for (uint i = 0; i < selected.length; i++)
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
                node.box.Color = Color.Red;
            }
            else
            {
                ThrowIfNotSelected(node);
                USpan<SelectedLeaf> selected = transform.AsEntity().GetArray<SelectedLeaf>();
                uint index = 0;
                for (uint i = 0; i < selected.length; i++)
                {
                    rint nodeReference = selected[i].nodeReference;
                    uint nodeEntity = transform.GetReference(nodeReference);
                    if (nodeEntity == node.GetEntityValue())
                    {
                        index = i;
                        break;
                    }
                }

                for (uint i = index; i < selected.length - 1; i++)
                {
                    selected[i] = selected[i + 1];
                }

                transform.AsEntity().ResizeArray<SelectedLeaf>(selectedCount - 1);
                node.box.Color = Color.White;
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
    }
}