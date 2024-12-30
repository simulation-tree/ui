using Collections;
using Data;
using InteractionKit.Components;
using InteractionKit.Functions;
using Meshes;
using Rendering;
using Rendering.Components;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Unmanaged;
using Worlds;

namespace InteractionKit
{
    public readonly struct TextField : ISelectable
    {
        private readonly Image background;

        public readonly ref Vector2 Position => ref background.Position;
        public readonly ref Vector2 Size => ref background.Size;
        public readonly ref float Z => ref background.Z;
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

        public readonly ref BeginEditing BeginEditing => ref background.AsEntity().GetComponent<IsTextField>().beginEditing;
        public readonly ref TextValidation Validation => ref background.AsEntity().GetComponent<IsTextField>().validation;
        public readonly ref Submit Submit => ref background.AsEntity().GetComponent<IsTextField>().submit;
        public readonly ref Cancel Cancel => ref background.AsEntity().GetComponent<IsTextField>().cancel;

        /// <summary>
        /// The content of this text field.
        /// </summary>
        public readonly USpan<char> Value => TextLabel.Text;

        readonly uint IEntity.Value => background.GetEntityValue();
        readonly World IEntity.World => background.GetWorld();

        readonly Definition IEntity.GetDefinition(Schema schema)
        {
            return new Definition().AddComponentTypes<IsTextField, IsSelectable>(schema);
        }

        public unsafe TextField(Canvas canvas, FixedString defaultValue = default, BeginEditing beginEditing = default, TextValidation validation = default, Submit submit = default, Cancel cancel = default)
        {
            background = new(canvas);
            background.AddComponent(new IsSelectable());

            Label text = new(canvas, defaultValue);
            text.SetParent(background);
            text.Anchor = Anchor.TopLeft;
            text.Color = Color.Black;
            text.Position = new(4f, -4f);
            text.Pivot = new(0f, 1f, 0f);
            text.AddComponent(new RendererScissor());

            Image cursor = new(canvas);
            cursor.SetParent(background);
            cursor.Anchor = Anchor.TopLeft;
            cursor.Color = Color.Black;
            cursor.Size = new(2f, 16f);
            cursor.Position = new(16f, 0f);
            cursor.AddComponent(new RendererScissor());

            Image highlight = new(canvas);
            highlight.SetParent(background);
            highlight.Color = new Color(0f, 0.4f, 1f, 0.3f);
            highlight.Anchor = Anchor.TopLeft;
            highlight.Position = new(4f, -4f);
            highlight.Size = new(1, 1);
            highlight.Z = Settings.ZScale * 1.5f;
            highlight.SetEnabled(false);
            highlight.AddComponent(new RendererScissor());

            MeshRenderer highlightRenderer = highlight;
            highlightRenderer.Mesh = highlightRenderer.Mesh.Clone();
            highlightRenderer.Mesh.CreateColors(0);

            rint textReference = background.AddReference(text);
            rint cursorReference = background.AddReference(cursor);
            rint highlightReference = background.AddReference(highlight);
            background.AddComponent(new IsTextField(textReference, cursorReference, highlightReference, beginEditing, validation, submit, cancel));
        }

        public readonly void Dispose()
        {
            background.Dispose();
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
            USpan<char> buffer = stackalloc char[(int)newText.Length];
            uint length = newText.CopyTo(buffer);
            SetText(buffer.Slice(0, length));
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