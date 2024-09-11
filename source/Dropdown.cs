using Data;
using InteractionKit.Components;
using InteractionKit.Functions;
using Rendering.Components;
using Simulation;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct Dropdown : ISelectable
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
        public readonly ref Color BackgroundColor => ref box.Color;

        public readonly Label Label
        {
            get
            {
                rint labelReference = box.AsEntity().GetComponent<IsDropdown>().labelReference;
                uint labelEntity = box.AsEntity().GetReference(labelReference);
                return new(box.GetWorld(), labelEntity);
            }
        }

        public readonly ref Color LabelColor => ref Label.Color;

        public readonly Box Triangle
        {
            get
            {
                rint triangleReference = box.AsEntity().GetComponent<IsDropdown>().triangleReference;
                uint triangleEntity = box.AsEntity().GetReference(triangleReference);
                return new(box.GetWorld(), triangleEntity);
            }
        }

        public readonly ref Color TriangleColor => ref Triangle.Color;

        public readonly uint SelectedOption
        {
            get
            {
                uint selectedOption = box.AsEntity().GetComponent<IsDropdown>().selectedOption;
                return selectedOption;
            }
            set
            {
                ref IsDropdown dropdown = ref box.AsEntity().GetComponentRef<IsDropdown>();
                dropdown.selectedOption = value;

                USpan<DropdownOption> options = box.AsEntity().GetArray<DropdownOption>();
                if (value < options.length)
                {
                    DropdownOption option = options[value];
                    rint labelReference = dropdown.labelReference;
                    uint labelEntity = box.AsEntity().GetReference(labelReference);
                    Label dropdownLabel = new(box.GetWorld(), labelEntity);
                    dropdownLabel.SetText(option.text);
                }
            }
        }

        public readonly USpan<DropdownOption> Options => box.AsEntity().GetArray<DropdownOption>();

        public readonly ref DropdownCallbackFunction Callback
        {
            get
            {
                ref IsDropdown dropdown = ref box.AsEntity().GetComponentRef<IsDropdown>();
                return ref dropdown.callback;
            }
        }

        readonly uint IEntity.Value => box.GetEntityValue();
        readonly World IEntity.World => box.GetWorld();
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsTrigger>(), RuntimeType.Get<IsSelectable>()], [RuntimeType.Get<DropdownOption>()]);

#if NET
        [Obsolete("Default constructor not available", true)]
        public Dropdown()
        {
            throw new NotSupportedException();
        }
