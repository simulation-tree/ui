using InteractionKit.Components;
using Rendering;
using Simulation;
using Simulation.Functions;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms.Components;

namespace InteractionKit.Systems
{
    public readonly struct CanvasSystem : ISystem
    {
        private readonly ComponentQuery<IsCanvas, Position, Scale> canvasQuery;

        readonly unsafe InitializeFunction ISystem.Initialize => new(&Initialize);
        readonly unsafe IterateFunction ISystem.Iterate => new(&Update);
        readonly unsafe FinalizeFunction ISystem.Finalize => new(&Finalize);

        [UnmanagedCallersOnly]
        private static void Initialize(SystemContainer container, World world)
        {
        }

        [UnmanagedCallersOnly]
        private static void Update(SystemContainer container, World world, TimeSpan delta)
        {
            ref CanvasSystem system = ref container.Read<CanvasSystem>();
            system.Update(world);
        }

        [UnmanagedCallersOnly]
        private static void Finalize(SystemContainer container, World world)
        {
            if (container.World == world)
            {
                ref CanvasSystem system = ref container.Read<CanvasSystem>();
                system.CleanUp();
            }
        }

        public CanvasSystem()
        {
            canvasQuery = new();
        }

        private readonly void CleanUp()
        {
            canvasQuery.Dispose();
        }

        private readonly void Update(World world)
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