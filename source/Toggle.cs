using Data;
using InteractionKit.Components;
using Simulation;
using System.Numerics;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct Toggle : ISelectable
    {
        public readonly Box box;

        public readonly Entity Parent
        {
            get => box.Parent;
            set => box.Parent = value;
        }

        public readonly Vector2 Position
        {
            get
            {
                Vector3 position = box.transform.LocalPosition;
                return new(position.X, position.Y);
            }
            set
            {
                Vector3 position = box.transform.LocalPosition;
                box.transform.LocalPosition = new(value.X, value.Y, position.Z);
            }
        }

        public readonly Vector2 Size
        {
            get
            {
                Vector3 scale = box.transform.LocalScale;
                return new(scale.X, scale.Y);
            }
            set
            {
                Vector3 scale = box.transform.LocalScale;
                box.transform.LocalScale = new(value.X, value.Y, scale.Z);
            }
        }

        public readonly ref Anchor Anchor => ref box.AsEntity().GetComponentRef<Anchor>();
        public readonly ref Vector3 Pivot => ref box.AsEntity().GetComponentRef<Pivot>().value;
        public readonly ref Color BackgroundColor => ref box.AsEntity().GetComponentRef<BaseColor>().value;
        public readonly ref Color CheckmarkColor
        {
            get
            {
                rint checkmarkReference = box.AsEntity().GetComponent<IsToggle>().checkmarkReference;
                uint checkmarkEntity = box.AsEntity().GetReference(checkmarkReference);
                return ref box.GetWorld().GetComponentRef<BaseColor>(checkmarkEntity).value;
            }
        }

        public readonly bool Value
        {
            get => box.AsEntity().GetComponentRef<IsToggle>().value;
            set
            {
                ref IsToggle toggle = ref box.AsEntity().GetComponentRef<IsToggle>();
                toggle.value = value;

                rint checkmarkReference = toggle.checkmarkReference;
                uint checkmarkEntity = box.AsEntity().GetReference(checkmarkReference);
                box.GetWorld().SetEnabled(checkmarkEntity, value);  
            }
        }

        readonly uint IEntity.Value => box.GetEntityValue();
        readonly World IEntity.World => box.GetWorld();
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsToggle>(), RuntimeType.Get<IsSelectable>()], []);

        public Toggle(World world, uint existingEntity)
        {
            box = new(world, existingEntity);
        }

        public Toggle(World world, InteractiveContext context, bool initialValue = false)
        {
            box = new(world, context);

            Box checkmarkBox = new(world, context);
            checkmarkBox.transform.LocalPosition = new(0f, 0f, 0.1f);
            checkmarkBox.Parent = box.AsEntity();
            checkmarkBox.Anchor = new(new(4, true), new(4, true), default, new(4, true), new(4, true), default);
            checkmarkBox.Color = Color.Black;

            rint checkmarkReference = box.AsEntity().AddReference(checkmarkBox);
            box.AsEntity().AddComponent(new IsToggle(checkmarkReference, initialValue));
            box.AsEntity().AddComponent(new IsSelectable());
            checkmarkBox.SetEnabled(initialValue);
        }
    }
}