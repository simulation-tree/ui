using Cameras;
using InteractionKit.Components;
using System;
using Transforms;
using Worlds;

namespace InteractionKit
{
    public readonly struct Canvas : IEntity
    {
        private readonly Transform transform;

        /// <summary>
        /// The camera that all elements in the canvas will be rendered to.
        /// </summary>
        public readonly Camera Camera
        {
            get
            {
                ref IsCanvas component = ref transform.AsEntity().GetComponentRef<IsCanvas>();
                return transform.AsEntity().GetReference<Camera>(component.cameraReference);
            }
            set
            {
                ref IsCanvas component = ref transform.AsEntity().GetComponentRef<IsCanvas>();
                if (component.cameraReference == default)
                {
                    component.cameraReference = transform.AsEntity().AddReference(value);
                }
                else
                {
                    transform.AsEntity().SetReference(component.cameraReference, value);
                }
            }
        }

        readonly uint IEntity.Value => transform.GetEntityValue();
        readonly World IEntity.World => transform.GetWorld();
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

        public readonly void Dispose()
        {
            transform.Dispose();
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