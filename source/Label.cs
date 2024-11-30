using Cameras;
using Data;
using Fonts;
using InteractionKit.Components;
using Rendering;
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
                Vector3 position = transform.LocalPosition;
                return new(position.X, position.Y);
            }
            set
            {
                Transform transform = textRenderer.AsEntity().As<Transform>();
                Vector3 position = transform.LocalPosition;
                transform.LocalPosition = new(value.X, value.Y, position.Z);
            }
        }

        public readonly Vector2 Size
        {
            get
            {
                Transform transform = textRenderer.AsEntity().As<Transform>();
                Vector3 scale = transform.LocalScale;
                return new(scale.X, scale.Y);
            }
            set
            {
                Transform transform = textRenderer.AsEntity().As<Transform>();
                Vector3 scale = transform.LocalScale;
                transform.LocalScale = new(value.X, value.Y, scale.Z);
            }
        }

        public readonly ref Anchor Anchor => ref textRenderer.AsEntity().GetComponentRef<Anchor>();
        public readonly ref Vector3 Pivot => ref textRenderer.AsEntity().GetComponentRef<Pivot>().value;
        public readonly ref Color Color => ref textRenderer.AsEntity().GetComponentRef<BaseColor>().value;

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
        readonly Definition IEntity.Definition => new([ComponentType.Get<TextRenderer>(), ComponentType.Get<IsSelectable>()], []);

        public Label(World world, uint existingEntity)
        {
            textRenderer = new(world, existingEntity);
        }

        public Label(World world, Canvas canvas, FixedString text, Font font = default)
        {
            Settings settings = world.GetFirst<Settings>();
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
            transform.AddComponent(ComponentMix.Create<ColorTint, BaseColor, Color>(ComponentMix.Operation.FloatingMultiply, 4));
            transform.SetParent(canvas);

            Camera camera = canvas.Camera;
            textRenderer = transform.AsEntity().Become<TextRenderer>();
            textRenderer.TextMesh = textMesh;
            textRenderer.Material = settings.GetTextMaterial(camera);
            textRenderer.Mask = 1; //todo: customizing the layer that ui controls are on

            textRenderer.AddComponent(new IsLabel());

            transform.LocalScale = Vector3.One * 16f;
            transform.LocalPosition = new(0f, 0f, 0.1f);
        }

        public readonly void Dispose()
        {
            textRenderer.Dispose();
        }

        public readonly void SetText(USpan<char> text)
        {
            TextMesh textMesh = textRenderer.TextMesh;
            textMesh.SetText(text);
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
    }
}