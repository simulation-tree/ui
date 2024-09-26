using Data;
using InteractionKit.Components;
using InteractionKit.Functions;
using Simulation;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct TextField : IEntity, ISelectable, ICanvasDescendant
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

        public readonly ref bool Editing => ref background.AsEntity().GetComponentRef<IsTextField>().editing;
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

        public readonly Image Cursor
        {
            get
            {
                rint cursorReference = background.AsEntity().GetComponent<IsTextField>().cursorReference;
                uint cursorEntity = background.GetReference(cursorReference);
                return new Entity(background.GetWorld(), cursorEntity).As<Image>();
            }
        }

        public readonly Color TextColor
        {
            get
            {
                return TextLabel.Color;
            }
            set
            {
                TextLabel.Color = value;
                Cursor.Color = value;
            }
        }

        public readonly USpan<char> Value => TextLabel.Text;

        readonly uint IEntity.Value => background.GetEntityValue();
        readonly World IEntity.World => background.GetWorld();
        readonly Definition IEntity.Definition => new Definition().AddComponentTypes<IsTextField, IsSelectable>();

        public unsafe TextField(World world, Canvas canvas, FixedString defaultValue = default)
        {
            background = new(world, canvas);
            background.AddComponent(new IsTrigger(new(&Filter), new(&StartEditing)));
            background.AddComponent(new IsSelectable());

            Label text = new(world, canvas, defaultValue);
            text.Parent = background;
            text.Anchor = Anchor.TopLeft;
            text.Color = Color.Black;
            text.Position = new(4f, -4f);
            text.Pivot = new(0f, 1f, 0f);

            Image cursor = new(world, canvas);
            cursor.Parent = background;
            cursor.Color = Color.Black;
            cursor.Anchor = new(new(0f, false), new(0f, false), default, new(0f, false), new(1f, false), default);
            cursor.Size = new(2f, 1f);
            cursor.Position = new(16f, 0f);

            Image highlight = new(world, canvas);
            highlight.transform.LocalPosition = new(0f, 0f, 0.05f);
            highlight.Parent = background;
            highlight.Color = new Color(0.3f, 0.3f, 0.3f, 1f);
            highlight.Anchor = new(new(0f, false), new(0f, false), default, new(0f, false), new(1f, false), default);
            highlight.Size = new(0f, 1f);
            highlight.SetEnabled(false);

            rint textReference = background.AddReference(text);
            rint cursorReference = background.AddReference(cursor);
            rint highlightReference = background.AddReference(highlight);
            background.AddComponent(new IsTextField(textReference, cursorReference, highlightReference));
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
            //stop editing other text fields
            foreach (uint entity in world.GetAll<IsTextField>())
            {
                ref IsTextField otherComponent = ref world.GetComponentRef<IsTextField>(entity);
                otherComponent.editing = false;
            }

            Pointer pointer = world.GetFirst<Pointer>();
            TextField textField = new Entity(world, textFieldEntity).As<TextField>();
            textField.Editing = true;
            Vector3 worldPosition = textField.AsEntity().As<Transform>().WorldPosition;
            Vector2 pointerPosition = pointer.Position;
            pointerPosition.X -= worldPosition.X;
            pointerPosition.Y -= worldPosition.Y;

            Label textLabel = textField.TextLabel;
            USpan<char> text = textLabel.Text;
            Settings settings = world.GetFirst<Settings>();
            (uint start, uint end, uint index) range = settings.EditRange;
            USpan<char> tempText = stackalloc char[(int)(text.Length + 1)];
            text.CopyTo(tempText);
            tempText[text.Length] = ' ';
            if (textLabel.Font.TryIndexOf(tempText, 32, pointerPosition / 16f, out uint newIndex))
            {
                bool holdingShift = settings.PressedCharacters.Contains(Settings.ShiftCharacter);
                if (holdingShift)
                {
                    uint start = Math.Min(range.start, range.end);
                    uint end = Math.Max(range.start, range.end);
                    uint length = end - start;
                    if (length == 0)
                    {
                        range.start = range.index;
                    }

                    range.end = newIndex;
                }
                else
                {
                    range.start = 0;
                    range.end = 0;
                }

                range.index = newIndex;
            }

            settings.EditRange = range;
        }
    }
}