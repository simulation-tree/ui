using InteractionKit.Components;
using InteractionKit.Events;
using Simulation;
using System.Numerics;
using Transforms.Components;
using Unmanaged.Collections;

namespace InteractionKit.Systems
{
    public class SelectionSystem : SystemBase
    {
        private readonly ComponentQuery<IsSelectable, LocalToWorld> selectableQuery;
        private readonly ComponentQuery<IsPointer> pointersQuery;
        private readonly UnmanagedDictionary<uint, uint> selectionStates;
        private readonly UnmanagedDictionary<uint, PointerAction> pointerStates;

        public SelectionSystem(World world) : base(world)
        {
            selectableQuery = new();
            pointersQuery = new();
            selectionStates = new();
            pointerStates = new();
            Subscribe<InteractionUpdate>(Update);
        }

        public override void Dispose()
        {
            pointerStates.Dispose();
            selectionStates.Dispose();
            pointersQuery.Dispose();
            selectableQuery.Dispose();
            base.Dispose();
        }

        private void Update(InteractionUpdate update)
        {
            pointersQuery.Update(world, true);
            selectableQuery.Update(world, true);
            foreach (var p in pointersQuery)
            {
                ref IsPointer pointer = ref p.Component1;
                Vector2 pointerPosition = pointer.position;
                float lastDepth = float.MinValue;
                uint currentHoveringOver = default;
                bool hasPrimaryIntent = pointer.HasPrimaryIntent;
                bool hasSecondaryIntent = pointer.HasSecondaryIntent;
                bool primaryIntentStarted = false;
                bool secondaryIntentStarted = false;
                if (pointerStates.TryAdd(p.entity, pointer.action))
                {
                    primaryIntentStarted = hasPrimaryIntent;
                    secondaryIntentStarted = hasSecondaryIntent;
                }
                else
                {
                    ref PointerAction action = ref pointerStates[p.entity];
                    bool lastPrimaryIntent = (action & PointerAction.Primary) != 0;
                    if (!lastPrimaryIntent && hasPrimaryIntent)
                    {
                        primaryIntentStarted = true;
                    }

                    bool lastSecondaryIntent = (action & PointerAction.Secondary) != 0;
                    if (!lastSecondaryIntent && hasSecondaryIntent)
                    {
                        secondaryIntentStarted = true;
                    }

                    action = pointer.action;
                }

                //find currently hovering over entity
                foreach (var x in selectableQuery)
                {
                    uint selectableEntity = x.entity;
                    LocalToWorld ltw = x.Component2;
                    Vector3 position = ltw.Position;
                    Vector3 scale = ltw.Scale;
                    if (world.TryGetComponent(selectableEntity, out WorldRotation worldRotationComponent))
                    {
                        scale = Vector3.Transform(scale, worldRotationComponent.value);
                    }

                    Vector3 offset = position + scale;
                    Vector3 min = Vector3.Min(position, offset);
                    Vector3 max = Vector3.Max(position, offset);
                    bool isHoveringOver = pointerPosition.X >= min.X && pointerPosition.X <= max.X && pointerPosition.Y >= min.Y && pointerPosition.Y <= max.Y;
                    if (isHoveringOver)
                    {
                        float depth = position.Z;
                        if (lastDepth < depth)
                        {
                            lastDepth = depth;
                            currentHoveringOver = selectableEntity;
                        }
                    }
                }

                //update currently selected entity
                ref rint hoveringOverReference = ref pointer.hoveringOverReference;
                uint oldHoveringOver = world.GetReference(p.entity, hoveringOverReference);
                if (oldHoveringOver != currentHoveringOver)
                {
                    if (oldHoveringOver != default)
                    {
                        ref IsSelectable oldSelectable = ref world.GetComponentRef<IsSelectable>(oldHoveringOver);
                        oldSelectable = default;
                    }

                    if (currentHoveringOver != default)
                    {
                        ref IsSelectable newSelectable = ref world.GetComponentRef<IsSelectable>(currentHoveringOver);
                        newSelectable.state |= IsSelectable.State.IsSelected;
                    }

                    if (currentHoveringOver == default)
                    {
                        world.RemoveReference(p.entity, hoveringOverReference);
                        hoveringOverReference = default;
                    }
                    else
                    {
                        if (hoveringOverReference == default)
                        {
                            hoveringOverReference = world.AddReference(p.entity, currentHoveringOver);
                        }
                        else
                        {
                            world.SetReference(p.entity, hoveringOverReference, currentHoveringOver);
                        }
                    }
                }
                else if (currentHoveringOver != default && world.ContainsEntity(currentHoveringOver))
                {
                    ref IsSelectable selectable = ref world.GetComponentRef<IsSelectable>(currentHoveringOver);
                    if (primaryIntentStarted)
                    {
                        selectable.state |= IsSelectable.State.WasPrimaryInteractedWith;
                    }
                    else if (hasPrimaryIntent)
                    {
                        if (selectable.WasPrimaryInteractedWith)
                        {
                            selectable.state |= IsSelectable.State.IsPrimaryInteractedWith;
                        }

                        selectable.state &= ~IsSelectable.State.WasPrimaryInteractedWith;
                    }
                    else
                    {
                        selectable.state &= ~IsSelectable.State.IsPrimaryInteractedWith;
                        selectable.state &= ~IsSelectable.State.WasPrimaryInteractedWith;
                    }

                    if (secondaryIntentStarted)
                    {
                        selectable.state |= IsSelectable.State.WasSecondaryInteractedWith;
                    }
                    else if (hasSecondaryIntent)
                    {
                        if (selectable.WasSecondaryInteractedWith)
                        {
                            selectable.state |= IsSelectable.State.IsSecondaryInteractedWith;
                        }

                        selectable.state &= ~IsSelectable.State.WasSecondaryInteractedWith;
                    }
                    else
                    {
                        selectable.state &= ~IsSelectable.State.IsSecondaryInteractedWith;
                        selectable.state &= ~IsSelectable.State.WasSecondaryInteractedWith;
                    }
                }
            }

            //todo: handle pressing Tab to switch to the next selectable
            //todo: handle using arrow keys to switch to an adjacent selectable
        }
    }
}