using Automations;
using Materials;
using Materials.Components;
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
        public unsafe readonly ref Vector2 Position
        {
            get
            {
                ref Vector3 localPosition = ref As<Transform>().LocalPosition;
                fixed (Vector3* positionPointer = &localPosition)
                {
                    return ref *(Vector2*)positionPointer;
                }
            }
        }

        public readonly ref float Z => ref As<Transform>().LocalPosition.Z;

        public unsafe readonly ref Vector2 Size
        {
            get
            {
                ref Vector3 localScale = ref As<Transform>().LocalScale;
                fixed (Vector3* sizePointer = &localScale)
                {
                    return ref *(Vector2*)sizePointer;
                }
            }
        }

        public readonly float Rotation
        {
            get
            {
                Quaternion rotation = As<Transform>().LocalRotation;
                return new EulerAngles(rotation).value.Z;
            }
            set
            {
                ref Quaternion rotation = ref As<Transform>().LocalRotation;
                rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, value);
            }
        }

        public readonly ref Anchor Anchor => ref GetComponent<Anchor>();
        public readonly ref Vector3 Pivot => ref GetComponent<Pivot>().value;
        public readonly ref Vector4 Color => ref GetComponent<BaseColor>().value;

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
            transform.AddComponent(new ColorTint(new Vector4(1f)));
            transform.AddComponent(new BaseColor(new Vector4(1f)));
            transform.AddComponent(new Color(new Vector4(1f)));
            transform.AddComponent(ComponentMix.Create<ColorTint, BaseColor, Color>(ComponentMix.Operation.FloatingMultiply, 4, schema));
            transform.SetParent(canvas);
            value = transform.value;

            StatefulAutomationPlayer stateful = transform.AsEntity().Become<StatefulAutomationPlayer>();
            stateful.StateMachine = settings.ControlStateMachine;
            stateful.AddParameter("pressed", 0f);
            stateful.AddParameter("selected", 1f);
            stateful.AddOrSetLink<ColorTint>("idle", settings.IdleAutomation);
            stateful.AddOrSetLink<ColorTint>("selected", settings.SelectedAutomation);
            stateful.AddOrSetLink<ColorTint>("pressed", settings.PressedAutomation);

            MeshRenderer renderer = transform.AsEntity().Become<MeshRenderer>();
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

        public static implicit operator MeshRenderer(Image box)
        {
            return box.As<MeshRenderer>();
        }
    }
}