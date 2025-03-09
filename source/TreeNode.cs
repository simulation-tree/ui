using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms;
using Transforms.Components;
using UI.Components;
using UI.Functions;
using Unmanaged;
using Worlds;

namespace UI
{
    public readonly partial struct TreeNode : IEntity
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
        public readonly System.Span<TreeNodeOption> Nodes => GetArray<TreeNodeOption>().AsSpan();

        public readonly Label Label
        {
            get
            {
                uint labelEntity = GetReference(GetComponent<IsTreeNode>().labelReference);
                return new Entity(world, labelEntity).As<Label>();
            }
        }

        public readonly ref Vector4 BackgroundColor
        {
            get
            {
                rint boxReference = GetComponent<IsTreeNode>().boxReference;
                uint boxEntity = GetReference(boxReference);
                return ref new Entity(world, boxEntity).As<Image>().Color;
            }
        }

        public readonly bool IsExpanded => GetComponent<IsTreeNode>().expanded;

        public unsafe TreeNode(ASCIIText256 text, Canvas canvas)
        {
            world = canvas.world;
            Transform transform = new(world);
            value = transform.value;

            transform.AddComponent(new Anchor());

            Image box = new(canvas);
            box.SetParent(transform);
            box.Color = new(1, 1, 1, 1);
            box.Anchor = new(new(24f, true), new(0f, false), default, new(1f, false), new(1f, false), default);
            box.AddComponent(new IsTrigger(new(&Filter), new(&ToggleSelected)));
            box.AddComponent(new IsSelectable(canvas.SelectionMask));

            Label label = new(canvas, text);
            label.SetParent(box);
            label.Anchor = Anchor.TopLeft;
            label.Color = new(0, 0, 0, 1);
            label.Position = new(4f, -4f);
            label.Pivot = new(0f, 1f, 0f);

            rint boxReference = transform.AddReference(box);
            rint labelReference = transform.AddReference(label);

            transform.AddComponent(new IsTreeNode(text, boxReference, labelReference));
            transform.CreateArray<TreeNodeOption>();
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsTreeNode>();
            archetype.AddArrayType<TreeNodeOption>();
        }

        public override string ToString()
        {
            Span<char> buffer = stackalloc char[256];
            int length = ToString(buffer);
            return buffer.Slice(0, length).ToString();
        }

        public readonly int ToString(Span<char> destination)
        {
            Label text = Label;
            ReadOnlySpan<char> processedText = text.ProcessedText;
            processedText.CopyTo(destination);
            return processedText.Length;
        }

        public unsafe readonly TreeNode AddLeaf(ASCIIText256 text)
        {
            Vector2 size = Size;
            Canvas canvas = this.GetCanvas();
            Settings settings = canvas.Settings;
            Values<TreeNodeOption> options = GetArray<TreeNodeOption>();
            int nodeCount = options.Length;
            if (nodeCount == 0)
            {
                //the button that toggles expanded state
                float triangleButtonSize = 16f;
                Button triangle = new(new(&ToggleExpanded), canvas);
                triangle.SetParent(value);
                triangle.Anchor = Anchor.TopLeft;
                triangle.Size = new(triangleButtonSize, triangleButtonSize);
                triangle.Color = new(0, 0, 0, 1);
                triangle.Position = new(4f, -4f);

                Image triangleImage = triangle;
                triangleImage.Material = settings.GetTriangleMaterial(canvas.Camera);

                UITransform triangleTransform = triangle;
                triangleTransform.Rotation = MathF.Tau * -0.25f;
            }

            TreeNode node = new(text, canvas);
            node.SetParent(value);
            node.Position = new(30, -(nodeCount + 1) * size.Y);
            node.Size = size;
            node.IsEnabled = false;

            rint childNodeReference = AddReference(node);
            options.Length++;
            options[nodeCount] = new(childNodeReference);
            return node;
        }

        public readonly float UpdatePositions()
        {
            Vector2 size = Size;
            Span<TreeNodeOption> nodes = Nodes;
            float y = size.Y;
            for (int i = 0; i < nodes.Length; i++)
            {
                rint nodeReference = nodes[i].childNodeReference;
                uint nodeEntity = GetReference(nodeReference);
                TreeNode node = new Entity(world, nodeEntity).As<TreeNode>();
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
            Span<TreeNodeOption> nodes = Nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                rint nodeReference = nodes[i].childNodeReference;
                uint nodeEntity = GetReference(nodeReference);
                TreeNode node = new Entity(world, nodeEntity).As<TreeNode>();
                node.IsEnabled = expanded;
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
            World world = boxEntity.world;
            Entity nodeEntity = boxEntity.Parent;
            Entity current = nodeEntity.Parent;
            while (current != default)
            {
                if (current.ContainsTag<IsTree>())
                {
                    break;
                }
                else if (current.ContainsComponent<IsTreeNode>())
                {
                    current = current.Parent;
                }
                else
                {
                    throw new System.Exception();
                }
            }

            Tree tree = current.As<Tree>();
            Settings settings = world.GetFirst<Settings>();
            bool selectMultiple = settings.PressedCharacters.Contains(Settings.ShiftCharacter);
            if (!selectMultiple)
            {
                //deselect all
                Values<SelectedLeaf> currentSelection = tree.GetArray<SelectedLeaf>();
                for (int i = 0; i < currentSelection.Length; i++)
                {
                    rint nodeReference = currentSelection[i].nodeReference;
                    uint selectedNodeEntity = tree.GetReference(nodeReference);
                    TreeNode selectedNode = new Entity(world, selectedNodeEntity).As<TreeNode>();
                    selectedNode.BackgroundColor = new(1, 1, 1, 1);
                    selectedNode.Label.Color = new(0, 0, 0, 1);
                }

                currentSelection.Length = 0;
            }

            TreeNode node = nodeEntity.As<TreeNode>();
            bool isSelected = tree.IsSelected(node);
            tree.SetSelected(node, !isSelected);
        }

        [UnmanagedCallersOnly]
        private static void ToggleExpanded(Entity expandButtonEntity)
        {
            Entity treeNodeEntity = expandButtonEntity.Parent;
            ref IsTreeNode component = ref treeNodeEntity.GetComponent<IsTreeNode>();
            component.expanded = !component.expanded;
            TreeNode treeNode = treeNodeEntity.As<TreeNode>();

            ref Rotation rotation = ref expandButtonEntity.GetComponent<Rotation>();
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
            World world = expandButtonEntity.world;
            Span<TreeNodeOption> nodes = treeNode.Nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                rint nodeReference = nodes[i].childNodeReference;
                uint nodeEntity = treeNode.GetReference(nodeReference);
                TreeNode node = new Entity(world, nodeEntity).As<TreeNode>();
                node.IsEnabled = component.expanded;
                node.UpdateChildEnabledStates();
            }

            //ask parent trees to update their positions
            Entity current = treeNodeEntity.Parent;
            while (current != default)
            {
                if (current.ContainsTag<IsTree>())
                {
                    Tree tree = current.As<Tree>();
                    tree.UpdatePositions();
                    break;
                }
                else if (current.ContainsComponent<IsTreeNode>())
                {
                    current = current.Parent;
                }
                else
                {
                    throw new System.Exception();
                }
            }
        }

        public static implicit operator Transform(TreeNode node)
        {
            return node.As<Transform>();
        }
    }
}