using Data;
using InteractionKit.Components;
using InteractionKit.Functions;
using Simulation;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct TextField : IEntity, ISelectable
    {
        public readonly Image background;

        public readonly Entity Parent
        {
            get => background.Parent;
            set => background.Parent = value;
        }

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

        public readonly ref Color BackgroundColor => ref background.Color;
        public readonly ref Anchor Anchor => ref background.Anchor;
        public readonly ref Vector3 Pivot => ref background.Pivot;

        public readonly Label TextLabel
        {
            get
            {
                rint textReference = background.AsEntity().GetComponent<IsTextField>().textLabelReference;
                uint textEntity = background.GetReference(textReference);
                return new Entity(background.GetWorld(), textEntity).As<Label>();
            }
        }

        public readonly ref Color TextColor => ref TextLabel.Color;
        public readonly USpan<char> Value => TextLabel.Text;

        readonly uint IEntity.Value => background.GetEntityValue();
        readonly World IEntity.World => background.GetWorld();
        readonly Definition IEntity.Definition => new Definition().AddComponentTypes<IsTextField, IsSelectable>();

        public unsafe TextField(World world, InteractiveContext context, FixedString defaultValue = default)
        {
            background = new(world, context);
            background.AddComponent(new IsTrigger(new(&Filter), new(&StartEditing)));
            background.AddComponent(new IsSelectable());

            Label text = new(world, context, defaultValue);
            text.Parent = background;
            text.Anchor = Anchor.TopLeft;
            text.Color = Color.Black;
            text.Position = new(4f, -4f);
            text.Pivot = new(0f, 1f, 0f);

            Image cursor = new(world, context);
            cursor.Parent = background;
            cursor.Color = Color.Black;
            cursor.Anchor = new(new(0f, false), new(0f, false), default, new(0f, false), new(1f, false), default);
            cursor.Size = new(2f, 1f);
            cursor.Position = new(16f, 0f);

            Image highlight = new(world, context);
            highlight.transform.LocalPosition = new(0f, 0f, 0.2f);
            highlight.Parent = background;
            highlight.Color = Color.SkyBlue * new Color(1f, 1f, 1f, 0.5f);
            highlight.Anchor = new(new(0f, false), new(0f, false), default, new(0f, false), new(1f, false), default);
            highlight.Size = new(32, 1f);
            highlight.SetEnabled(false);

            rint textReference = background.AddReference(text);
            rint cursorReference = background.AddReference(cursor);
            rint highlightReference = background.AddReference(highlight);
            background.AddComponent(new IsTextField(textReference, cursorReference, highlightReference));
            background.AddComponent(context);
        }

        public readonly void SetText(USpan<char> text)
        {
            TextLabel.SetText(text);
        }

        public readonly void SetText(FixedString text)
        {
            TextLabel.SetText(text);
        }

        [UnmanagedCallersOnly]
        private static void Filter(FilterFunction.Input input)
        {
            World world = input.world;
            foreach (ref uint entity in input.Entities)
            {
                //todo: efficiency: doing individual calls within a filter function
                IsSelectable component = world.GetComponent<IsSelectable>(entity);
                if (!component.WasPrimaryInteractedWith || !component.IsSelected)
                {
                    entity = default;
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void StartEditing(World world, uint textFieldEntity)
        {
            ref IsTextField component = ref world.GetComponentRef<IsTextField>(textFieldEntity);
            component.editing = true;

            //stop editing other text fields
            foreach (uint entity in world.GetAll<IsTextField>())
            {
                if (entity != textFieldEntity)
                {
                    ref IsTextField otherComponent = ref world.GetComponentRef<IsTextField>(entity);
                    otherComponent.editing = false;
                }
            }
        }
    }
}