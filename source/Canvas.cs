using InteractionKit.Components;
using Rendering;
using Simulation;
using System;
using Transforms;

namespace InteractionKit
{
    public readonly struct Canvas : IEntity
    {
        public readonly Transform transform;

        /// <summary>
        /// The camera that all elements in the canvas will be rendered to.
        /// </summary>
        public readonly Camera Camera
        {
            get
            {
                ref IsCanvas component = ref transform.entity.GetComponentRef<IsCanvas>();
                return transform.entity.GetReference<Camera>(component.cameraReference);
            }
            set
            {
                ref IsCanvas component = ref transform.entity.GetComponentRef<IsCanvas>();
                if (component.cameraReference == default)
                {
                    component.cameraReference = transform.entity.AddReference(value);
                }
                else
                {
                    transform.entity.SetReference(component.cameraReference, value);
                }
            }
        }

        readonly uint IEntity.Value => transform.entity.value;
        readonly World IEntity.World => transform.entity.world;
        readonly Definition IEntity.Definition => new Definition().AddComponentType<IsCanvas>();

#if NET
        [Obsolete("Default constructor not available", true)]
        public Canvas()
        {
            throw new NotSupportedException();
        }
#endif

        public Canvas(World world, Camera camera)
        {
            Settings.ThrowIfMissing(world);
            transform = new(world);

            rint cameraReference = transform.AddReference(camera);
            transform.AddComponent(new IsCanvas(cameraReference));
        }

        public static implicit operator Transform(Canvas canvas)
        {
            return canvas.transform;
        }

        public static implicit operator Entity(Canvas canvas)
        {
            return canvas.AsEntity();
        }
    }
}