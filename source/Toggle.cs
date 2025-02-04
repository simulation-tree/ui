using UI.Components;
using UI.Functions;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Worlds;

namespace UI
{
    public readonly partial struct Toggle : ISelectable
    {
        public readonly ref Vector2 Position => ref As<Image>().Position;
        public readonly ref Vector2 Size => ref As<Image>().Size;
        public readonly ref float Z => ref As<Image>().Z;
        public readonly ref Anchor Anchor => ref GetComponent<Anchor>();
        public readonly ref Vector3 Pivot => ref GetComponent<Pivot>().value;
        public readonly ref Vector4 BackgroundColor => ref GetComponent<BaseColor>().value;

        public readonly ref Vector4 CheckmarkColor
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

            Transform checkmarkTransform = checkmarkBox;
            checkmarkTransform.LocalPosition = new(0f, 0f, Settings.ZScale);

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

        public static implicit operator Transform(Toggle toggle)
        {
            return toggle.As<Transform>();
        }
    }
}