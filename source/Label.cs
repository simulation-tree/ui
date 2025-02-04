using Cameras;
using Fonts;
using Rendering;
using Rendering.Components;
using System;
using System.Collections.Generic;
using System.Numerics;
using Transforms;
using Transforms.Components;
using UI.Components;
using Unmanaged;
using Worlds;

namespace UI
{
    public readonly partial struct Label : ISelectable
    {
        public const float DefaultLabelSize = 16f;

        public unsafe readonly ref Vector2 Position
        {
            get
            {
                Transform transform = As<Transform>();
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
                Transform transform = As<Transform>();
                ref Vector3 localPosition = ref transform.LocalPosition;
                return ref localPosition.Z;
            }
        }

        /// <summary>
        /// Size of the label.
        /// </summary>
        public unsafe readonly ref Vector2 Size
        {
            get
            {
                Transform transform = As<Transform>();
                ref Vector3 localPosition = ref transform.LocalScale;
                fixed (Vector3* scale = &localPosition)
                {
                    return ref *(Vector2*)scale;
                }
            }
        }

        public readonly ref Anchor Anchor => ref GetComponent<Anchor>();
        public readonly ref Vector3 Pivot => ref GetComponent<Pivot>().value;
        public readonly ref Vector4 Color => ref GetComponent<BaseColor>().value;

        public readonly USpan<char> Text
        {
            get
            {
                TextMesh textMesh = As<TextRenderer>().TextMesh;
                return textMesh.Text;
            }
        }

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
            textRenderer.AddComponent(new IsLabel());
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
            TextRenderer textRenderer = transform.AsEntity().Become<TextRenderer>();
            textRenderer.TextMesh = textMesh;
            textRenderer.Material = settings.GetTextMaterial(camera);
            textRenderer.RenderMask = canvas.RenderMask;

            textRenderer.AddComponent(new IsLabel());

            USpan<char> textSpan = stackalloc char[text.Length];
            uint length = text.CopyTo(textSpan);
            textRenderer.AsEntity().CreateArray(textSpan.As<LabelCharacter>());

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
            archetype.AddComponentType<IsLabel>();
            archetype.AddComponentType<IsTextRenderer>();
            archetype.AddArrayType<LabelCharacter>();
        }

        public readonly void SetText(USpan<char> text)
        {
            USpan<LabelCharacter> array = ResizeArray<LabelCharacter>(text.Length);
            text.CopyTo(array.As<char>());
        }

        public readonly void SetText(string text)
        {
            SetText(text.AsSpan());
        }

        public readonly void SetText(FixedString text)
        {
            USpan<char> buffer = stackalloc char[text.Length];
            uint length = text.CopyTo(buffer);
            SetText(buffer.Slice(0, length));
        }

        public static implicit operator TextRenderer(Label label)
        {
            return label.As<TextRenderer>();
        }

        public static implicit operator Transform(Label label)
        {
            return label.As<Transform>();
        }
    }
}