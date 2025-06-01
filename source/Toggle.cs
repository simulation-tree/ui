using Data;
using System.Numerics;
using Transforms;
using Transforms.Components;
using UI.Components;
using UI.Functions;
using Worlds;

namespace UI
{
    public readonly partial struct Toggle : ISelectable
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
        public readonly ref Color BackgroundColor => ref GetComponent<BaseColor>().value;

        public readonly ref Color CheckmarkColor
        {
            get
            {
                rint checkmarkReference = GetComponent<IsToggle>().checkmarkReference;
                uint checkmarkEntity = GetReference(checkmarkReference);
                return ref world.GetComponent<BaseColor>(checkmarkEntity).value;
            }
        }

        public readonly bool Value
        {
            get => GetComponent<IsToggle>().value;
            set
            {
                ref IsToggle toggle = ref GetComponent<IsToggle>();
                toggle.value = value;

                rint checkmarkReference = toggle.checkmarkReference;
                uint checkmarkEntity = GetReference(checkmarkReference);
                world.SetEnabled(checkmarkEntity, value);
            }
        }

        public readonly ref ToggleCallback Callback => ref GetComponent<IsToggle>().callback;

        public Toggle(Canvas canvas, bool initialValue = false)
        {
            world = canvas.world;
            Image background = new(canvas);
            value = background.value;

            Image checkmarkBox = new(canvas);
            checkmarkBox.SetParent(background);
            checkmarkBox.Anchor = new("4", "4", "0", "4", "4", "0");
            checkmarkBox.Color = new(0, 0, 0, 1);

            UITransform checkmarkTransform = checkmarkBox;
            checkmarkTransform.Z = Settings.ZScale;

            rint checkmarkReference = background.AddReference(checkmarkBox);
            background.AddComponent(new IsToggle(checkmarkReference, initialValue, default));
            background.AddComponent(new IsSelectable(canvas.SelectionMask));
            checkmarkBox.IsEnabled = initialValue;
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsToggle>();
            archetype.AddComponentType<IsSelectable>();
        }

        public static implicit operator Image(Toggle toggle)
        {
            return toggle.As<Image>();
        }

        public static implicit operator UITransform(Toggle toggle)
        {
            return toggle.As<UITransform>();
        }

        public static implicit operator Transform(Toggle toggle)
        {
            return toggle.As<Transform>();
        }
    }
}