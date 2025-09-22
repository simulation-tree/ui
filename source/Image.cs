using Automations;
using Data;
using Materials;
using Materials.Arrays;
using Rendering;
using Rendering.Components;
using System.Numerics;
using Textures;
using Transforms;
using Transforms.Components;
using UI.Components;
using Worlds;

namespace UI
{
    public readonly partial struct Image : IEntity
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
        public readonly ref Color Color => ref GetComponent<BaseColor>().value;

        public readonly Material Material
        {
            get => As<MeshRenderer>().Material;
            set => As<MeshRenderer>().Material = value;
        }

        public readonly Texture Texture
        {
            get
            {
                DescriptorResourceKey key = new(1, 0);
                TextureBinding binding = Material.GetTextureBinding(key);
                uint textureEntity = binding.Entity;
                return new Entity(world, textureEntity).As<Texture>();
            }
            set
            {
                DescriptorResourceKey key = new(1, 0);
                ref TextureBinding binding = ref Material.GetTextureBinding(key);
                binding.SetTexture(value);
            }
        }

        public Image(Canvas canvas)
        {
            world = canvas.world;
            Settings settings = canvas.Settings;
            Schema schema = world.Schema;

            Transform transform = new(world);
            transform.LocalPosition = new(0f, 0f, Settings.ZScale);
            transform.AddComponent(new Anchor());
            transform.AddComponent(new Pivot());
            transform.AddComponent(new ColorTint(new Color(1f)));
            transform.AddComponent(new BaseColor(new Color(1f)));
            transform.AddComponent(new Color(1f));
            transform.AddComponent(ComponentMix.Create<ColorTint, BaseColor, Color>(ComponentMix.Operation.FloatingMultiply, 4, schema));
            transform.SetParent(canvas);
            value = transform.value;

            StatefulAutomationPlayer stateful = transform.Become<StatefulAutomationPlayer>();
            stateful.StateMachine = settings.ControlStateMachine;
            stateful.AddParameter("pressed", 0f);
            stateful.AddParameter("selected", 1f);
            stateful.AddOrSetLinkToComponent<ColorTint>("idle", settings.IdleAutomation);
            stateful.AddOrSetLinkToComponent<ColorTint>("selected", settings.SelectedAutomation);
            stateful.AddOrSetLinkToComponent<ColorTint>("pressed", settings.PressedAutomation);

            MeshRenderer renderer = transform.Become<MeshRenderer>();
            renderer.Mesh = settings.QuadMesh;
            renderer.Material = settings.GetSquareMaterial(canvas.Camera);
            renderer.RenderMask = canvas.RenderMask;
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsRenderer>();
            archetype.AddTagType<IsTransform>();
        }

        public static implicit operator Transform(Image box)
        {
            return box.As<Transform>();
        }

        public static implicit operator UITransform(Image box)
        {
            return box.As<UITransform>();
        }

        public static implicit operator MeshRenderer(Image box)
        {
            return box.As<MeshRenderer>();
        }
    }
}