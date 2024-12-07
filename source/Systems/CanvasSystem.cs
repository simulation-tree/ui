using Cameras;
using InteractionKit.Components;
using Rendering;
using Simulation;
using System;
using System.Numerics;
using Transforms.Components;
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
            ComponentQuery<IsCanvas, Position, Scale> canvasQuery = new(world);
            foreach (var x in canvasQuery)
            {
                uint canvasEntity = x.entity;
                ref IsCanvas canvas = ref x.component1;
                rint cameraReference = canvas.cameraReference;
                uint cameraEntity = world.GetReference(canvasEntity, cameraReference);
                float distanceFromCamera = 0.1f;
                Vector2 size = default;
                if (cameraEntity != default && world.ContainsEntity(cameraEntity))
                {
                    Camera camera = new(world, cameraEntity);
                    Destination destination = camera.Destination;
                    if (destination != default && !destination.IsDestroyed())
                    {
                        size = destination.SizeAsVector2();
                    }

                    distanceFromCamera += camera.Depth.min;
                }

                ref Position position = ref x.component2;
                position.value = new(0, 0, distanceFromCamera); //todo: wtf: why negative?

                ref Scale scale = ref x.component3;
                scale.value = new(size, scale.value.Z);
            }
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
        }
    }
}