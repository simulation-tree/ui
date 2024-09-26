using Data;
using InteractionKit.Components;
using InteractionKit.Functions;
using Simulation;
using System.Numerics;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct Toggle : ISelectable
    {
        public readonly Image background;

        public readonly Entity Parent
        {
            get => background.Parent;
            set => background.Parent = value;
        }

        public readonly Vector2 Position
        {
            get
            {
                Vector3 position = background.transform.LocalPosition;
                return new(position.X, position.Y);
            }
            set
            {
                Vector3 position = background.transform.LocalPosition;
                background.transform.LocalPosition = new(value.X, value.Y, position.Z);
            }
        }

        public readonly Vector2 Size
        {
            get
            {
                Vector3 scale = background.transform.LocalScale;
                return new(scale.X, scale.Y);
            }
            set
            {
                Vector3 scale = background.transform.LocalScale;
                background.transform.LocalScale = new(value.X, value.Y, scale.Z);
            }
        }

        public readonly ref Anchor Anchor => ref background.AsEntity().GetComponentRef<Anchor>();
        public readonly ref Vector3 Pivot => ref background.AsEntity().GetComponentRef<Pivot>().value;
        public readonly ref Color BackgroundColor => ref background.AsEntity().GetComponentRef<BaseColor>().value;
        public readonly ref Color CheckmarkColor
        {
            get
            {
                rint checkmarkReference = background.AsEntity().GetComponent<IsToggle>().checkmarkReference;
                uint checkmarkEntity = background.GetReference(checkmarkReference);
                return ref background.GetWorld().GetComponentRef<BaseColor>(checkmarkEntity).value;
            }
        }

        public readonly bool Value
        {
            get => background.AsEntity().GetComponentRef<IsToggle>().value;
            set
            {
                ref IsToggle toggle = ref background.AsEntity().GetComponentRef<IsToggle>();
                toggle.value = value;

                rint checkmarkReference = toggle.checkmarkReference;
                uint checkmarkEntity = background.GetReference(checkmarkReference);
                background.GetWorld().SetEnabled(checkmarkEntity, value);
            }
        }

        public readonly ref ToggleCallbackFunction Callback => ref background.AsEntity().GetComponentRef<IsToggle>().callback;

        readonly uint IEntity.Value => background.GetEntityValue();
        readonly World IEntity.World => background.GetWorld();
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsToggle>(), RuntimeType.Get<IsSelectable>()], []);

        public Toggle(World world, uint existingEntity)
        {
            background = new(world, existingEntity);
        }

        public Toggle(World world, Canvas canvas, bool initialValue = false)
        {
            background = new(world, canvas);

            Image checkmarkBox = new(world, canvas);
            checkmarkBox.transform.LocalPosition = new(0f, 0f, 0.1f);
            checkmarkBox.Parent = background;
            checkmarkBox.Anchor = new(new(4, true), new(4, true), default, new(4, true), new(4, true), default);
            checkmarkBox.Color = Color.Black;

            rint checkmarkReference = background.AddReference(checkmarkBox);
            background.AddComponent(new IsToggle(checkmarkReference, initialValue, default));
            background.AddComponent(new IsSelectable());
            checkmarkBox.SetEnabled(initialValue);
        }
    }
}