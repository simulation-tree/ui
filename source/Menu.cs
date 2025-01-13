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
    public readonly struct Menu : IEntity
    {
        private readonly Transform transform;

        public unsafe readonly ref Vector2 Position
        {
            get
            {
                ref Vector3 localPosition = ref transform.LocalPosition;
                fixed (Vector3* position = &localPosition)
                {
                    return ref *(Vector2*)position;
                }
            }
        }

        public readonly ref float Z
        {
            get
            {
                ref Vector3 localPosition = ref transform.LocalPosition;
                return ref localPosition.Z;
            }
        }

        public readonly ref Anchor Anchor => ref transform.AsEntity().GetComponent<Anchor>();
        public readonly ref Vector3 Pivot => ref transform.AsEntity().GetComponent<Pivot>().value;

        /// <summary>
        /// All options of this menu not including descendants.
        /// </summary>
        public readonly USpan<IsMenuOption> Options => transform.AsEntity().GetArray<IsMenuOption>();

        public readonly ref MenuCallback Callback
        {
            get
            {
                ref IsMenu component = ref transform.AsEntity().GetComponent<IsMenu>();
                return ref component.callback;
            }
        }

        public readonly Vector2 OptionSize
        {
            get
            {
                ref IsMenu component = ref transform.AsEntity().GetComponent<IsMenu>();
                return component.optionSize;
            }
            set
            {
                ref IsMenu component = ref transform.AsEntity().GetComponent<IsMenu>();
                component.optionSize = value;

                USpan<IsMenuOption> options = Options;
                for (uint i = 0; i < options.Length; i++)
                {
                    IsMenuOption option = options[i];
                    rint buttonReference = option.buttonReference;
                    uint buttonEntity = transform.GetReference(buttonReference);
                    Button optionButton = new Entity(transform.GetWorld(), buttonEntity).As<Button>();
                    optionButton.Size = value;
                    optionButton.Position = new(0, -value.Y * i);

                    rint childMenuReference = option.childMenuReference;
                    if (childMenuReference != default)
                    {
                        uint childMenuEntity = transform.GetReference(childMenuReference);
                        Menu childMenu = new Entity(transform.GetWorld(), childMenuEntity).As<Menu>();
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
                World world = transform.GetWorld();
                uint entity = transform.GetEntityValue();
                while (true)
                {
                    uint parent = world.GetParent(entity);
                    if (parent == default || !world.ContainsComponent<IsMenu>(parent))
                    {
                        return new Entity(world, entity).As<Menu>();
                    }

                    entity = parent;
                }
            }
        }

        public readonly bool IsExpanded
        {
            get => transform.IsEnabled();
            set
            {
                transform.SetEnabled(value);

                //keep nested menus disabled
                World world = transform.GetWorld();
                USpan<IsMenuOption> options = Options;
                for (uint i = 0; i < options.Length; i++)
                {
                    ref IsMenuOption option = ref options[i];
                    if (value)
                    {
                        option.expanded = false;
                    }

                    if (option.childMenuReference != default)
                    {
                        uint childMenuEntity = transform.GetReference(option.childMenuReference);
                        Menu childMenu = new Entity(world, childMenuEntity).As<Menu>();
                        childMenu.SetEnabled(false);
                    }
                }
            }
        }

        public readonly uint Count => transform.AsEntity().GetArrayLength<IsMenuOption>();

        public readonly MenuOption this[uint index]
        {
            get
            {
                USpan<IsMenuOption> options = Options;
                ref IsMenuOption option = ref options[index];
                OptionPath path = GetPath(this, index);
                return new(this, path);
            }
        }

        readonly uint IEntity.Value => transform.GetEntityValue();
        readonly World IEntity.World => transform.GetWorld();

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsMenu>();
            archetype.AddArrayElementType<IsMenuOption>();
        }

        public Menu(Canvas canvas, Vector2 optionSize, MenuCallback callback = default)
        {
            World world = canvas.GetWorld();
            transform = new(world);
            transform.SetParent(canvas);
            transform.LocalPosition = new(0, 0, Settings.ZScale);
            transform.AddComponent(new Anchor());
            transform.AddComponent(new Pivot());
            transform.AddComponent(new IsMenu(optionSize, callback));
            transform.AsEntity().CreateArray<IsMenuOption>();

            new DropShadow(canvas, transform);
        }

        public readonly void Dispose()
        {
            transform.Dispose();
        }

        /// <summary>
        /// Adds a new option.
        /// </summary>
        /// <returns>Index path towards this specific option local to this menu.</returns>
        public unsafe readonly OptionPath AddOption(FixedString label)
        {
            Vector2 optionSize = OptionSize;
            Entity entity = transform;
            Canvas canvas = entity.GetCanvas();
            uint optionCount = entity.GetArrayLength<IsMenuOption>();
            World world = transform.GetWorld();
            bool hasPath = label.TryIndexOf('/', out uint slashIndex);
            FixedString remainder = hasPath ? label.Slice(slashIndex + 1) : default;
            label = hasPath ? label.Slice(0, slashIndex) : label;
            if (hasPath)
            {
                //try to find existing option with same text
                USpan<IsMenuOption> existingOptions = entity.GetArray<IsMenuOption>();
                OptionPath path = default;
                for (uint i = 0; i < existingOptions.Length; i++)
                {
                    ref IsMenuOption existingOption = ref existingOptions[i];
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
                newChildMenu.SetParent(transform);
                newChildMenu.Position = new(optionSize.X, 0);
                newChildMenu.Anchor = Anchor.BottomLeft;
                newChildMenu.Pivot = new(0, 1f, 0f);
                newChildMenu.Callback = Callback;
                newChildMenu.SetEnabled(addedOption.expanded);

                uint buttonEntity = transform.GetReference(addedOption.buttonReference);
                Image triangle = new(canvas);
                triangle.SetParent(buttonEntity);
                triangle.Material = GetTriangleMaterialFromSettings(world, canvas.Camera);
                triangle.Anchor = Anchor.Right;
                triangle.Size = new(16f, 16f);
                triangle.Color = new(0, 0, 0, 1);
                triangle.Pivot = new(1f, -0.5f, 0f);

                Transform triangleTransform = triangle;
                triangleTransform.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI * -0.5f);

                addedOption.childMenuReference = transform.AddReference(newChildMenu);
                OptionPath pathInNew = newChildMenu.AddOption(remainder);
                path = path.Append(pathInNew);
                return path;
            }
            else
            {
                Button optionButton = new(new(&OptionChosen), canvas);
                optionButton.SetParent(transform);
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
                USpan<IsMenuOption> options = entity.ResizeArray<IsMenuOption>(optionCount + 1);
                rint buttonReference = transform.AddReference(optionButton);
                rint buttonLabelReference = transform.AddReference(optionButtonLabel);
                options[optionCount] = new(label, buttonReference, buttonLabelReference, childMenuReference);
                OptionPath path = default;
                path = path.Append(optionCount);
                return path;
            }
        }

        [UnmanagedCallersOnly]
        private unsafe static void OptionChosen(Entity optionButtonEntity)
        {
            World world = optionButtonEntity.GetWorld();
            Entity menuEntity = optionButtonEntity.GetParent();
            USpan<uint> menuChildren = menuEntity.GetChildren();
            uint chosenIndex = 0;
            for (uint i = 0; i < menuChildren.Length; i++)
            {
                uint childEntity = menuChildren[i];
                if (childEntity == optionButtonEntity.GetEntityValue())
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

        private static Material GetTriangleMaterialFromSettings(World world, Camera camera)
        {
            Settings settings = world.GetFirst<Settings>();
            return settings.GetTriangleMaterial(camera);
        }

        /// <summary>
        /// Retrieves the full path that referes to this local index.
        /// </summary>
        public static OptionPath GetPath(Menu menu, uint localIndex)
        {
            World world = menu.GetWorld();
            uint entity = menu.GetEntityValue();
            OptionPath path = default;
            while (true)
            {
                uint parent = world.GetParent(entity);
                path = path.Insert(0, localIndex);
                if (parent == default || !world.ContainsComponent<IsMenu>(parent))
                {
                    return path;
                }

                USpan<IsMenuOption> options = world.GetArray<IsMenuOption>(parent);
                for (uint i = 0; i < options.Length; i++)
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
            return menu.transform;
        }

        public static implicit operator Entity(Menu menu)
        {
            return menu.transform;
        }
    }
}