using Cameras;
using Rendering;
using System.Numerics;
using Transforms;
using UI.Components;
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
        public readonly ref Vector2 Size => ref As<UITransform>().Size;

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

        public Canvas(Settings settings, Camera camera, LayerMask renderMask, LayerMask selectionMask)
        {
            world = settings.world;
            Transform transform = new(world);
            value = transform.value;

            rint cameraReference = transform.AddReference(camera);
            rint settingsReference = transform.AddReference(settings);
            transform.AddComponent(new IsCanvas(cameraReference, settingsReference, renderMask, selectionMask));
        }

        public Canvas(Settings settings, Camera camera)
        {
            world = settings.world;
            Transform transform = new(world);
            rint cameraReference = transform.AddReference(camera);
            rint settingsReference = transform.AddReference(settings);
            transform.AddComponent(new IsCanvas(cameraReference, settingsReference, LayerMask.All, LayerMask.All));
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