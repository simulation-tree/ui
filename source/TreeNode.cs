using Data;
using InteractionKit.Components;
using InteractionKit.Functions;
using Simulation;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct TreeNode : IEntity
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

        public readonly ref Anchor Anchor => ref transform.AsEntity().GetComponentRef<Anchor>();
        public readonly ref Vector3 Pivot => ref transform.AsEntity().GetComponentRef<Pivot>().value;
        public readonly USpan<TreeNodeOption> Nodes => transform.AsEntity().GetArray<TreeNodeOption>();

        public readonly Label Label
        {
            get
            {
                uint labelEntity = transform.GetReference(transform.AsEntity().GetComponent<IsTreeNode>().labelReference);
                return new(transform.GetWorld(), labelEntity);
            }
        }

        public readonly ref Color BackgroundColor
        {
            get
            {
                rint boxReference = transform.AsEntity().GetComponent<IsTreeNode>().boxReference;
                uint boxEntity = transform.GetReference(boxReference);
                return ref new Entity(transform.GetWorld(), boxEntity).As<Image>().Color;
            }
        }

        public readonly bool IsExpanded => transform.AsEntity().GetComponent<IsTreeNode>().expanded;

        public readonly InteractiveContext Context => transform.AsEntity().GetComponent<IsTreeNode>().context;

        readonly uint IEntity.Value => transform.GetEntityValue();
        readonly World IEntity.World => transform.GetWorld();
        readonly Definition IEntity.Definition => new Definition().AddComponentType<IsTreeNode>().AddArrayType<TreeNodeOption>();

        public TreeNode(World world, uint existingEntity)
        {
            transform = new(world, existingEntity);
        }

        public unsafe TreeNode(World world, FixedString text, InteractiveContext context)
        {
            transform = new(world);
            transform.AsEntity().AddComponent(new Anchor());

            Image box = new(world, context);
            box.Parent = transform;
            box.Color = Color.White;
            box.Anchor = new(new(24f, true), new(0f, false), default, new(1f, false), new(1f, false), default);
            box.AsEntity().AddComponent(new IsTrigger(new(&Filter), new(&ToggleSelected)));
            box.AsEntity().AddComponent(new IsSelectable());

            Label label = new(world, context, text);
            label.Parent = box;
            label.Anchor = Anchor.TopLeft;
            label.Color = Color.Black;
            label.Position = new(4f, -4f);
            label.Pivot = new(0f, 1f, 0f);

            rint boxReference = transform.AddReference(box);
            rint labelReference = transform.AddReference(label);

            transform.AsEntity().AddComponent(new IsTreeNode(text, boxReference, labelReference, context));
            transform.AsEntity().CreateArray<TreeNodeOption>();
        }

        public override string ToString()
        {
            USpan<char> buffer = stackalloc char[256];
            return ToString(buffer).ToString();
        }

        public readonly uint ToString(USpan<char> buffer)
        {
            Label text = Label;
            return text.Text.ToString(buffer);
        }

        public unsafe readonly TreeNode AddLeaf(FixedString text)
        {
            Vector2 size = Size;
            InteractiveContext context = Context;
            World world = transform.GetWorld();
            uint nodeCount = transform.AsEntity().GetArrayLength<TreeNodeOption>();
            if (nodeCount == 0)
            {
                //the button that toggles expanded state
                float triangleButtonSize = 16f;
                Button triangle = new(world, new(&ToggleExpanded), context);
                triangle.Parent = transform.AsEntity();
                triangle.image.Material = context.TriangleMaterial;
                triangle.Anchor = Anchor.TopLeft;
                triangle.Size = new(triangleButtonSize, triangleButtonSize);
                triangle.Color = Color.Black;
                triangle.Position = new(4f, -4f);
                triangle.image.transform.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI * -0.5f);
            }

            TreeNode node = new(world, text, context);
            node.Parent = transform;
            node.Position = new(30, -(nodeCount + 1) * size.Y);
            node.Size = size;
            node.SetEnabled(false);

            rint childNodeReference = transform.AddReference(node);
            USpan<TreeNodeOption> nodes = transform.AsEntity().ResizeArray<TreeNodeOption>(nodeCount + 1);
            nodes[nodeCount] = new(childNodeReference);
            return node;
        }

        public readonly float UpdatePositions()
        {
            Vector2 size = Size;
            USpan<TreeNodeOption> nodes = Nodes;
            float y = size.Y;
            for (uint i = 0; i < nodes.Length; i++)
            {
                rint nodeReference = nodes[i].childNodeReference;
                uint nodeEntity = transform.GetReference(nodeReference);
                TreeNode node = new Entity(transform.GetWorld(), nodeEntity).As<TreeNode>();
                node.Position = new(30, -y);
                if (node.IsExpanded)
                {
                    y += node.UpdatePositions();
                }
                else
                {
                    y += size.Y;
                }
            }

            return y;
        }

        private readonly void UpdateChildEnabledStates()
        {
            bool expanded = IsExpanded;
            USpan<TreeNodeOption> nodes = Nodes;
            for (uint i = 0; i < nodes.Length; i++)
            {
                rint nodeReference = nodes[i].childNodeReference;
                uint nodeEntity = transform.GetReference(nodeReference);
                TreeNode node = new Entity(transform.GetWorld(), nodeEntity).As<TreeNode>();
                node.SetEnabled(expanded);
                if (node.IsExpanded)
                {
                    node.UpdateChildEnabledStates();
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void Filter(FilterFunction.Input input)
        {
            World world = input.world;
            foreach (ref uint entity in input.Entities)
            {
                IsSelectable component = world.GetComponent<IsSelectable>(entity);
                if (!component.WasPrimaryInteractedWith || !component.IsSelected)
                {
                    entity = default;
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void ToggleSelected(World world, uint boxEntity)
        {
            uint nodeEntity = world.GetParent(boxEntity);
            uint parentEntity = world.GetParent(nodeEntity);
            while (parentEntity != default)
            {
                if (world.ContainsComponent<IsTree>(parentEntity))
                {
                    break;
                }
                else if (world.ContainsComponent<IsTreeNode>(parentEntity))
                {
                    parentEntity = world.GetParent(parentEntity);
                }
                else
                {
                    throw new System.Exception();
                }
            }

            Tree tree = new Entity(world, parentEntity).As<Tree>();
            bool selectMultiple = tree.Context.SelectMultiple;
            if (!selectMultiple)
            {
                //deselect all
                USpan<SelectedLeaf> currentSelection = tree.Selected;
                for (uint i = 0; i < currentSelection.Length; i++)
                {
                    rint nodeReference = currentSelection[i].nodeReference;
                    uint selectedNodeEntity = tree.GetReference(nodeReference);
                    TreeNode selectedNode = new Entity(world, selectedNodeEntity).As<TreeNode>();
                    selectedNode.BackgroundColor = Color.White;
                    selectedNode.Label.Color = Color.Black;
                }

                tree.AsEntity().ResizeArray<SelectedLeaf>(0);
            }

            TreeNode node = new Entity(world, nodeEntity).As<TreeNode>();
            bool isSelected = tree.IsSelected(node);
            tree.SetSelected(node, !isSelected);
        }

        [UnmanagedCallersOnly]
        private static void ToggleExpanded(World world, uint expandButtonEntity)
        {
            uint treeNodeEntity = world.GetParent(expandButtonEntity);
            ref IsTreeNode component = ref world.GetComponentRef<IsTreeNode>(treeNodeEntity);
            component.expanded = !component.expanded;
            TreeNode treeNode = new Entity(world, treeNodeEntity).As<TreeNode>();

            ref Rotation rotation = ref world.GetComponentRef<Rotation>(expandButtonEntity);
            rotation.value = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, component.expanded ? MathF.PI : MathF.PI * -0.5f);

            Button triangleButton = new Entity(world, expandButtonEntity).As<Button>();
            if (component.expanded)
            {
                triangleButton.Position = new(18f, -6f);
            }
            else
            {
                triangleButton.Position = new(4f, -4f);
            }

            //set enable state of children
            USpan<TreeNodeOption> nodes = treeNode.Nodes;
            for (uint i = 0; i < nodes.Length; i++)
            {
                rint nodeReference = nodes[i].childNodeReference;
                uint nodeEntity = treeNode.GetReference(nodeReference);
                TreeNode node = new Entity(world, nodeEntity).As<TreeNode>();
                node.SetEnabled(component.expanded);
                node.UpdateChildEnabledStates();
            }

            //ask parent trees to update their positions
            uint parentEntity = world.GetParent(treeNodeEntity);
            while (parentEntity != default)
            {
                if (world.ContainsComponent<IsTree>(parentEntity))
                {
                    Tree tree = new Entity(world, parentEntity).As<Tree>();
                    tree.UpdatePositions();
                    break;
                }
                else if (world.ContainsComponent<IsTreeNode>(parentEntity))
                {
                    parentEntity = world.GetParent(parentEntity);
                }
                else
                {
                    throw new System.Exception();
                }
            }
        }
    }
}