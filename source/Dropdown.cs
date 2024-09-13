using Data;
using InteractionKit.Components;
using InteractionKit.Functions;
using Simulation;
using System;
using System.Diagnostics;
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
                uint labelEntity = box.GetReference(labelReference);
                return new(box.GetWorld(), labelEntity);
            }
        }

        public readonly ref Color LabelColor => ref Label.Color;

        public readonly Box Triangle
        {
            get
            {
                rint triangleReference = box.AsEntity().GetComponent<IsDropdown>().triangleReference;
                uint triangleEntity = box.GetReference(triangleReference);
                return new(box.GetWorld(), triangleEntity);
            }
        }

        public readonly ref Color TriangleColor => ref Triangle.Color;

        public readonly Menu Menu
        {
            get
            {
                rint menuReference = box.AsEntity().GetComponent<IsDropdown>().menuReference;
                uint menuEntity = box.GetReference(menuReference);
                return new Entity(box.GetWorld(), menuEntity).As<Menu>();
            }
        }

        public readonly MenuOptionPath SelectedOption
        {
            get
            {
                MenuOptionPath selectedOption = box.AsEntity().GetComponent<IsDropdown>().selectedOption;
                return selectedOption;
            }
            set
            {
                ref IsDropdown component = ref box.AsEntity().GetComponentRef<IsDropdown>();
                component.selectedOption = value;

                World world = box.GetWorld();
                Menu menu = Menu;
                USpan<MenuOption> options = menu.Options;
                FixedString text = default;
                MenuOptionPath path = value;
                while (path.Length > 0)
                {
                    uint index = path[0];
                    if (path.Length > 1)
                    {
                        if (index < options.length)
                        {
                            MenuOption option = options[index];
                            if (option.childMenuReference != default)
                            {
                                uint childMenuEntity = menu.GetReference(option.childMenuReference);
                                options = world.GetArray<MenuOption>(childMenuEntity);
                                path = path.Slice(1);
                                menu = new Entity(world, childMenuEntity).As<Menu>();
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (index < options.length)
                        {
                            MenuOption option = options[index];
                            text = option.text;
                        }

                        break;
                    }
                }

                rint labelReference = component.labelReference;
                uint labelEntity = box.GetReference(labelReference);
                Label dropdownLabel = new(world, labelEntity);
                dropdownLabel.SetText(text);
            }
        }

        public readonly bool IsExpanded
        {
            get
            {
                ref IsDropdown component = ref box.AsEntity().GetComponentRef<IsDropdown>();
                return component.expanded;
            }
            set
            {
                ref IsDropdown component = ref box.AsEntity().GetComponentRef<IsDropdown>();
                component.expanded = value;

                Menu menu = Menu;
                menu.Size = Size;
                menu.SetEnabled(value);

                USpan<MenuOption> options = menu.Options;
                for (uint i = 0; i < options.length; i++)
                {
                    MenuOption option = options[i];
                    if (option.childMenuReference != default)
                    {
                        uint childMenuEntity = menu.GetReference(option.childMenuReference);
                        Menu childMenu = new Entity(menu.GetWorld(), childMenuEntity).As<Menu>();
                        childMenu.SetEnabled(false);
                    }
                }
            }
        }

        public readonly USpan<MenuOption> Options => Menu.Options;

        public readonly ref DropdownCallbackFunction Callback
        {
            get
            {
                ref IsDropdown component = ref box.AsEntity().GetComponentRef<IsDropdown>();
                return ref component.callback;
            }
        }

        readonly uint IEntity.Value => box.GetEntityValue();
        readonly World IEntity.World => box.GetWorld();
        readonly Definition IEntity.Definition => new Definition().AddComponentTypes<IsTrigger, IsSelectable>();

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

            Menu menu = new(world, new(&ChosenOption));
            menu.Parent = box.AsEntity();
            menu.Anchor = Anchor.BottomLeft;
            menu.Size = box.Size;
            menu.Pivot = new(0f, 1f, 0f);
            menu.SetEnabled(false);

            rint labelReference = box.AddReference(label);
            rint triangleReference = box.AddReference(triangle);
            rint menuReference = box.AddReference(menu);
            box.AsEntity().AddComponent(new IsDropdown(labelReference, triangleReference, menuReference, callback));
        }

        [UnmanagedCallersOnly]
        private static void ChosenOption(Menu menu, uint chosenOption)
        {
            MenuOptionPath path = default;
            path = path.Append(chosenOption);

            World world = menu.GetWorld();
            uint childEntity = menu.GetEntityValue();
            uint parentEntity = menu.Parent.GetEntityValue();
            while (parentEntity != default)
            {
                if (world.ContainsComponent<IsDropdown>(parentEntity))
                {
                    break;
                }
                else if (world.ContainsComponent<IsMenu>(parentEntity))
                {
                    USpan<MenuOption> parentOptions = world.GetArray<MenuOption>(parentEntity);
                    bool found = false;
                    for (uint i = 0; i < parentOptions.length; i++)
                    {
                        rint menuReference = parentOptions[i].childMenuReference;
                        uint menuEntity = world.GetReference(parentEntity, menuReference);
                        if (menuEntity == childEntity)
                        {
                            path = path.Insert(0, i);
                            childEntity = parentEntity;
                            parentEntity = world.GetParent(parentEntity);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    throw new Exception();
                }
            }

            Dropdown dropdown = new Entity(world, parentEntity).As<Dropdown>();
            dropdown.SelectedOption = path;
            dropdown.IsExpanded = false;
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
            Dropdown dropdown = new(world, dropdownEntity);
            dropdown.IsExpanded = !dropdown.IsExpanded;
        }
    }
}