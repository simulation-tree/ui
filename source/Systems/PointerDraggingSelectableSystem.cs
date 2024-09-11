using InteractionKit.Components;
using InteractionKit.Events;
using Simulation;
using System.Numerics;
using Transforms.Components;
using Unmanaged.Collections;

namespace InteractionKit.Systems
{
    public class PointerDraggingSelectableSystem : SystemBase
    {
        private readonly ComponentQuery<IsPointer> pointerQuery;
        private readonly ComponentQuery<IsDraggable> draggableQuery;
        private readonly UnmanagedList<uint> pressedStates;
        private uint dragTarget;
        private uint currentPointer;
        private Vector2 lastPosition;

        public PointerDraggingSelectableSystem(World world) : base(world)
        {
            pointerQuery = new();
            draggableQuery = new();
            pressedStates = new();
            Subscribe<InteractionUpdate>(Update);
        }

        public override void Dispose()
        {
            pressedStates.Dispose();
            draggableQuery.Dispose();
            pointerQuery.Dispose();
            base.Dispose();
        }

        private void Update(InteractionUpdate update)
        {
            draggableQuery.Update(world, true);
            pointerQuery.Update(world);
            foreach (var p in pointerQuery)
            {
                uint pointerEntity = p.entity;
                bool pressed = p.Component1.HasPrimaryIntent;
                bool wasPressed = false;
                if (pressed)
                {
                    wasPressed = pressedStates.TryAdd(pointerEntity);
                }
                else
                {
                    pressedStates.TryRemove(pointerEntity);
                }

                Vector2 position = p.Component1.position;
                if (wasPressed && dragTarget == default)
                {
                    rint selectedReference = p.Component1.selectedReference;
                    uint selectedEntity = world.GetReference(pointerEntity, selectedReference);
                    if (selectedEntity != default && draggableQuery.TryIndexOf(selectedEntity, out uint index))
                    {
                        rint targetReference = draggableQuery[index].Component1.targetReference;
                        uint targetEntity = world.GetReference(selectedEntity, targetReference);
                        if (targetEntity == default)
                        {
                            targetEntity = selectedEntity;
                        }

                        if (targetEntity != default && world.ContainsEntity(targetEntity))
                        {
                            dragTarget = targetEntity;
                            currentPointer = pointerEntity;
                            lastPosition = position;
                        }
                    }
                }
                else if (!pressed && pointerEntity == currentPointer)
                {
                    dragTarget = default;
                }
            }

            if (dragTarget != default)
            {
                Vector2 position = world.GetComponent<IsPointer>(currentPointer).position;
                Vector2 pointerDelta = position - lastPosition;
                lastPosition = position;
                ref Position selectablePosition = ref world.TryGetComponentRef<Position>(dragTarget, out bool contains);
                if (contains)
                {
                    selectablePosition.value += new Vector3(pointerDelta, 0);
                }
            }
        }
    }
}