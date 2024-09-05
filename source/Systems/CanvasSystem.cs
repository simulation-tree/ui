using InteractionKit.Components;
using InteractionKit.Events;
using Rendering;
using Simulation;
using System.Numerics;
using Transforms.Components;

namespace InteractionKit.Systems
{
    public class CanvasSystem : SystemBase
    {
        private readonly ComponentQuery<IsCanvas, Position, Scale> canvasQuery;

        public CanvasSystem(World world) : base(world)
        {
            canvasQuery = new();
            Subscribe<InteractionUpdate>(Update);
        }

        public override void Dispose()
        {
            canvasQuery.Dispose();
            base.Dispose();
        }

        private void Update(InteractionUpdate update)
        {
            canvasQuery.Update(world);
            foreach (var x in canvasQuery)
            {
                uint canvasEntity = x.entity;
                IsCanvas canvas = x.Component1;
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

                ref Position position = ref x.Component2;
                position.value = new(0, 0, distanceFromCamera); //todo: wtf: why negative?
                ref Scale scale = ref x.Component3;
                scale.value = new(size, scale.value.Z);
            }
        }
    }
}