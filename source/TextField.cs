using Data;
using InteractionKit.Components;
using InteractionKit.Functions;
using Rendering;
using Rendering.Components;
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

        public unsafe TextField(Canvas canvas, FixedString defaultValue = default)
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
            highlight.Color = new Color(0f, 0.4f, 1f, 0.5f);
            highlight.Anchor = Anchor.TopLeft;
            highlight.Position = new(4f, -4f);
            highlight.Size = new(1, 1);
            highlight.SetEnabled(false);
            highlight.AddComponent(new RendererScissor());

            MeshRenderer highlightRenderer = highlight;
            highlightRenderer.Mesh = highlightRenderer.Mesh.Clone();
            highlightRenderer.Mesh.CreateColors(0);

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