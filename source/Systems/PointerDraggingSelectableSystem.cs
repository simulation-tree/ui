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
        private uint dragTargetEntity;
        private uint dragPointerEntity;
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
                if (wasPressed && dragTargetEntity == default)
                {
                    rint hoveringOverReference = p.Component1.hoveringOverReference;
                    uint hoveringOverEntity = world.GetReference(pointerEntity, hoveringOverReference);
                    if (hoveringOverEntity != default && draggableQuery.TryIndexOf(hoveringOverEntity, out uint index))
                    {
                        rint targetReference = draggableQuery[index].Component1.targetReference;
                        uint targetEntity = world.GetReference(hoveringOverEntity, targetReference);
                        if (targetEntity == default)
                        {
                            targetEntity = hoveringOverEntity;
                        }

                        if (targetEntity != default && world.ContainsEntity(targetEntity))
                        {
                            dragTargetEntity = targetEntity;
                            dragPointerEntity = pointerEntity;
                            lastPosition = position;
                        }
                    }
                }
                else if (!pressed && pointerEntity == dragPointerEntity)
                {
                    dragTargetEntity = default;
                }
            }

            if (dragTargetEntity != default && world.ContainsEntity(dragTargetEntity))
            {
                Vector2 position = world.GetComponent<IsPointer>(dragPointerEntity).position;
                Vector2 pointerDelta = position - lastPosition;
                lastPosition = position;
                ref Position selectablePosition = ref world.TryGetComponentRef<Position>(dragTargetEntity, out bool contains);
                if (contains)
                {
                    selectablePosition.value += new Vector3(pointerDelta, 0);
                }
            }
        }
    }
}