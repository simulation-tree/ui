using Data;
using InteractionKit.Components;
using InteractionKit.Functions;
using Simulation;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct TreeNode : IEntity
    {
        public readonly Box box;

        public readonly Entity Parent
        {
            get => box.Parent;
            set => box.Parent = value;
        }

        public readonly Vector2 Position
        {
            get => box.Position;
            set => box.Position = value;
        }

        public readonly Vector2 Size
        {
            get => box.Size;
            set => box.Size = value;
        }

        public readonly ref Anchor Anchor => ref box.Anchor;
        public readonly ref Vector3 Pivot => ref box.Pivot;
        public readonly USpan<TreeNodeOption> Nodes => box.AsEntity().GetArray<TreeNodeOption>();

        public readonly Label Label
        {
            get
            {
                uint labelEntity = box.GetReference(box.AsEntity().GetComponent<IsTreeNode>().labelReference);
                return new(box.GetWorld(), labelEntity);
            }
        }

        public readonly InteractiveContext Context => box.AsEntity().GetComponent<IsTreeNode>().context;

        readonly uint IEntity.Value => box.GetEntityValue();
        readonly World IEntity.World => box.GetWorld();
        readonly Definition IEntity.Definition => new Definition().AddComponentType<IsTreeNode>().AddArrayType<TreeNodeOption>();

        public unsafe TreeNode(World world, FixedString text, InteractiveContext context)
        {
            box = new(world, context);
            box.AsEntity().AddComponent(new IsTrigger(new(&Filter), new(&ToggleSelected)));
            box.AsEntity().AddComponent(new IsSelectable());

            FixedString indentedText = text;
            indentedText.Insert(0, "    ");
            Label label = new(world, context, indentedText);
            label.Parent = box.AsEntity();
            label.Anchor = Anchor.TopLeft;
            label.Color = Color.Black;
            label.Position = new(4f, -4f);
            label.Pivot = new(0f, 1f, 0f);

            rint labelReference = box.AddReference(label);

            box.AsEntity().AddComponent(new IsTreeNode(text, labelReference, context));
            box.AsEntity().CreateArray<TreeNodeOption>();
        }

        public unsafe readonly TreeNode AddLeaf(FixedString text)
        {
            InteractiveContext context = Context;
            World world = box.GetWorld();
            uint nodeCount = box.AsEntity().GetArrayLength<TreeNodeOption>();
            if (nodeCount == 0)
            {
                Button triangle = new(world, new(&ToggleExpanded), context);
                triangle.Parent = box.AsEntity();
                triangle.box.Material = context.TriangleMaterial;
                triangle.Anchor = Anchor.TopLeft;
                triangle.Size = new(16f, 16f);
                triangle.Color = Color.Black;
                triangle.Position = new(4f, -4f);
                triangle.box.transform.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI * -0.5f);
            }

            Vector2 size = Size;
            TreeNode node = new(world, text, context);
            node.Parent = box;
            node.Position = new(30, -(nodeCount + 1) * size.Y);
            node.Size = size;
            rint nodeReference = box.AddReference(node);
            USpan<TreeNodeOption> nodes = box.AsEntity().ResizeArray<TreeNodeOption>(nodeCount + 1);
            nodes[nodeCount] = new(nodeReference);
            return node;
        }

        [UnmanagedCallersOnly]
        private static void Filter(FilterFunction.Input input)
        {
            World world = input.world;
            foreach (ref uint entity in input.Entities)
            {
                IsSelectable component = world.GetComponent<IsSelectable>(entity);
                bool pressed = (component.state & IsSelectable.State.WasPrimaryInteractedWith) != 0;
                if (!pressed)
                {
                    entity = default;
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void ToggleSelected(World world, uint nodeEntity)
        {
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
                USpan<SelectedLeaf> currentSelection = tree.Selected;
                for (uint i = 0; i < currentSelection.length; i++)
                {
                    rint nodeReference = currentSelection[i].nodeReference;
                    uint selectedNodeEntity = tree.GetReference(nodeReference);
                    TreeNode selectedNode = new Entity(world, selectedNodeEntity).As<TreeNode>();
                    selectedNode.box.Color = Color.White;
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
        }
    }
}