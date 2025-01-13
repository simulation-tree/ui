using Cameras;
using InteractionKit.Components;
using InteractionKit.Functions;
using Rendering;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms;
using Transforms.Components;
using Unmanaged;
using Worlds;

namespace InteractionKit
{
    public readonly struct Dropdown : ISelectable
    {
        public readonly Image background;

        public readonly ref Vector2 Position => ref background.Position;

        /// <summary>
        /// Size of the dropdown button.
        /// </summary>
        public readonly ref Vector2 Size => ref background.Size;

        public readonly ref float Z => ref background.Z;
        public readonly ref Anchor Anchor => ref background.Anchor;
        public readonly ref Vector3 Pivot => ref background.Pivot;
        public readonly ref Vector4 BackgroundColor => ref background.Color;

        public readonly Label Label
        {
            get
            {
                rint labelReference = background.AsEntity().GetComponent<IsDropdown>().labelReference;
                uint labelEntity = background.GetReference(labelReference);
                return new(background.GetWorld(), labelEntity);
            }
        }

        public readonly ref Vector4 LabelColor => ref Label.Color;

        public readonly Image Triangle
        {
            get
            {
                rint triangleReference = background.AsEntity().GetComponent<IsDropdown>().triangleReference;
                uint triangleEntity = background.GetReference(triangleReference);
                return new(background.GetWorld(), triangleEntity);
            }
        }

        public readonly ref Vector4 TriangleColor => ref Triangle.Color;

        public readonly Menu Menu
        {
            get
            {
                rint menuReference = background.AsEntity().GetComponent<IsDropdown>().menuReference;
                uint menuEntity = background.GetReference(menuReference);
                return new Entity(background.GetWorld(), menuEntity).As<Menu>();
            }
        }

        public readonly OptionPath SelectedOption
        {
            get
            {
                OptionPath selectedOption = background.AsEntity().GetComponent<IsDropdown>().selectedOption;
                return selectedOption;
            }
            set
            {
                ref IsDropdown component = ref background.AsEntity().GetComponent<IsDropdown>();
                component.selectedOption = value;

                World world = background.GetWorld();
                Menu menu = Menu;
                USpan<IsMenuOption> options = menu.Options;
                FixedString text = default;
                OptionPath path = value;
                while (path.Depth > 0)
                {
                    uint index = path[0];
                    if (path.Depth > 1)
                    {
                        if (index < options.Length)
                        {
                            IsMenuOption option = options[index];
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
                            IsMenuOption option = options[index];
                            text = option.text;
                        }

                        break;
                    }
                }

                rint labelReference = component.labelReference;
                uint labelEntity = background.GetReference(labelReference);
                Label dropdownLabel = new(world, labelEntity);
                dropdownLabel.SetText(text);
            }
        }

        public readonly bool IsExpanded
        {
            get
            {
                ref IsDropdown component = ref background.AsEntity().GetComponent<IsDropdown>();
                return component.expanded;
            }
            set
            {
                ref IsDropdown component = ref background.AsEntity().GetComponent<IsDropdown>();
                component.expanded = value;

                Menu menu = Menu;
                menu.OptionSize = Size;
                menu.SetEnabled(component.expanded);

                USpan<IsMenuOption> options = menu.Options;
                for (uint i = 0; i < options.Length; i++)
                {
                    ref IsMenuOption option = ref options[i];
                    if (option.childMenuReference != default)
                    {
                        if (!component.expanded)
                        {
                            option.expanded = false;
                        }

                        uint childMenuEntity = menu.GetReference(option.childMenuReference);
                        Menu childMenu = new Entity(menu.GetWorld(), childMenuEntity).As<Menu>();
                        childMenu.SetEnabled(option.expanded);
                    }
                }
            }
        }

        public readonly USpan<IsMenuOption> Options => Menu.Options;

        public readonly ref DropdownCallback Callback
        {
            get
            {
                ref IsDropdown component = ref background.AsEntity().GetComponent<IsDropdown>();
                return ref component.callback;
            }
        }

        readonly uint IEntity.Value => background.GetEntityValue();
        readonly World IEntity.World => background.GetWorld();

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsTrigger>();
            archetype.AddComponentType<IsSelectable>();
            archetype.AddComponentType<IsDropdown>();
        }

#if NET
        [Obsolete("Default constructor not available", true)]
        public Dropdown()
        {
            throw new NotSupportedException();
        }
#endif

