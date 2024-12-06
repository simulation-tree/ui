using Cameras;
using Data;
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

        public readonly Vector2 Position
        {
            get => background.Position;
            set => background.Position = value;
        }

        public readonly Vector2 Size
        {
            get => background.Size;
            set => background.Size = value;
        }

        public readonly ref Anchor Anchor => ref background.Anchor;
        public readonly ref Vector3 Pivot => ref background.Pivot;
        public readonly ref Color BackgroundColor => ref background.Color;

        public readonly Label Label
        {
            get
            {
                rint labelReference = background.AsEntity().GetComponent<IsDropdown>().labelReference;
                uint labelEntity = background.GetReference(labelReference);
                return new(background.GetWorld(), labelEntity);
            }
        }

        public readonly ref Color LabelColor => ref Label.Color;

        public readonly Image Triangle
        {
            get
            {
                rint triangleReference = background.AsEntity().GetComponent<IsDropdown>().triangleReference;
                uint triangleEntity = background.GetReference(triangleReference);
                return new(background.GetWorld(), triangleEntity);
            }
        }

        public readonly ref Color TriangleColor => ref Triangle.Color;

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
                USpan<MenuOption> options = menu.Options;
                FixedString text = default;
                OptionPath path = value;
                while (path.Length > 0)
                {
                    uint index = path[0];
                    if (path.Length > 1)
                    {
                        if (index < options.Length)
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
                        if (index < options.Length)
                        {
                            MenuOption option = options[index];
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
                menu.Size = Size;
                menu.SetEnabled(component.expanded);

                USpan<MenuOption> options = menu.Options;
                for (uint i = 0; i < options.Length; i++)
                {
                    ref MenuOption option = ref options[i];
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

        public readonly USpan<MenuOption> Options => Menu.Options;

        public readonly ref DropdownCallbackFunction Callback
        {
            get
            {
                ref IsDropdown component = ref background.AsEntity().GetComponent<IsDropdown>();
                return ref component.callback;
            }
        }

        readonly uint IEntity.Value => background.GetEntityValue();
        readonly World IEntity.World => background.GetWorld();
        readonly Definition IEntity.Definition => new Definition().AddComponentTypes<IsTrigger, IsSelectable, IsDropdown>();

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

        public unsafe Dropdown(World world, Canvas canvas, DropdownCallbackFunction callback = default)
        {
            background = new(world, canvas);
            background.AddComponent(new IsTrigger(new(&Filter), new(&ToggleDropdown)));
            background.AddComponent(new IsSelectable());

            Label label = new(world, canvas, "");
            label.SetParent(background);
            label.Anchor = Anchor.TopLeft;
            label.Color = Color.Black;
            label.Position = new(4f, -4f);
            label.Pivot = new(0f, 1f, 0f);

            Image triangle = new(world, canvas);
            triangle.SetParent(background);
            triangle.Material = GetTriangleMaterialFromSettings(world, canvas.Camera);
            triangle.Anchor = Anchor.TopRight;
            triangle.Size = new(16f, 16f);
            triangle.Color = Color.Black;
            triangle.Pivot = new(0.5f, 0.5f, 0f);

            Transform triangleTransform = triangle;
            triangleTransform.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI * 1f);

            Menu menu = new(world, new(&ChosenOption));
            menu.SetParent(background);
            menu.Anchor = Anchor.BottomLeft;
            menu.Size = background.Size;
            menu.Pivot = new(0f, 1f, 0f);
            menu.SetEnabled(false);

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
        private static void ChosenOption(Menu menu, uint chosenOption)
        {
            OptionPath path = default;
            path = path.Append(chosenOption);

            World world = menu.GetWorld();
            uint childEntity = menu.GetEntityValue();
            uint parentEntity = menu.GetParent().GetEntityValue();
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
                    for (uint i = 0; i < parentOptions.Length; i++)
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
        private static void ToggleDropdown(Entity dropdownEntity)
        {
            Dropdown dropdown = dropdownEntity.As<Dropdown>();
            dropdown.IsExpanded = !dropdown.IsExpanded;
        }

        private static Material GetTriangleMaterialFromSettings(World world, Camera camera)
        {
            Settings settings = world.GetFirst<Settings>();
            return settings.GetTriangleMaterial(camera);
        }
    }
}