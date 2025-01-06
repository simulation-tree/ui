using Cameras;
using InteractionKit.Components;
using System;
using System.Numerics;
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
                World world = transform.GetWorld();
                ref IsCanvas component = ref transform.AsEntity().GetComponent<IsCanvas>();
                uint cameraEntity = transform.GetReference(component.cameraReference);
                return new Entity(world, cameraEntity).As<Camera>();
            }
            set
            {
                ref IsCanvas component = ref transform.AsEntity().GetComponent<IsCanvas>();
                if (component.cameraReference == default)
                {
                    component.cameraReference = transform.AddReference(value);
                }
                else
                {
                    transform.AsEntity().SetReference(component.cameraReference, value);
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
                ref Vector3 localScale = ref transform.LocalScale;
                fixed (Vector3* sizePointer = &localScale)
                {
                    return ref *(Vector2*)sizePointer;
                }
            }
        }

        readonly uint IEntity.Value => transform.GetEntityValue();
        readonly World IEntity.World => transform.GetWorld();

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsCanvas>();
        }

#if NET
        [Obsolete("Default constructor not available", true)]
        public Canvas()
        {
            throw new NotSupportedException();
        }
#endif

        public Canvas(World world, uint existingEntity)
        {
            transform = new(world, existingEntity);
        }

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