        public Dropdown(World world, uint existingEntity)
        {
            background = new(world, existingEntity);
        }

        public unsafe Dropdown(Canvas canvas, Vector2 optionSize, DropdownCallback callback = default)
        {
            background = new(canvas);
            background.AddComponent(new IsTrigger(new(&Filter), new(&ToggleDropdown)));
            background.AddComponent(new IsSelectable(canvas.SelectionMask));

            Label label = new(canvas, default(FixedString));
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

            Transform triangleTransform = triangle;
            triangleTransform.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI * 1f);

            Menu menu = new(canvas, optionSize, new(&ChosenOption));
            menu.Anchor = Anchor.BottomLeft;
            menu.Pivot = new(0f, 1f, 0f);
            menu.SetEnabled(false);

            rint dropdownReference = menu.AddReference(this);
            menu.AddComponent(new DropdownMenu(dropdownReference));

            rint labelReference = background.AddReference(label);
            rint triangleReference = background.AddReference(triangle);
            rint menuReference = background.AddReference(menu);
            background.AddComponent(new IsDropdown(labelReference, triangleReference, menuReference, callback));
        }

        public readonly void Dispose()
        {
            background.Dispose();
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
            World world = option.rootMenu.GetWorld();
            rint dropdownReference = option.rootMenu.AsEntity().GetComponent<DropdownMenu>().dropdownReference;
            uint dropdownEntity = option.rootMenu.GetReference(dropdownReference);
            Dropdown dropdown = new Entity(world, dropdownEntity).As<Dropdown>();
            dropdown.SelectedOption = option.optionPath;
            dropdown.IsExpanded = false;
        }

        public static Material GetTriangleMaterialFromSettings(Camera camera)
        {
            Settings settings = camera.GetWorld().GetFirst<Settings>();
            return settings.GetTriangleMaterial(camera);
        }

        public static implicit operator Entity(Dropdown dropdown)
        {
            return dropdown.background;
        }
    }

    public readonly struct Dropdown<T> : ISelectable where T : struct, Enum
    {
        private readonly Dropdown dropdown;

        readonly uint IEntity.Value => dropdown.GetEntityValue();
        readonly World IEntity.World => dropdown.GetWorld();

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.Add<Dropdown>();
        }

#if NET
        [Obsolete("Default constructor not available", true)]
        public Dropdown()
        {
            throw new NotSupportedException();
        }
#endif

        public Dropdown(World world, uint existingEntity)
        {
            dropdown = new(world, existingEntity);
        }

        public unsafe Dropdown(Canvas canvas, Vector2 optionSize, DropdownCallback callback = default)
        {
            Image background = new(canvas);
            background.AddComponent(new IsTrigger(new(&Dropdown.Filter), new(&Dropdown.ToggleDropdown)));
            background.AddComponent(new IsSelectable(canvas.SelectionMask));

            Label label = new(canvas, default(FixedString));
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

            Transform triangleTransform = triangle;
            triangleTransform.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI * 1f);

            Menu menu = new(canvas, optionSize, new(&Dropdown.ChosenOption));
            menu.Anchor = Anchor.BottomLeft;
            menu.Pivot = new(0f, 1f, 0f);
            menu.SetEnabled(false);

            T[] options = Enum.GetValues<T>();
            for (uint i = 0; i < options.Length; i++)
            {
                T option = options[i];
                menu.AddOption(option.ToString());
            }

            rint dropdownReference = menu.AddReference(background);
            menu.AddComponent(new DropdownMenu(dropdownReference));

            rint labelReference = background.AddReference(label);
            rint triangleReference = background.AddReference(triangle);
            rint menuReference = background.AddReference(menu);
            background.AddComponent(new IsDropdown(labelReference, triangleReference, menuReference, callback));

            dropdown = background.AsEntity().As<Dropdown>();
            dropdown.SelectedOption = "0";
        }

        public readonly void Dispose()
        {
            dropdown.Dispose();
        }

        public static implicit operator Dropdown(Dropdown<T> dropdown)
        {
            return dropdown.dropdown;
        }

        public static implicit operator Entity(Dropdown<T> dropdown)
        {
            return dropdown.dropdown;
        }
    }
}