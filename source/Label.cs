using Data;
using Fonts;
using InteractionKit.Components;
using Rendering;
using Simulation;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct Label : ISelectable
    {
        public readonly TextRenderer textRenderer;

        public readonly Entity Parent
        {
            get => textRenderer.Parent;
            set => textRenderer.Parent = value;
        }

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

        public readonly ref Anchor Anchor => ref textRenderer.entity.GetComponentRef<Anchor>();
        public readonly ref Vector3 Pivot => ref textRenderer.entity.GetComponentRef<Pivot>().value;
        public readonly ref Color Color => ref textRenderer.entity.GetComponentRef<BaseColor>().value;

        public readonly USpan<char> Text
        {
            get
            {
                rint textMeshReference = textRenderer.AsEntity().GetComponent<IsLabel>().textMeshReference;
                uint textMeshEntity = textRenderer.AsEntity().GetReference(textMeshReference);
                TextMesh textMesh = new(textRenderer.GetWorld(), textMeshEntity);
                return textMesh.Text;
            }
        }

        readonly uint IEntity.Value => textRenderer.GetEntityValue();
        readonly World IEntity.World => textRenderer.GetWorld();
        readonly Definition IEntity.Definition => new([RuntimeType.Get<TextRenderer>(), RuntimeType.Get<IsSelectable>()], []);

        public Label(World world, uint existingEntity)
        {
            textRenderer = new(world, existingEntity);
        }

        public Label(World world, InteractiveContext context, FixedString text)
            : this(world, context, text, context.defaultFont)
        {
        }

        public Label(World world, InteractiveContext context, FixedString text, Font font)
        {
            TextMesh textMesh = new(world, text, font);

            Transform transform = new(world);
            transform.entity.AddComponent(new Anchor());
            transform.entity.AddComponent(new Pivot());
            transform.entity.AddComponent(new ColorTint(new Vector4(1f)));
            transform.entity.AddComponent(new BaseColor(new Vector4(1f)));
            transform.entity.AddComponent(new Color(new Vector4(1f)));
            transform.entity.AddComponent(ComponentMix.Create<ColorTint, BaseColor, Color>(ComponentMix.Operation.FloatingMultiply, 4));
            transform.Parent = context.canvas.AsEntity();

            textRenderer = transform.entity.Become<TextRenderer>();
            textRenderer.TextMesh = textMesh;
            textRenderer.Material = context.textMaterial;
            textRenderer.Camera = context.camera;

            rint textMeshReference = textRenderer.AsEntity().AddReference(textMesh);
            textRenderer.AsEntity().AddComponent(new IsLabel(textMeshReference));

            transform.LocalScale = Vector3.One * 16f;
            transform.LocalPosition = new(0f, 0f, 0.1f);
        }

        public readonly void SetText(USpan<char> text)
        {
            rint textMeshReference = textRenderer.AsEntity().GetComponent<IsLabel>().textMeshReference;
            uint textMeshEntity = textRenderer.AsEntity().GetReference(textMeshReference);
            TextMesh textMesh = new(textRenderer.GetWorld(), textMeshEntity);
            textMesh.SetText(text);
        }

        public readonly void SetText(FixedString text)
        {
            USpan<char> buffer = stackalloc char[(int)text.Length];
            uint length = text.CopyTo(buffer);
            SetText(buffer.Slice(0, length));
        }
    }
}