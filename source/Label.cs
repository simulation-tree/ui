using Cameras;
using Fonts;
using Rendering;
using Rendering.Components;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Transforms;
using Transforms.Components;
using UI.Components;
using Unmanaged;
using Worlds;

namespace UI
{
    [SkipLocalsInit]
    public readonly partial struct Label : ISelectable
    {
        public const float DefaultLabelSize = 16f;

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
        public readonly ref Vector4 Color => ref GetComponent<BaseColor>().value;

        /// <summary>
        /// Displayed text of the label after being processed.
        /// </summary>
        public readonly USpan<char> ProcessedText => As<TextRenderer>().TextMesh.Content;

        /// <summary>
        /// The original and underlying text of the label.
        /// </summary>
        public readonly USpan<char> UnderlyingText => GetArray<LabelCharacter>().AsSpan<char>();

        public readonly Font Font
        {
            get
            {
                TextMesh textMesh = As<TextRenderer>().TextMesh;
                return textMesh.Font;
            }
            set
            {
                TextMesh textMesh = As<TextRenderer>().TextMesh;
                textMesh.Font = value;
            }
        }

        public Label(Canvas canvas, USpan<char> text, Font font = default, float size = DefaultLabelSize)
        {
            world = canvas.world;
            Settings settings = canvas.Settings;
            Schema schema = world.Schema;
            if (font == default)
            {
                font = settings.Font;
            }

            TextMesh textMesh = new(world, default(FixedString), font);

            Transform transform = new(world);
            transform.AddComponent(new Anchor());
            transform.AddComponent(new Pivot());
            transform.AddComponent(new ColorTint(new Vector4(1f)));
            transform.AddComponent(new BaseColor(new Vector4(1f)));
            transform.AddComponent(new Color(new Vector4(1f)));
            transform.AddComponent(ComponentMix.Create<ColorTint, BaseColor, Color>(ComponentMix.Operation.FloatingMultiply, 4, schema));
            transform.SetParent(canvas);

            Camera camera = canvas.Camera;
            TextRenderer textRenderer = transform.Become<TextRenderer>();
            textRenderer.TextMesh = textMesh;
            textRenderer.Material = settings.GetTextMaterial(camera);
            textRenderer.RenderMask = canvas.RenderMask;
            textRenderer.AddTag<IsLabel>();
            textRenderer.CreateArray(text.As<LabelCharacter>());

            transform.LocalScale = Vector3.One * size;
            transform.LocalPosition = new(0f, 0f, Settings.ZScale);

            value = transform.value;
        }

        public Label(Canvas canvas, FixedString text, Font font = default, float size = DefaultLabelSize)
        {
            world = canvas.world;
            Settings settings = canvas.Settings;
            Schema schema = world.Schema;
            if (font == default)
            {
                font = settings.Font;
            }

            TextMesh textMesh = new(world, text, font);

            Transform transform = new(world);
            transform.AddComponent(new Anchor());
            transform.AddComponent(new Pivot());
            transform.AddComponent(new ColorTint(new Vector4(1f)));
            transform.AddComponent(new BaseColor(new Vector4(1f)));
            transform.AddComponent(new Color(new Vector4(1f)));
            transform.AddComponent(ComponentMix.Create<ColorTint, BaseColor, Color>(ComponentMix.Operation.FloatingMultiply, 4, schema));
            transform.SetParent(canvas);

            Camera camera = canvas.Camera;
            TextRenderer textRenderer = transform.Become<TextRenderer>();
            textRenderer.TextMesh = textMesh;
            textRenderer.Material = settings.GetTextMaterial(camera);
            textRenderer.RenderMask = canvas.RenderMask;

            textRenderer.AddTag<IsLabel>();

            USpan<char> textSpan = stackalloc char[text.Length];
            text.CopyTo(textSpan);
            textRenderer.CreateArray(textSpan.As<LabelCharacter>());

            transform.LocalScale = Vector3.One * size;
            transform.LocalPosition = new(0f, 0f, Settings.ZScale);

            value = transform.value;
        }

        public Label(Canvas canvas, IEnumerable<char> text, Font font = default, float size = DefaultLabelSize)
            : this(canvas, new FixedString(text), font, size)
        {
        }

        public Label(Canvas canvas, string text, Font font = default, float size = DefaultLabelSize)
            : this(canvas, new FixedString(text), font, size)
        {
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddTagType<IsLabel>();
            archetype.AddComponentType<IsTextRenderer>();
            archetype.AddArrayType<LabelCharacter>();
        }

        /// <summary>
        /// Updates the underlying text of the label.
        /// </summary>
        public readonly void SetText(USpan<char> text)
        {
            Values<LabelCharacter> array = GetArray<LabelCharacter>();
            array.Length = text.Length;
            text.CopyTo(array.AsSpan<char>());
        }

        /// <summary>
        /// Updates the underlying text of the label.
        /// </summary>
        public readonly void SetText(string text)
        {
            SetText(text.AsSpan());
        }

        /// <summary>
        /// Updates the underlying text of the label.
        /// </summary>
        public readonly void SetText(FixedString text)
        {
            USpan<char> buffer = stackalloc char[text.Length];
            text.CopyTo(buffer);
            SetText(buffer);
        }

        public static implicit operator TextRenderer(Label label)
        {
            return label.As<TextRenderer>();
        }

        public static implicit operator UITransform(Label label)
        {
            return label.As<UITransform>();
        }

        public static implicit operator Transform(Label label)
        {
            return label.As<Transform>();
        }
    }
}