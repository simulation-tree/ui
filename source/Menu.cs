using Data;
using InteractionKit.Components;
using InteractionKit.Functions;
using Simulation;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct Menu : IEntity
    {
        public readonly Transform transform;

        public readonly Entity Parent
        {
            get => transform.Parent;
            set => transform.Parent = value;
        }

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

                USpan<MenuOption> options = Options;
                for (uint i = 0; i < options.length; i++)
                {
                    MenuOption option = options[i];
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
                        childMenu.Size = value;
                    }
                }
            }
        }

        public readonly ref Anchor Anchor => ref transform.entity.GetComponentRef<Anchor>();
        public readonly ref Vector3 Pivot => ref transform.entity.GetComponentRef<Pivot>().value;

        public readonly USpan<MenuOption> Options => transform.AsEntity().GetArray<MenuOption>();

        public readonly ref MenuCallbackFunction Callback
        {
            get
            {
                ref IsMenu component = ref transform.AsEntity().GetComponentRef<IsMenu>();
                return ref component.callback;
            }
        }

        readonly uint IEntity.Value => transform.GetEntityValue();
        readonly World IEntity.World => transform.GetWorld();
        readonly Definition IEntity.Definition => new Definition().AddComponentType<IsMenu>().AddArrayType<MenuOption>();

        public Menu(World world, MenuCallbackFunction callback = default)
        {
            transform = new(world);
            transform.LocalPosition = new(0, 0, 0.1f);
            transform.AsEntity().AddComponent(new Anchor());
            transform.AsEntity().AddComponent(new Pivot());
            transform.AsEntity().AddComponent(new IsMenu(callback));
            transform.AsEntity().CreateArray<MenuOption>();
        }

        /// <summary>
        /// Adds a new option.
        /// </summary>
        /// <returns>Index path towards this specific option.</returns>
        public unsafe readonly OptionPath AddOption(FixedString label, InteractiveContext context)
        {
            Vector2 size = Size;
            Entity entity = transform.AsEntity();
            uint optionCount = entity.GetArrayLength<MenuOption>();
            World world = transform.GetWorld();
            bool hasPath = label.TryIndexOf('/', out uint slashIndex);
            FixedString remainder = hasPath ? label.Slice(slashIndex + 1) : default;
            label = hasPath ? label.Slice(0, slashIndex) : label;
            if (hasPath)
            {
                //try to find existing option with same text
                USpan<MenuOption> existingOptions = entity.GetArray<MenuOption>();
                OptionPath path = default;
                for (uint i = 0; i < existingOptions.length; i++)
                {
                    ref MenuOption existingOption = ref existingOptions[i];
                    if (existingOption.text == label)
                    {
                        if (existingOption.childMenuReference != default)
                        {
                            path = path.Append(i);
                            uint existingChildMenuEntity = entity.GetReference(existingOption.childMenuReference);
                            Menu existingChildMenu = new Entity(world, existingChildMenuEntity).As<Menu>();
                            OptionPath pathInExisting = existingChildMenu.AddOption(remainder, context);
                            path = path.Append(pathInExisting);
                            return path;
                        }
                        else
                        {
                            throw new InvalidOperationException("Cannot add option to existing option.");
                        }
                    }
                }

                OptionPath firstPath = AddOption(label, context);
                path = path.Append(firstPath);

                //todo: for some reason, the buttons in child menus position themselves upwards
                //while the menus of dropdowns position downwards
                ref MenuOption addedOption = ref entity.GetArrayElementRef<MenuOption>(optionCount);
                Menu newChildMenu = new(world);
                newChildMenu.Parent = transform;
                newChildMenu.Position = new(size.X, 0);
                newChildMenu.Size = size;
                newChildMenu.Anchor = Anchor.BottomLeft;
                newChildMenu.Pivot = new(0, 1f, 0f);
                newChildMenu.Callback = Callback;
                newChildMenu.SetEnabled(addedOption.expanded);

                uint buttonEntity = transform.GetReference(addedOption.buttonReference);
                Box triangle = new(world, context);
                triangle.Parent = new Entity(world, buttonEntity);
                triangle.Material = context.TriangleMaterial;
                triangle.Anchor = Anchor.Right;
                triangle.Size = new(16f, 16f);
                triangle.Color = Color.Black;
                triangle.Pivot = new(1f, -0.5f, 0f);
                triangle.transform.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI * -0.5f);

                addedOption.childMenuReference = transform.AddReference(newChildMenu);
                OptionPath pathInNew = newChildMenu.AddOption(remainder, context);
                path = path.Append(pathInNew);
                return path;
            }
            else
            {
                Button optionButton = new(world, new(&OptionChosen), context);
                optionButton.Parent = transform;
                optionButton.Position = new(0, -size.Y * optionCount);
                optionButton.Size = size;
                optionButton.Anchor = Anchor.TopLeft;
                optionButton.Pivot = new(0f, 1f, 0f);

                Label optionButtonLabel = new(world, context, label);
                optionButtonLabel.Parent = optionButton.AsEntity();
                optionButtonLabel.Anchor = Anchor.TopLeft;
                optionButtonLabel.Color = Color.Black;
                optionButtonLabel.Position = new(4f, -4f);
                optionButtonLabel.Pivot = new(0f, 1f, 0f);

                rint childMenuReference = default;
                USpan<MenuOption> options = entity.ResizeArray<MenuOption>(optionCount + 1);
                rint buttonReference = transform.AddReference(optionButton);
                rint buttonLabelReference = transform.AddReference(optionButtonLabel);
                options[optionCount] = new(label, buttonReference, buttonLabelReference, childMenuReference);
                OptionPath path = default;
                path = path.Append(optionCount);
                return path;
            }
        }

        [UnmanagedCallersOnly]
        private unsafe static void OptionChosen(World world, uint optionButtonEntity)
        {
            uint menuEntity = world.GetParent(optionButtonEntity);
            USpan<uint> menuChildren = world.GetChildren(menuEntity);
            uint chosenIndex = 0;
            for (uint i = 0; i < menuChildren.length; i++)
            {
                uint childEntity = menuChildren[i];
                if (childEntity == optionButtonEntity)
                {
                    break;
                }

                if (world.ContainsComponent<IsTrigger>(childEntity))
                {
                    chosenIndex++;
                }
            }

            Menu menu = new Entity(world, menuEntity).As<Menu>();
            ref MenuOption option = ref world.GetArrayElementRef<MenuOption>(menuEntity, chosenIndex);
            if (option.childMenuReference != default)
            {
                option.expanded = !option.expanded;
                uint childMenuEntity = world.GetReference(menuEntity, option.childMenuReference);
                world.SetEnabled(childMenuEntity, option.expanded);
                if (option.expanded)
                {
                    Vector2 size = menu.Size;
                    Menu childMenu = new Entity(world, childMenuEntity).As<Menu>();
                    childMenu.Size = size;
                    childMenu.Position = new(size.X, 0);

                    USpan<MenuOption> childMenuOptions = childMenu.Options;
                    for (uint i = 0; i < childMenuOptions.length; i++)
                    {
                        ref MenuOption childMenuOption = ref childMenuOptions[i];
                        if (childMenuOption.childMenuReference != default)
                        {
                            uint subChildMenu = world.GetReference(childMenuEntity, childMenuOption.childMenuReference);
                            world.SetEnabled(subChildMenu, childMenuOption.expanded);
                        }
                    }
                }
            }
            else
            {
                ref IsMenu component = ref world.GetComponentRef<IsMenu>(menuEntity);
                if (component.callback != default)
                {
                    component.callback.Invoke(menu, chosenIndex);
                }
            }
        }
    }
}