using InteractionKit.Components;
using InteractionKit.Functions;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Worlds;

namespace InteractionKit
{
    public readonly struct Toggle : ISelectable
    {
        private readonly Image background;

        public readonly ref Vector2 Position => ref background.Position;
        public readonly ref Vector2 Size => ref background.Size;
        public readonly ref float Z => ref background.Z;
        public readonly ref Anchor Anchor => ref background.AsEntity().GetComponent<Anchor>();
        public readonly ref Vector3 Pivot => ref background.AsEntity().GetComponent<Pivot>().value;
        public readonly ref Vector4 BackgroundColor => ref background.AsEntity().GetComponent<BaseColor>().value;
        public readonly ref Vector4 CheckmarkColor
        {
            get
            {
                rint checkmarkReference = background.AsEntity().GetComponent<IsToggle>().checkmarkReference;
                uint checkmarkEntity = background.GetReference(checkmarkReference);
                return ref background.GetWorld().GetComponent<BaseColor>(checkmarkEntity).value;
            }
        }

        public readonly bool Value
        {
            get => background.AsEntity().GetComponent<IsToggle>().value;
            set
            {
                ref IsToggle toggle = ref background.AsEntity().GetComponent<IsToggle>();
                toggle.value = value;

                rint checkmarkReference = toggle.checkmarkReference;
                uint checkmarkEntity = background.GetReference(checkmarkReference);
                background.GetWorld().SetEnabled(checkmarkEntity, value);
            }
        }

        public readonly ref ToggleCallback Callback => ref background.AsEntity().GetComponent<IsToggle>().callback;

        readonly uint IEntity.Value => background.GetEntityValue();
        readonly World IEntity.World => background.GetWorld();

        readonly Definition IEntity.GetDefinition(Schema schema)
        {
            return new Definition().AddComponentTypes<IsToggle, IsSelectable>(schema);
        }

        public Toggle(World world, uint existingEntity)
        {
            background = new(world, existingEntity);
        }

        public Toggle(Canvas canvas, bool initialValue = false)
        {
            background = new(canvas);

            Image checkmarkBox = new(canvas);
            checkmarkBox.SetParent(background);
            checkmarkBox.Anchor = new("4", "4", "0", "4", "4", "0");
            checkmarkBox.Color = new(0, 0, 0, 1);

            Transform checkmarkTransform = checkmarkBox;
            checkmarkTransform.LocalPosition = new(0f, 0f, Settings.ZScale);

            rint checkmarkReference = background.AddReference(checkmarkBox);
            background.AddComponent(new IsToggle(checkmarkReference, initialValue, default));
            background.AddComponent(new IsSelectable());
            checkmarkBox.SetEnabled(initialValue);
        }

        public readonly void Dispose()
        {
            background.Dispose();
        }

        public static implicit operator Entity(Toggle toggle)
        {
            return toggle.AsEntity();
        }

        public static implicit operator Image(Toggle toggle)
        {
            return toggle.background;
        }

        public static implicit operator Transform(Toggle toggle)
        {
            return toggle.background;
        }
    }
}