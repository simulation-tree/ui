using Collections;
using InteractionKit.Components;
using Simulation;
using System;
using System.Numerics;
using Transforms.Components;
using Worlds;

namespace InteractionKit.Systems
{
    public partial struct PointerDraggingSelectableSystem : ISystem
    {
        private readonly ComponentQuery<IsPointer> pointerQuery;
        private readonly ComponentQuery<IsDraggable> draggableQuery;
        private readonly List<Entity> pressedStates;
        private Entity dragTarget;
        private Entity dragPointer;
        private Vector2 lastPosition;

        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
            Update(world);
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                CleanUp();
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
                    pressedStates.TryRemoveBySwapping(pointer);
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