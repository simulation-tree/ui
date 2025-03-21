using Cameras;
using Materials;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms.Components;
using UI.Components;
using UI.Functions;
using Unmanaged;
using Worlds;

namespace UI
{
    public readonly partial struct Dropdown : ISelectable
    {
        public readonly ref Vector2 Position => ref As<UITransform>().Position;
        public readonly ref float Z => ref As<UITransform>().Z;
        public readonly ref Vector2 Size => ref As<UITransform>().Size;

        public readonly float Rotation
        {
            get => As<UITransform>().Rotation;
            set => As<UITransform>().Rotation = value;
        }

        public readonly ref Anchor Anchor => ref As<UITransform>().Anchor;
        public readonly ref Vector3 Pivot => ref As<UITransform>().Pivot;
        public readonly ref Vector4 BackgroundColor => ref As<Image>().Color;

        public readonly Label Label
        {
            get
            {
                rint labelReference = GetComponent<IsDropdown>().labelReference;
                uint labelEntity = GetReference(labelReference);
                return new Entity(world, labelEntity).As<Label>();
            }
        }

        public readonly ref Vector4 LabelColor => ref Label.Color;

        public readonly Image Triangle
        {
            get
            {
                rint triangleReference = GetComponent<IsDropdown>().triangleReference;
                uint triangleEntity = GetReference(triangleReference);
                return new Entity(world, triangleEntity).As<Image>();
            }
        }

        public readonly ref Vector4 TriangleColor => ref Triangle.Color;

        public readonly Menu Menu
        {
            get
            {
                rint menuReference = GetComponent<IsDropdown>().menuReference;
                uint menuEntity = GetReference(menuReference);
                return new Entity(world, menuEntity).As<Menu>();
            }
        }

        public readonly OptionPath SelectedOption
        {
            get => GetComponent<IsDropdown>().selectedOption;
            set
            {
                ref IsDropdown component = ref GetComponent<IsDropdown>();
                component.selectedOption = value;

                Menu menu = Menu;
                Span<IsMenuOption> options = menu.Options;
                ASCIIText256 text = default;
                OptionPath path = value;
                while (path.Depth > 0)
                {
                    uint index = path[0];
                    if (path.Depth > 1)
                    {
                        if (index < options.Length)
                        {
                            IsMenuOption option = options[(int)index];
                            if (option.childMenuReference != default)
                            {
                                uint childMenuEntity = menu.GetReference(option.childMenuReference);
                                options = world.GetArray<IsMenuOption>(childMenuEntity);
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
                        if (index < options.Length)
                        {
                            IsMenuOption option = options[(int)index];
                            text = option.text;
                        }

                        break;
                    }
                }

                rint labelReference = component.labelReference;
                uint labelEntity = GetReference(labelReference);
                Label dropdownLabel = new Entity(world, labelEntity).As<Label>();
                dropdownLabel.SetText(text);
            }
        }

        public readonly bool IsExpanded
        {
            get => GetComponent<IsDropdown>().expanded;
            set
            {
                ref IsDropdown component = ref GetComponent<IsDropdown>();
                component.expanded = value;

                Menu menu = Menu;
                menu.OptionSize = Size;
                menu.IsEnabled = component.expanded;

                Span<IsMenuOption> options = menu.Options;
                for (int i = 0; i < options.Length; i++)
                {
                    ref IsMenuOption option = ref options[i];
                    if (option.childMenuReference != default)
                    {
                        if (!component.expanded)
                        {
                            option.expanded = false;
                        }

                        uint childMenuEntity = menu.GetReference(option.childMenuReference);
                        Menu childMenu = new Entity(world, childMenuEntity).As<Menu>();
                        childMenu.IsEnabled = option.expanded;
                    }
                }
            }
        }

        public readonly Span<IsMenuOption> Options => Menu.Options;
        public readonly ref DropdownCallback Callback => ref GetComponent<IsDropdown>().callback;

        public unsafe Dropdown(Canvas canvas, Vector2 optionSize, DropdownCallback callback = default)
        {
            Image background = new(canvas);
            background.AddComponent(new IsTrigger(new(&Filter), new(&ToggleDropdown)));
            background.AddComponent(new IsSelectable(canvas.SelectionMask));
            world = canvas.world;
            value = background.value;

            Label label = new(canvas, default(ASCIIText256));
            label.SetParent(background);
            label.Anchor = Anchor.TopLeft;
            label.Color = new(0, 0, 0, 1);
            label.Position = new(4f, -4f);
            label.Pivot = new(0f, 1f, 0f);

            Image triangle = new(canvas);
            triangle.SetParent(background);
            triangle.Material = GetTriangleMaterialFromSettings(canvas.Camera);
            triangle.Anchor = Anchor.TopRight;
            triangle.Size = new(16f, 16f);
            triangle.Color = new(0, 0, 0, 1);
            triangle.Pivot = new(0.5f, 0.5f, 0f);

            UITransform triangleTransform = triangle;
            triangleTransform.Rotation = MathF.Tau * 0.5f;

            Menu menu = new(canvas, optionSize, new(&ChosenOption));
            menu.Anchor = Anchor.BottomLeft;
            menu.Pivot = new(0f, 1f, 0f);
            menu.IsEnabled = false;

            rint dropdownReference = menu.AddReference(this);
            menu.AddComponent(new DropdownMenu(dropdownReference));

            rint labelReference = background.AddReference(label);
            rint triangleReference = background.AddReference(triangle);
            rint menuReference = background.AddReference(menu);
            background.AddComponent(new IsDropdown(labelReference, triangleReference, menuReference, callback));
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsTrigger>();
            archetype.AddComponentType<IsSelectable>();
            archetype.AddComponentType<IsDropdown>();
        }

        [UnmanagedCallersOnly]
        public static void Filter(TriggerFilter.Input input)
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
        public static void ToggleDropdown(Entity dropdownEntity)
        {
            Dropdown dropdown = dropdownEntity.As<Dropdown>();
            dropdown.IsExpanded = !dropdown.IsExpanded;
        }

        [UnmanagedCallersOnly]
        public static void ChosenOption(MenuOption option)
        {
            World world = option.rootMenu.world;
            rint dropdownReference = option.rootMenu.GetComponent<DropdownMenu>().dropdownReference;
            uint dropdownEntity = option.rootMenu.GetReference(dropdownReference);
            Dropdown dropdown = new Entity(world, dropdownEntity).As<Dropdown>();
            dropdown.SelectedOption = option.optionPath;
            dropdown.IsExpanded = false;
        }

        public static Material GetTriangleMaterialFromSettings(Camera camera)
        {
            Settings settings = camera.world.GetFirst<Settings>();
            return settings.GetTriangleMaterial(camera);
        }
    }

    public readonly struct Dropdown<T> : ISelectable where T : struct, Enum
    {
        private readonly Dropdown dropdown;

        public unsafe Dropdown(Canvas canvas, Vector2 optionSize, DropdownCallback callback = default)
        {
            Image background = new(canvas);
            background.AddComponent(new IsTrigger(new(&Dropdown.Filter), new(&Dropdown.ToggleDropdown)));
            background.AddComponent(new IsSelectable(canvas.SelectionMask));

            Label label = new(canvas, default(ASCIIText256));
            label.SetParent(background);
            label.Anchor = Anchor.TopLeft;
            label.Color = new(0, 0, 0, 1);
            label.Position = new(4f, -4f);
            label.Pivot = new(0f, 1f, 0f);

            Image triangle = new(canvas);
            triangle.SetParent(background);
            triangle.Material = Dropdown.GetTriangleMaterialFromSettings(canvas.Camera);
            triangle.Anchor = Anchor.TopRight;
            triangle.Size = new(16f, 16f);
            triangle.Color = new(0, 0, 0, 1);
            triangle.Pivot = new(0.5f, 0.5f, 0f);

            UITransform triangleTransform = triangle;
            triangleTransform.Rotation = MathF.Tau * 0.5f;

            Menu menu = new(canvas, optionSize, new(&Dropdown.ChosenOption));
            menu.Anchor = Anchor.BottomLeft;
            menu.Pivot = new(0f, 1f, 0f);
            menu.IsEnabled = false;

            T[] options = Enum.GetValues<T>();
            for (uint i = 0; i < options.Length; i++)
            {
                T option = options[i];
                string optionText = option.ToString();
                menu.AddOption(optionText);
            }

            rint dropdownReference = menu.AddReference(background);
            menu.AddComponent(new DropdownMenu(dropdownReference));

            rint labelReference = background.AddReference(label);
            rint triangleReference = background.AddReference(triangle);
            rint menuReference = background.AddReference(menu);
            background.AddComponent(new IsDropdown(labelReference, triangleReference, menuReference, callback));

            dropdown = background.As<Dropdown>();
            dropdown.SelectedOption = "0";
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.Add<Dropdown>();
        }

        public readonly void Dispose()
        {
            dropdown.Dispose();
        }

        public static implicit operator Dropdown(Dropdown<T> dropdown)
        {
            return dropdown.dropdown;
        }

        public static implicit operator UITransform(Dropdown<T> dropdown)
        {
            return dropdown.AsEntity().As<UITransform>();
        }

        public static implicit operator Entity(Dropdown<T> dropdown)
        {
            return dropdown.AsEntity();
        }
    }
}