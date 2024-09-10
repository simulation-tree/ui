using Automations;
using Data;
using InteractionKit.Components;
using Rendering;
using Rendering.Components;
using Simulation;
using System;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct Box : IEntity
    {
        public readonly Transform transform;

        public readonly Entity Parent
        {
            get => transform.Parent;
            set => transform.Parent = value;
        }

        public readonly Vector2 Position
        {
            get
            {
                Vector3 position = transform.LocalPosition;
                return new(position.X, position.Y);
            }
            set
            {
                Vector3 position = transform.LocalPosition;
                transform.LocalPosition = new(value.X, value.Y, position.Z);
            }
        }

        public readonly Vector2 Size
        {
            get
            {
                Vector3 scale = transform.LocalScale;
                return new(scale.X, scale.Y);
            }
            set
            {
                Vector3 scale = transform.LocalScale;
                transform.LocalScale = new(value.X, value.Y, scale.Z);
            }
        }

        public readonly ref Anchor Anchor => ref transform.entity.GetComponentRef<Anchor>();
        public readonly ref Vector3 Pivot => ref transform.entity.GetComponentRef<Pivot>().value;
        public readonly ref Color Color => ref transform.entity.GetComponentRef<BaseColor>().value;

        readonly uint IEntity.Value => transform.GetEntityValue();
        readonly World IEntity.World => transform.GetWorld();
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsTransform>(), RuntimeType.Get<IsRenderer>()], []);

#if NET
        [Obsolete("Default constructor not available", true)]
        public Box()
        {
            throw new NotSupportedException();
        }
#endif
        public Box(World world, uint existingEntity)
        {
            transform = new(world, existingEntity);
        }

        public Box(World world, InteractiveContext context)
        {
            transform = new(world);
            transform.entity.AddComponent(new Anchor());
            transform.entity.AddComponent(new Pivot());
            transform.entity.AddComponent(new ColorTint(new Vector4(1f)));
            transform.entity.AddComponent(new BaseColor(new Vector4(1f)));
            transform.entity.AddComponent(new Color(new Vector4(1f)));
            transform.entity.AddComponent(ComponentMix.Create<ColorTint, BaseColor, Color>(ComponentMix.Operation.FloatingMultiply, 4));
            transform.Parent = context.canvas.AsEntity();

            StatefulAutomationPlayer stateful = transform.entity.Become<StatefulAutomationPlayer>();
            stateful.StateMachine = context.controlStateMachine;
            stateful.AddParameter("pressed", 0f);
            stateful.AddParameter("selected", 1f);
            stateful.AddOrSetLink<ColorTint>("idle", context.idleAutomation);
            stateful.AddOrSetLink<ColorTint>("selected", context.selectedAutomation);
            stateful.AddOrSetLink<ColorTint>("pressed", context.pressedAutomation);

            Renderer renderer = transform.entity.Become<Renderer>();
            renderer.Mesh = context.quadMesh;
            renderer.Material = context.unlitMaterial;
            renderer.Camera = context.camera;
        }
    }
}