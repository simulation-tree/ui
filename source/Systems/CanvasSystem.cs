using Cameras;
using InteractionKit.Components;
using Rendering;
using Simulation;
using System;
using System.Numerics;
using Transforms.Components;
using Unmanaged;
using Worlds;

namespace InteractionKit.Systems
{
    public readonly partial struct CanvasSystem : ISystem
    {
        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
            USpan<uint> destroyedCanvases = stackalloc uint[64];
            uint destroyedCanvasCount = 0;

            ComponentQuery<IsCanvas, Position, Scale> canvasQuery = new(world);
            canvasQuery.ExcludeDisabled(true);
            foreach (var x in canvasQuery)
            {
                uint canvasEntity = x.entity;
                ref IsCanvas component = ref x.component1;
                rint cameraReference = component.cameraReference;
                uint cameraEntity = world.GetReference(canvasEntity, cameraReference);
                float distanceFromCamera = Settings.ZScale;
                Vector2 size = default;
                if (cameraEntity != default && world.ContainsEntity(cameraEntity))
                {
                    Camera camera = new(world, cameraEntity);
                    if (camera.IsDestroyed())
                    {
                        destroyedCanvases[destroyedCanvasCount++] = canvasEntity;
                        continue;
                    }

                    Destination destination = camera.Destination;
                    if (destination != default && !destination.IsDestroyed())
                    {
                        size = destination.SizeAsVector2();
                    }

                    distanceFromCamera += camera.Depth.min;
                }

                ref Position position = ref x.component2;
                position.value = new(0, 0, distanceFromCamera);

                ref Scale scale = ref x.component3;
                scale.value = new(size, scale.value.Z);
            }

            for (uint i = 0; i < destroyedCanvasCount; i++)
            {
                world.DestroyEntity(destroyedCanvases[i]);
            }
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
        }
    }
}