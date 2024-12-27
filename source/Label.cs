using Cameras;
using Data;
using Fonts;
using InteractionKit.Components;
using Rendering;
using Rendering.Components;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Unmanaged;
using Worlds;

namespace InteractionKit
{
    public readonly struct Label : ISelectable
    {
        private readonly TextRenderer textRenderer;

        public readonly Vector2 Position
        {
            get
            {
                Transform transform = textRenderer.AsEntity().As<Transform>();
                ref Vector3 position = ref transform.LocalPosition;
                return new(position.X, position.Y);
            }
            set
            {
                Transform transform = textRenderer.AsEntity().As<Transform>();
                ref Vector3 position = ref transform.LocalPosition;
                position.X = value.X;
                position.Y = value.Y;
            }
        }

        public readonly float Z
        {
            get
            {
                return textRenderer.AsEntity().As<Transform>().LocalPosition.Z;
            }
            set
            {
                Transform transform = textRenderer.AsEntity().As<Transform>();
                ref Vector3 position = ref transform.LocalPosition;
                position.Z = value;
            }
        }

        /// <summary>
        /// Size of the label.
        /// </summary>
        public readonly Vector2 Size
        {
            get
            {
                Transform transform = textRenderer.AsEntity().As<Transform>();
                ref Vector3 scale = ref transform.LocalScale;
                return new(scale.X, scale.Y);
            }
            set
            {
                Transform transform = textRenderer.AsEntity().As<Transform>();
                ref Vector3 scale = ref transform.LocalScale;
                scale.X = value.X;
                scale.Y = value.Y;
            }
        }

        public readonly ref Anchor Anchor => ref textRenderer.AsEntity().GetComponent<Anchor>();
        public readonly ref Vector3 Pivot => ref textRenderer.AsEntity().GetComponent<Pivot>().value;
        public readonly ref Color Color => ref textRenderer.AsEntity().GetComponent<BaseColor>().value;

        public readonly USpan<char> Text
        {
            get
            {
                TextMesh textMesh = textRenderer.TextMesh;
                return textMesh.Text;
            }
        }

        public readonly Font Font
        {
            get
            {
                TextMesh textMesh = textRenderer.TextMesh;
                return textMesh.Font;
            }
            set
            {
                TextMesh textMesh = textRenderer.TextMesh;
                textMesh.Font = value;
            }
        }

        readonly uint IEntity.Value => textRenderer.GetEntityValue();
        readonly World IEntity.World => textRenderer.GetWorld();

        readonly Definition IEntity.GetDefinition(Schema schema)
        {
            return new Definition().AddComponentTypes<IsLabel, IsTextRenderer, IsSelectable>(schema).AddArrayType<LabelCharacter>(schema);
        }

        public Label(World world, uint existingEntity)
        {
            textRenderer = new(world, existingEntity);
        }

        public Label(Canvas canvas, USpan<char> text, Font font = default, float size = 16f)
        {
            World world = canvas.GetWorld();
            Settings settings = canvas.GetSettings();
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
            textRenderer = transform.AsEntity().Become<TextRenderer>();
            textRenderer.TextMesh = textMesh;
            textRenderer.Material = settings.GetTextMaterial(camera);
            textRenderer.Mask = settings.Mask;

            textRenderer.AddComponent(new IsLabel());

            textRenderer.AsEntity().CreateArray(text.As<LabelCharacter>());

            transform.LocalScale = Vector3.One * size;
            transform.LocalPosition = new(0f, 0f, Settings.ZScale);
        }

        public Label(Canvas canvas, FixedString text, Font font = default, float size = 16f)
        {
            World world = canvas.GetWorld();
            Settings settings = canvas.GetSettings();
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
            textRenderer = transform.AsEntity().Become<TextRenderer>();
            textRenderer.TextMesh = textMesh;
            textRenderer.Material = settings.GetTextMaterial(camera);
            textRenderer.Mask = settings.Mask;

            textRenderer.AddComponent(new IsLabel());

            USpan<char> textSpan = stackalloc char[(int)text.Length];
            uint length = text.CopyTo(textSpan);
            textRenderer.AsEntity().CreateArray(textSpan.As<LabelCharacter>());

            transform.LocalScale = Vector3.One * size;
            transform.LocalPosition = new(0f, 0f, Settings.ZScale);
        }

        public readonly void Dispose()
        {
            textRenderer.Dispose();
        }

        public readonly void SetText(USpan<char> text)
        {
            USpan<LabelCharacter> array = textRenderer.AsEntity().ResizeArray<LabelCharacter>(text.Length);
            text.CopyTo(array.As<char>());
        }

        public readonly void SetText(string text)
        {
            SetText(text.AsUSpan());
        }

        public readonly void SetText(FixedString text)
        {
            USpan<char> buffer = stackalloc char[(int)text.Length];
            uint length = text.CopyTo(buffer);
            SetText(buffer.Slice(0, length));
        }

        public static implicit operator Entity(Label label)
        {
            return label.textRenderer.AsEntity();
        }

        public static implicit operator TextRenderer(Label label)
        {
            return label.textRenderer;
        }

        public static implicit operator Transform(Label label)
        {
            return label.textRenderer.AsEntity().As<Transform>();
        }
    }
}