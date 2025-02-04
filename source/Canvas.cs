using Cameras;
using UI.Components;
using Rendering;
using System.Numerics;
using Transforms;
using Worlds;

namespace UI
{
    public readonly partial struct Canvas : IEntity
    {
        /// <summary>
        /// The camera that all elements in the canvas will be rendered to.
        /// </summary>
        public readonly Camera Camera
        {
            get
            {
                ref IsCanvas component = ref GetComponent<IsCanvas>();
                uint cameraEntity = GetReference(component.cameraReference);
                return new Entity(world, cameraEntity).As<Camera>();
            }
            set
            {
                ref IsCanvas component = ref GetComponent<IsCanvas>();
                if (component.cameraReference == default)
                {
                    component.cameraReference = AddReference(value);
                }
                else
                {
                    SetReference(component.cameraReference, value);
                }
            }
        }

        /// <summary>
        /// Size of the destination that this camera is rendering to.
        /// </summary>
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

        /// <summary>
        /// The settings that this canvas will use.
        /// </summary>
        public readonly Settings Settings
        {
            get
            {
                ref IsCanvas component = ref GetComponent<IsCanvas>();
                uint settingsEntity = GetReference(component.settingsReference);
                return new Entity(world, settingsEntity).As<Settings>();
            }
        }

        public readonly ref LayerMask RenderMask => ref GetComponent<IsCanvas>().renderMask;
        public readonly ref LayerMask SelectionMask => ref GetComponent<IsCanvas>().selectionMask;

        public Canvas(World world, Settings settings, Camera camera, LayerMask renderMask, LayerMask selectionMask)
        {
            this.world = world;
            Transform transform = new(world);
            value = transform.value;

            rint cameraReference = transform.AddReference(camera);
            rint settingsReference = transform.AddReference(settings);
            transform.AddComponent(new IsCanvas(cameraReference, settingsReference, renderMask, selectionMask));
        }

        public Canvas(World world, Settings settings, Camera camera)
        {
            Transform transform = new(world);
            rint cameraReference = transform.AddReference(camera);
            rint settingsReference = transform.AddReference(settings);
            transform.AddComponent(new IsCanvas(cameraReference, settingsReference, LayerMask.All, LayerMask.All));
            this.world = world;
            value = transform.value;
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsCanvas>();
        }

        public static implicit operator Transform(Canvas canvas)
        {
            return canvas.As<Transform>();
        }
    }
}