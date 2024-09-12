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
                    rint buttonReference = options[i].buttonReference;
                    uint buttonEntity = transform.AsEntity().GetReference(buttonReference);
                    Button optionButton = new Entity(transform.GetWorld(), buttonEntity).As<Button>();
                    optionButton.Size = value;
                    optionButton.Position = new(0, -value.Y * i);
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
        public unsafe readonly FixedString AddOption(FixedString label, InteractiveContext context)
        {
            Entity entity = transform.AsEntity();
            uint optionCount = entity.GetArrayLength<MenuOption>();
            World world = transform.GetWorld();
            bool hasPath = label.TryIndexOf('/', out uint slashIndex);
            FixedString remainder = hasPath ? label.Slice(slashIndex + 1) : default;
            label = hasPath ? label.Slice(0, slashIndex) : label;
            if (hasPath)
            {
                //try to find existing option with same text
                USpan<MenuOption> options = entity.GetArray<MenuOption>();
                FixedString path = default;
                for (uint i = 0; i < options.length; i++)
                {
                    if (options[i].text == label)
                    {
                        path.Append(i);
                        return path.Append(remainder);
                    }
                }

                //missing, create new
                //options = entity.ResizeArray<MenuOption>(optionCount + 1);
                //path.Append(optionCount);
                //return path;

                FixedString firstPath = AddOption(label, context);
                ref MenuOption addedOption = ref entity.GetArrayElementRef<MenuOption>(optionCount);
                Menu childMenu = new(world);
                rint childMenuReference = entity.AddReference(childMenu.transform);
            }
            else
            {
                Vector2 size = Size;
                Button optionButton = new(world, new(&OptionChosen), context);
                optionButton.Parent = entity;
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
                rint buttonReference = entity.AddReference(optionButton);
                rint buttonLabelReference = entity.AddReference(optionButtonLabel);
                options[optionCount] = new(label, buttonReference, buttonLabelReference, childMenuReference);
                FixedString path = default;
                path.Append(optionCount);
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

            MenuOption option = world.GetArrayElementRef<MenuOption>(menuEntity, chosenIndex);
            if (option.childMenuReference != default)
            {
                uint childMenuEntity = world.GetReference(menuEntity, option.childMenuReference);
                //world.SetEnabled(childDropdownEntity, true);
            }
            else
            {
                ref IsMenu component = ref world.GetComponentRef<IsMenu>(menuEntity);
                if (component.callback != default)
                {
                    Menu menu = new Entity(world, menuEntity).As<Menu>();
                    component.callback.Invoke(menu, chosenIndex);
                }
            }
        }
    }
}