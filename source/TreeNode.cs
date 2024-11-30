using Data;
using InteractionKit.Components;
using InteractionKit.Functions;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms;
using Transforms.Components;
using Unmanaged;
using Worlds;

namespace InteractionKit
{
    public readonly struct TreeNode : ICanvasDescendant
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

        readonly uint IEntity.Value => transform.GetEntityValue();
        readonly World IEntity.World => transform.GetWorld();
        readonly Definition IEntity.Definition => new Definition().AddComponentType<IsTreeNode>().AddArrayType<TreeNodeOption>();

        public TreeNode(World world, uint existingEntity)
        {
            transform = new(world, existingEntity);
        }

        public unsafe TreeNode(World world, FixedString text, Canvas canvas)
        {
            transform = new(world);
            transform.AddComponent(new Anchor());

            Image box = new(world, canvas);
            box.SetParent(transform);
            box.Color = Color.White;
            box.Anchor = new(new(24f, true), new(0f, false), default, new(1f, false), new(1f, false), default);
            box.AddComponent(new IsTrigger(new(&Filter), new(&ToggleSelected)));
            box.AddComponent(new IsSelectable());

            Label label = new(world, canvas, text);
            label.SetParent(box);
            label.Anchor = Anchor.TopLeft;
            label.Color = Color.Black;
            label.Position = new(4f, -4f);
            label.Pivot = new(0f, 1f, 0f);

            rint boxReference = transform.AddReference(box);
            rint labelReference = transform.AddReference(label);

            transform.AddComponent(new IsTreeNode(text, boxReference, labelReference));
            transform.AsEntity().CreateArray<TreeNodeOption>();
        }

        public readonly void Dispose()
        {
            transform.Dispose();
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
            World world = transform.GetWorld();
            Vector2 size = Size;
            Canvas canvas = this.GetCanvas();
            Settings settings = world.GetFirst<Settings>();
            uint nodeCount = transform.AsEntity().GetArrayLength<TreeNodeOption>();
            if (nodeCount == 0)
            {
                //the button that toggles expanded state
                float triangleButtonSize = 16f;
                Button triangle = new(world, new(&ToggleExpanded), canvas);
                triangle.SetParent(transform);
                triangle.Anchor = Anchor.TopLeft;
                triangle.Size = new(triangleButtonSize, triangleButtonSize);
                triangle.Color = Color.Black;
                triangle.Position = new(4f, -4f);

                Image triangleImage = triangle;
                triangleImage.Material = settings.GetTriangleMaterial(canvas.Camera);

                Transform triangleTransform = triangle;
                triangleTransform.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI * -0.5f);
            }

            TreeNode node = new(world, text, canvas);
            node.SetParent(transform);
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
        private static void Filter(TriggerFilter.Input input)
        {
            foreach (ref Entity entity in input.Entities)
            {
                IsSelectable component = entity.GetComponent<IsSelectable>();
                if (!component.WasPrimaryInteractedWith || !component.IsSelected)
                {
                    entity = default;
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void ToggleSelected(Entity boxEntity)
        {
            World world = boxEntity.GetWorld();
            Entity nodeEntity = boxEntity.Parent;
            uint parentEntity = nodeEntity.Parent.GetEntityValue();
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
            Settings settings = world.GetFirst<Settings>();
            bool selectMultiple = settings.PressedCharacters.Contains(Settings.ShiftCharacter);
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

            TreeNode node = nodeEntity.As<TreeNode>();
            bool isSelected = tree.IsSelected(node);
            tree.SetSelected(node, !isSelected);
        }

        [UnmanagedCallersOnly]
        private static void ToggleExpanded(Entity expandButtonEntity)
        {
            Entity treeNodeEntity = expandButtonEntity.Parent;
            ref IsTreeNode component = ref treeNodeEntity.GetComponentRef<IsTreeNode>();
            component.expanded = !component.expanded;
            TreeNode treeNode = treeNodeEntity.As<TreeNode>();

            ref Rotation rotation = ref expandButtonEntity.GetComponentRef<Rotation>();
            rotation.value = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, component.expanded ? MathF.PI : MathF.PI * -0.5f);

            Button triangleButton = expandButtonEntity.As<Button>();
            if (component.expanded)
            {
                triangleButton.Position = new(18f, -6f);
            }
            else
            {
                triangleButton.Position = new(4f, -4f);
            }

            //set enable state of children
            World world = expandButtonEntity.GetWorld();
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
            uint parentEntity = treeNodeEntity.Parent.GetEntityValue();
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

        public static implicit operator Entity(TreeNode node)
        {
            return node.transform;
        }

        public static implicit operator Transform(TreeNode node)
        {
            return node.transform;
        }
    }
}