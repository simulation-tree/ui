using Cameras;
using InteractionKit.Components;
using Rendering;
using System;
using System.Numerics;
using Transforms;
using Worlds;

namespace InteractionKit
{
    public readonly struct Canvas : IEntity, IEquatable<Canvas>
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

        /// <summary>
        /// The settings that this canvas will use.
        /// </summary>
        public readonly Settings Settings
        {
            get
            {
                ref IsCanvas component = ref transform.AsEntity().GetComponent<IsCanvas>();
                uint settingsEntity = transform.GetReference(component.settingsReference);
                return new Entity(transform.GetWorld(), settingsEntity).As<Settings>();
            }
        }

        public readonly ref LayerMask RenderMask => ref transform.AsEntity().GetComponent<IsCanvas>().renderMask;
        public readonly ref LayerMask SelectionMask => ref transform.AsEntity().GetComponent<IsCanvas>().selectionMask;

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

        public Canvas(World world, Settings settings, Camera camera, LayerMask renderMask, LayerMask selectionMask)
        {
            transform = new(world);

            rint cameraReference = transform.AddReference(camera);
            rint settingsReference = transform.AddReference(settings);
            transform.AddComponent(new IsCanvas(cameraReference, settingsReference, renderMask, selectionMask));
        }

        public Canvas(World world, Settings settings, Camera camera)
        {
            transform = new(world);

            rint cameraReference = transform.AddReference(camera);
            rint settingsReference = transform.AddReference(settings);
            transform.AddComponent(new IsCanvas(cameraReference, settingsReference, LayerMask.All, LayerMask.All));
        }

        public readonly void Dispose()
        {
            transform.Dispose();
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is Canvas canvas && Equals(canvas);
        }

        public readonly bool Equals(Canvas other)
        {
            return transform.Equals(other.transform);
        }

        public readonly override int GetHashCode()
        {
            return transform.GetHashCode();
        }

        public static bool operator ==(Canvas left, Canvas right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Canvas left, Canvas right)
        {
            return !(left == right);
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