#endif

        public Dropdown(World world, uint existingEntity)
        {
            box = new(world, existingEntity);
        }

        public unsafe Dropdown(World world, InteractiveContext context, DropdownCallbackFunction callback = default)
        {
            box = new(world, context);
            box.AsEntity().AddComponent(new IsTrigger(new(&Filter), new(&ToggleDropdown)));
            box.AsEntity().AddComponent(new IsSelectable());
            box.AsEntity().CreateArray<DropdownOption>();

            Label label = new(world, context, "");
            label.Parent = box.AsEntity();
            label.Anchor = Anchor.TopLeft;
            label.Color = Color.Black;
            label.Position = new(4f, -4f);
            label.Pivot = new(0f, 1f, 0f);

            Box triangle = new(world, context);
            triangle.Parent = box.AsEntity();
            triangle.Material = context.triangleMaterial;
            triangle.Anchor = Anchor.TopRight;
            triangle.Size = new(16f, 16f);
            triangle.Color = Color.Black;
            triangle.Pivot = new(0.5f, 0.5f, 0f);
            triangle.transform.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI * 1f);

            rint labelReference = box.AsEntity().AddReference(label);
            rint triangleReference = box.AsEntity().AddReference(triangle);
            box.AsEntity().AddComponent(new IsDropdown(labelReference, triangleReference, callback));
        }

        public unsafe readonly uint AddOption(FixedString label, InteractiveContext context)
        {
            uint optionCount = box.AsEntity().GetArrayLength<DropdownOption>();
            Vector3 dropdownSize = box.AsEntity().GetComponent<Scale>().value;

            Button optionButton = new(box.GetWorld(), new(&ChoseOption), context);
            optionButton.Parent = box.AsEntity();
            optionButton.Position = new(0, -dropdownSize.Y * (optionCount + 1));
            optionButton.Size = new(dropdownSize.X, dropdownSize.Y);
            optionButton.Anchor = Anchor.TopLeft;
            optionButton.Pivot = new(0f, 1f, 0f);

            Label optionButtonLabel = new(box.GetWorld(), context, label);
            optionButtonLabel.Parent = optionButton.AsEntity();
            optionButtonLabel.Anchor = Anchor.TopLeft;
            optionButtonLabel.Color = Color.Black;
            optionButtonLabel.Position = new(4f, -4f);
            optionButtonLabel.Pivot = new(0f, 1f, 0f);

            optionButton.SetEnabled(false);
            optionButtonLabel.SetEnabled(false);

            USpan<DropdownOption> options = box.AsEntity().ResizeArray<DropdownOption>(optionCount + 1);
            rint buttonReference = box.AsEntity().AddReference(optionButton);
            rint buttonLabelReference = box.AsEntity().AddReference(optionButtonLabel);
            options[optionCount] = new(label, buttonReference, buttonLabelReference);

            //update the dropdowns label
            uint selectedOption = box.AsEntity().GetComponent<IsDropdown>().selectedOption;
            if (optionCount == selectedOption)
            {
                rint labelReference = box.AsEntity().GetComponent<IsDropdown>().labelReference;
                uint labelEntity = box.AsEntity().GetReference(labelReference);
                Label dropdownLabel = new(box.GetWorld(), labelEntity);
                dropdownLabel.SetText(label);
            }

            return optionCount;
        }

        [UnmanagedCallersOnly]
        private unsafe static void ChoseOption(World world, uint optionButtonEntity)
        {
            uint dropdownEntity = world.GetParent(optionButtonEntity);
            USpan<uint> dropdownChildren = world.GetChildren(dropdownEntity);
            uint selectedOption = 0;
            for (uint i = 0; i < dropdownChildren.length; i++)
            {
                uint childEntity = dropdownChildren[i];
                if (childEntity == optionButtonEntity)
                {
                    break;
                }

                if (world.ContainsComponent<IsTrigger>(childEntity))
                {
                    selectedOption++;
                }
            }

            USpan<uint> buttonChildren = world.GetChildren(optionButtonEntity);
            for (uint i = 0; i < buttonChildren.length; i++)
            {
                uint childEntity = buttonChildren[i];
                if (world.TryGetComponent(childEntity, out IsTextRenderer textRenderer))
                {
                    ref IsDropdown dropdown = ref world.GetComponentRef<IsDropdown>(dropdownEntity);
                    rint textMeshReference = textRenderer.textMeshReference;
                    uint textMeshEntity = world.GetReference(childEntity, textMeshReference);
                    USpan<char> text = world.GetArray<char>(textMeshEntity);

                    if (dropdown.callback != default)
                    {
                        dropdown.callback.Invoke(new Dropdown(world, dropdownEntity), dropdown.selectedOption, selectedOption);
                    }

                    dropdown.selectedOption = selectedOption;
                    rint dropdownLabelReference = dropdown.labelReference;
                    uint dropdownLabelEntity = world.GetReference(dropdownEntity, dropdownLabelReference);
                    Label dropdownLabel = new(world, dropdownLabelEntity);
                    dropdownLabel.SetText(text);

                    delegate* unmanaged<World, uint, void> functionPointer = &ToggleDropdown;
                    functionPointer(world, dropdownEntity);
                    break;
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
                bool pressed = (component.state & IsSelectable.State.WasPrimaryInteractedWith) != 0;
                if (!pressed)
                {
                    entity = default;
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void ToggleDropdown(World world, uint dropdownEntity)
        {
            ref IsDropdown dropdown = ref world.GetComponentRef<IsDropdown>(dropdownEntity);
            dropdown.expanded = !dropdown.expanded;
            USpan<DropdownOption> options = world.GetArray<DropdownOption>(dropdownEntity);
            for (uint i = 0; i < options.length; i++)
            {
                DropdownOption option = options[i];
                rint buttonReference = option.buttonReference;
                rint buttonLabelReference = option.buttonLabelReference;
                uint buttonEntity = world.GetReference(dropdownEntity, buttonReference);
                uint buttonLabelEntity = world.GetReference(dropdownEntity, buttonLabelReference);
                world.SetEnabled(buttonEntity, dropdown.expanded);
                world.SetEnabled(buttonLabelEntity, dropdown.expanded);
            }
        }
    }
}