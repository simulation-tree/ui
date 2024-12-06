using Data;
using InteractionKit.Components;
using InteractionKit.Functions;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms;
using Transforms.Components;
using Unmanaged;
using Worlds;

namespace InteractionKit
{
    public readonly struct TextField : ISelectable, ICanvasDescendant
    {
        private readonly Image background;

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

        public readonly ref bool Editing => ref background.AsEntity().GetComponent<IsTextField>().editing;
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
            background.AddComponent(new IsSelectable());

            Label text = new(world, canvas, defaultValue);
            text.SetParent(background);
            text.Anchor = Anchor.TopLeft;
            text.Color = Color.Black;
            text.Position = new(4f, -4f);
            text.Pivot = new(0f, 1f, 0f);

            Image cursor = new(world, canvas);
            cursor.SetParent(background);
            cursor.Color = Color.Black;
            cursor.Anchor = new(new(0f, false), new(0f, false), default, new(0f, false), new(1f, false), default);
            cursor.Size = new(2f, 1f);
            cursor.Position = new(16f, 0f);

            Image highlight = new(world, canvas);
            highlight.SetParent(background);
            highlight.Color = new Color(0.3f, 0.3f, 0.3f, 1f);
            highlight.Anchor = new(new(0f, false), new(0f, false), default, new(0f, false), new(1f, false), default);
            highlight.Size = new(0f, 1f);
            highlight.SetEnabled(false);

            Transform highlightTransform = highlight;
            highlightTransform.LocalPosition = new(0f, 0f, 0.05f);

            rint textReference = background.AddReference(text);
            rint cursorReference = background.AddReference(cursor);
            rint highlightReference = background.AddReference(highlight);
            background.AddComponent(new IsTextField(textReference, cursorReference, highlightReference));
        }

        public readonly void Dispose()
        {
            background.Dispose();
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
        private static void Filter(TriggerFilter.Input input)
        {
            foreach (ref Entity entity in input.Entities)
            {
                //todo: efficiency: doing individual calls within a filter function
                IsSelectable component = entity.GetComponent<IsSelectable>();
                if (!component.WasPrimaryInteractedWith || !component.IsSelected)
                {
                    entity = default;
                }
            }
        }

        public static implicit operator Entity(TextField textField)
        {
            return textField.background;
        }

        public static implicit operator Image(TextField textField)
        {
            return textField.background;
        }

        public static implicit operator Transform(TextField textField)
        {
            return textField.background;
        }
    }
}