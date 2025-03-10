using Cameras;
using Materials;
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
    public readonly partial struct Menu : IEntity
    {
        public readonly ref Vector2 Position => ref As<UITransform>().Position;
        public readonly ref float X => ref As<UITransform>().X;
        public readonly ref float Y => ref As<UITransform>().Y;
        public readonly ref float Z => ref As<UITransform>().Z;
        public readonly float Rotation => As<UITransform>().Rotation;
        public readonly ref Anchor Anchor => ref As<UITransform>().Anchor;
        public readonly ref Vector3 Pivot => ref As<UITransform>().Pivot;
        public readonly int OptionCount => GetArrayLength<IsMenuOption>();

        /// <summary>
        /// All options of this menu not including descendants.
        /// </summary>
        public readonly Span<IsMenuOption> Options => GetArray<IsMenuOption>().AsSpan();

        public readonly ref MenuCallback Callback => ref GetComponent<IsMenu>().callback;

        public readonly Vector2 OptionSize
        {
            get => GetComponent<IsMenu>().optionSize;
            set
            {
                ref IsMenu component = ref GetComponent<IsMenu>();
                component.optionSize = value;

                Span<IsMenuOption> options = Options;
                for (int i = 0; i < options.Length; i++)
                {
                    IsMenuOption option = options[i];
                    rint buttonReference = option.buttonReference;
                    uint buttonEntity = GetReference(buttonReference);
                    Button optionButton = new Entity(world, buttonEntity).As<Button>();
                    optionButton.Size = value;
                    optionButton.Position = new(0, -value.Y * i);

                    rint childMenuReference = option.childMenuReference;
                    if (childMenuReference != default)
                    {
                        uint childMenuEntity = GetReference(childMenuReference);
                        Menu childMenu = new Entity(world, childMenuEntity).As<Menu>();
                        childMenu.OptionSize = value;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the root <see cref="Menu"/> that contains this menu.
        /// <para>
        /// May be itself.
        /// </para>
        /// </summary>
        public readonly Menu RootMenu
        {
            get
            {
                uint current = value;
                while (true)
                {
                    uint parent = world.GetParent(current);
                    if (parent == default || !world.ContainsComponent<IsMenu>(parent))
                    {
                        return new Entity(world, current).As<Menu>();
                    }

                    current = parent;
                }
            }
        }

        public readonly bool IsExpanded
        {
            get => IsEnabled;
            set
            {
                IsEnabled = value;

                //keep nested menus disabled
                Span<IsMenuOption> options = Options;
                for (int i = 0; i < options.Length; i++)
                {
                    ref IsMenuOption option = ref options[i];
                    if (value)
                    {
                        option.expanded = false;
                    }

                    if (option.childMenuReference != default)
                    {
                        uint childMenuEntity = GetReference(option.childMenuReference);
                        Menu childMenu = new Entity(world, childMenuEntity).As<Menu>();
                        childMenu.IsEnabled = false;
                    }
                }
            }
        }

        public readonly MenuOption this[int index]
        {
            get
            {
                Span<IsMenuOption> options = Options;
                ref IsMenuOption option = ref options[index];
                OptionPath path = GetPath(this, index);
                return new(this, path);
            }
        }

        public Menu(Canvas canvas, Vector2 optionSize, MenuCallback callback = default)
        {
            world = canvas.world;
            Transform transform = new(world);
            transform.SetParent(canvas);
            transform.LocalPosition = new(0, 0, Settings.ZScale);
            transform.AddComponent(new Anchor());
            transform.AddComponent(new Pivot());
            transform.AddComponent(new IsMenu(optionSize, callback));
            transform.CreateArray<IsMenuOption>();
            value = transform.value;

            new DropShadow(canvas, transform);
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsMenu>();
            archetype.AddArrayType<IsMenuOption>();
        }

        /// <summary>
        /// Adds a new option.
        /// </summary>
        /// <returns>Index path towards this specific option local to this menu.</returns>
        public unsafe readonly OptionPath AddOption(ASCIIText256 label)
        {
            Vector2 optionSize = OptionSize;
            Entity entity = this;
            Canvas canvas = entity.GetCanvas();
            Values<IsMenuOption> options = entity.GetArray<IsMenuOption>();
            int optionCount = options.Length;
            bool hasPath = label.TryIndexOf('/', out int slashIndex);
            ASCIIText256 remainder = hasPath ? label.Slice(slashIndex + 1) : default;
            label = hasPath ? label.Slice(0, slashIndex) : label;
            if (hasPath)
            {
                //try to find existing option with same text
                OptionPath path = default;
                for (int i = 0; i < options.Length; i++)
                {
                    ref IsMenuOption existingOption = ref options[i];
                    if (existingOption.text == label)
                    {
                        if (existingOption.childMenuReference != default)
                        {
                            path = path.Append(i);
                            uint existingChildMenuEntity = entity.GetReference(existingOption.childMenuReference);
                            Menu existingChildMenu = new Entity(world, existingChildMenuEntity).As<Menu>();
                            OptionPath pathInExisting = existingChildMenu.AddOption(remainder);
                            path = path.Append(pathInExisting);
                            return path;
                        }
                        else
                        {
                            throw new InvalidOperationException("Cannot add option to existing option");
                        }
                    }
                }

                OptionPath firstPath = AddOption(label);
                path = path.Append(firstPath);

                //todo: for some reason, the buttons in child menus position themselves upwards
                //while the menus of dropdowns position downwards
                ref IsMenuOption addedOption = ref entity.GetArrayElement<IsMenuOption>(optionCount);
                Menu newChildMenu = new(canvas, optionSize);
                newChildMenu.SetParent(entity);
                newChildMenu.Position = new(optionSize.X, 0);
                newChildMenu.Anchor = Anchor.BottomLeft;
                newChildMenu.Pivot = new(0, 1f, 0f);
                newChildMenu.Callback = Callback;
                newChildMenu.IsEnabled = addedOption.expanded;

                uint buttonEntity = GetReference(addedOption.buttonReference);
                Image triangle = new(canvas);
                triangle.SetParent(buttonEntity);
                triangle.Material = GetTriangleMaterialFromSettings(world, canvas.Camera);
                triangle.Anchor = Anchor.Right;
                triangle.Size = new(16f, 16f);
                triangle.Color = new(0, 0, 0, 1);
                triangle.Pivot = new(1f, -0.5f, 0f);

                UITransform triangleTransform = triangle;
                triangleTransform.Rotation = MathF.Tau * -0.25f;

                addedOption.childMenuReference = AddReference(newChildMenu);
                OptionPath pathInNew = newChildMenu.AddOption(remainder);
                path = path.Append(pathInNew);
                return path;
            }
            else
            {
                Button optionButton = new(new(&OptionChosen), canvas);
                optionButton.SetParent(entity);
                optionButton.Position = new(0, -optionSize.Y * optionCount);
                optionButton.Size = optionSize;
                optionButton.Anchor = Anchor.TopLeft;
                optionButton.Pivot = new(0f, 1f, 0f);

                Label optionButtonLabel = new(canvas, label);
                optionButtonLabel.SetParent(optionButton);
                optionButtonLabel.Anchor = Anchor.TopLeft;
                optionButtonLabel.Color = new(0, 0, 0, 1);
                optionButtonLabel.Position = new(4f, -4f);
                optionButtonLabel.Pivot = new(0f, 1f, 0f);

                rint childMenuReference = default;
                options.Length++;
                rint buttonReference = AddReference(optionButton);
                rint buttonLabelReference = AddReference(optionButtonLabel);
                options[optionCount] = new(label, buttonReference, buttonLabelReference, childMenuReference);
                OptionPath path = default;
                path = path.Append(optionCount);
                return path;
            }
        }

        [UnmanagedCallersOnly]
        private unsafe static void OptionChosen(Entity optionButtonEntity)
        {
            World world = optionButtonEntity.world;
            Entity menuEntity = optionButtonEntity.Parent;
            int childCount = menuEntity.ChildCount;
            if (childCount > 0)
            {
                Span<uint> menuChildren = stackalloc uint[childCount];
                menuEntity.CopyChildrenTo(menuChildren);
                int chosenIndex = 0;
                for (int i = 0; i < menuChildren.Length; i++)
                {
                    uint childEntity = menuChildren[i];
                    if (childEntity == optionButtonEntity.value)
                    {
                        break;
                    }

                    if (world.ContainsComponent<IsTrigger>(childEntity))
                    {
                        chosenIndex++;
                    }
                }

                Menu menu = menuEntity.As<Menu>();
                ref IsMenuOption option = ref menuEntity.GetArrayElement<IsMenuOption>(chosenIndex);
                if (option.childMenuReference != default)
                {
                    option.expanded = !option.expanded;
                    uint childMenuEntity = menuEntity.GetReference(option.childMenuReference);
                    Menu childMenu = new Entity(world, childMenuEntity).As<Menu>();
                    Vector2 optionSize = menu.OptionSize;
                    childMenu.OptionSize = optionSize;
                    childMenu.Position = new(optionSize.X, 0);
                    childMenu.IsExpanded = option.expanded;
                }
                else
                {
                    ref IsMenu component = ref menuEntity.GetComponent<IsMenu>();
                    if (component.callback != default)
                    {
                        Menu rootMenu = menu.RootMenu;
                        OptionPath optionPath = GetPath(menu, chosenIndex);
                        MenuOption chosenOption = new(rootMenu, optionPath);
                        component.callback.Invoke(chosenOption);
                    }
                }
            }
        }

        private static Material GetTriangleMaterialFromSettings(World world, Camera camera)
        {
            Settings settings = world.GetFirst<Settings>();
            return settings.GetTriangleMaterial(camera);
        }

        /// <summary>
        /// Retrieves the full path that referes to this local index.
        /// </summary>
        public static OptionPath GetPath(Menu menu, int localIndex)
        {
            World world = menu.world;
            uint entity = menu.value;
            OptionPath path = default;
            while (true)
            {
                uint parent = world.GetParent(entity);
                path = path.Insert(0, localIndex);
                if (parent == default || !world.ContainsComponent<IsMenu>(parent))
                {
                    return path;
                }

                Values<IsMenuOption> options = world.GetArray<IsMenuOption>(parent);
                for (int i = 0; i < options.Length; i++)
                {
                    rint childMenuReference = options[i].childMenuReference;
                    if (childMenuReference != default && world.GetReference(parent, childMenuReference) == entity)
                    {
                        localIndex = i;
                        break;
                    }
                }

                entity = parent;
            }
        }

        public static implicit operator Transform(Menu menu)
        {
            return menu.As<Transform>();
        }
    }
}