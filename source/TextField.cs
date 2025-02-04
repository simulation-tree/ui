using Rendering;
using Rendering.Components;
using System.Numerics;
using Transforms;
using Transforms.Components;
using UI.Components;
using UI.Functions;
using Unmanaged;
using Worlds;

namespace UI
{
    public readonly partial struct TextField : ISelectable
    {
        public readonly ref Vector2 Position => ref As<UITransform>().Position;
        public readonly ref float X => ref As<UITransform>().X;
        public readonly ref float Y => ref As<UITransform>().Y;
        public readonly ref float Z => ref As<UITransform>().Z;
        public readonly ref Vector2 Size => ref As<UITransform>().Size;
        public readonly ref float Width => ref As<UITransform>().Width;
        public readonly ref float Height => ref As<UITransform>().Height;

        public readonly float Rotation
        {
            get => As<UITransform>().Rotation;
            set => As<UITransform>().Rotation = value;
        }

        public readonly ref Anchor Anchor => ref As<UITransform>().Anchor;
        public readonly ref Vector3 Pivot => ref As<UITransform>().Pivot;
        public readonly ref bool Editing => ref GetComponent<IsTextField>().editing;
        public readonly ref Vector4 BackgroundColor => ref As<Image>().Color;

        public readonly Label TextLabel
        {
            get
            {
                rint textReference = GetComponent<IsTextField>().textLabelReference;
                uint textEntity = GetReference(textReference);
                return new Entity(world, textEntity).As<Label>();
            }
        }

        public readonly Image Cursor
        {
            get
            {
                rint cursorReference = GetComponent<IsTextField>().cursorReference;
                uint cursorEntity = GetReference(cursorReference);
                return new Entity(world, cursorEntity).As<Image>();
            }
        }

        public readonly Vector4 TextColor
        {
            get => TextLabel.Color;
            set
            {
                TextLabel.Color = value;
                Cursor.Color = value;
            }
        }

        public readonly ref BeginEditing BeginEditing => ref GetComponent<IsTextField>().beginEditing;
        public readonly ref TextValidation Validation => ref GetComponent<IsTextField>().validation;
        public readonly ref Submit Submit => ref GetComponent<IsTextField>().submit;
        public readonly ref Cancel Cancel => ref GetComponent<IsTextField>().cancel;

        /// <summary>
        /// The content of this text field.
        /// </summary>
        public readonly USpan<char> Value => TextLabel.Text;

        public unsafe TextField(Canvas canvas, FixedString defaultValue = default, BeginEditing beginEditing = default, TextValidation validation = default, Submit submit = default, Cancel cancel = default)
        {
            world = canvas.world;
            Image background = new(canvas);
            value = background.value;

            background.AddComponent(new IsSelectable(canvas.SelectionMask));

            Label text = new(canvas, defaultValue);
            text.SetParent(background);
            text.Anchor = Anchor.TopLeft;
            text.Color = new(0, 0, 0, 1);
            text.Position = new(4f, -4f);
            text.Pivot = new(0f, 1f, 0f);
            text.AddComponent(new RendererScissor());

            Image cursor = new(canvas);
            cursor.SetParent(background);
            cursor.Anchor = Anchor.TopLeft;
            cursor.Color = new(0, 0, 0, 1);
            cursor.Size = new(2f, 16f);
            cursor.Position = new(16f, 0f);
            cursor.AddComponent(new RendererScissor());

            Image highlight = new(canvas);
            highlight.SetParent(background);
            highlight.Color = new(0f, 0.4f, 1f, 0.3f);
            highlight.Anchor = Anchor.TopLeft;
            highlight.Position = new(4f, -4f);
            highlight.Size = new(1, 1);
            highlight.Z = Settings.ZScale * 1.5f;
            highlight.IsEnabled = false;
            highlight.AddComponent(new RendererScissor());

            MeshRenderer highlightRenderer = highlight;
            highlightRenderer.Mesh = highlightRenderer.Mesh.Clone();
            highlightRenderer.Mesh.CreateColors(0);

            rint textReference = background.AddReference(text);
            rint cursorReference = background.AddReference(cursor);
            rint highlightReference = background.AddReference(highlight);
            background.AddComponent(new IsTextField(textReference, cursorReference, highlightReference, beginEditing, validation, submit, cancel));
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsTextField>();
            archetype.AddComponentType<IsSelectable>();
        }

        public readonly void SetText(USpan<char> newText)
        {
            ref TextValidation validation = ref Validation;
            if (validation != default)
            {
                using Text newTextContainer = new(newText);
                validation.Invoke(Value, newTextContainer);
                TextLabel.SetText(newTextContainer.AsSpan());
            }
            else
            {
                TextLabel.SetText(newText);
            }
        }

        public readonly void SetText(FixedString newText)
        {
            USpan<char> buffer = stackalloc char[newText.Length];
            uint length = newText.CopyTo(buffer);
            SetText(buffer.Slice(0, length));
        }

        public static implicit operator Image(TextField textField)
        {
            return textField.As<Image>();
        }

        public static implicit operator UITransform(TextField textField)
        {
            return textField.As<UITransform>();
        }

        public static implicit operator Transform(TextField textField)
        {
            return textField.As<Transform>();
        }
    }
}