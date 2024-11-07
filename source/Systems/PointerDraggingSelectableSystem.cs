using Collections;
using InteractionKit.Components;
using Simulation;
using Simulation.Functions;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms.Components;

namespace InteractionKit.Systems
{
    public struct PointerDraggingSelectableSystem : ISystem
    {
        private readonly ComponentQuery<IsPointer> pointerQuery;
        private readonly ComponentQuery<IsDraggable> draggableQuery;
        private readonly List<Entity> pressedStates;
        private Entity dragTarget;
        private Entity dragPointer;
        private Vector2 lastPosition;

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
            ref PointerDraggingSelectableSystem system = ref container.Read<PointerDraggingSelectableSystem>();
            system.Update(world);
        }

        [UnmanagedCallersOnly]
        private static void Finalize(SystemContainer container, World world)
        {
            if (container.World == world)
            {
                ref PointerDraggingSelectableSystem system = ref container.Read<PointerDraggingSelectableSystem>();
                system.CleanUp();
            }
        }

        public PointerDraggingSelectableSystem()
        {
            pointerQuery = new();
            draggableQuery = new();
            pressedStates = new();
        }

        private readonly void CleanUp()
        {
            pressedStates.Dispose();
            draggableQuery.Dispose();
            pointerQuery.Dispose();
        }

        private void Update(World world)
        {
            draggableQuery.Update(world, true);
            pointerQuery.Update(world);
            foreach (var p in pointerQuery)
            {
                Entity pointer = new(world, p.entity);
                bool pressed = p.Component1.HasPrimaryIntent;
                bool wasPressed = false;
                if (pressed)
                {
                    wasPressed = pressedStates.TryAdd(pointer);
                }
                else
                {
                    pressedStates.TryRemove(pointer);
                }

                Vector2 position = p.Component1.position;
                if (wasPressed && dragTarget == default)
                {
                    rint hoveringOverReference = p.Component1.hoveringOverReference;
                    if (hoveringOverReference != default)
                    {
                        uint hoveringOverEntity = pointer.GetReference(hoveringOverReference);
                        if (draggableQuery.TryIndexOf(hoveringOverEntity, out uint index))
                        {
                            rint targetReference = draggableQuery[index].Component1.targetReference;
                            uint targetEntity = targetReference == default ? default : world.GetReference(hoveringOverEntity, targetReference);
                            if (targetEntity == default)
                            {
                                targetEntity = hoveringOverEntity;
                            }

                            if (targetEntity != default && world.ContainsEntity(targetEntity))
                            {
                                dragTarget = new(world, targetEntity);
                                dragPointer = pointer;
                                lastPosition = position;
                            }
                        }
                    }
                }
                else if (!pressed && pointer == dragPointer)
                {
                    dragTarget = default;
                }
            }

            if (dragTarget != default && world.ContainsEntity(dragTarget))
            {
                Vector2 position = dragPointer.GetComponent<IsPointer>().position;
                Vector2 pointerDelta = position - lastPosition;
                lastPosition = position;
                ref Position selectablePosition = ref dragTarget.TryGetComponentRef<Position>(out bool contains);
                if (contains)
                {
                    selectablePosition.value += new Vector3(pointerDelta, 0);
                }
            }
        }
    }
}