using Collections;
using InteractionKit.Components;
using Simulation;
using System;
using System.Numerics;
using Transforms.Components;
using Worlds;

namespace InteractionKit.Systems
{
    public readonly partial struct SelectionSystem : ISystem
    {
        private readonly Dictionary<Entity, uint> selectionStates;
        private readonly Dictionary<Entity, PointerAction> pointerStates;
        private readonly List<(uint entity, LocalToWorld ltw)> selectableEntities;

        private SelectionSystem(Dictionary<Entity, uint> selectionStates, Dictionary<Entity, PointerAction> pointerStates, List<(uint entity, LocalToWorld ltw)> selectableEntities)
        {
            this.selectionStates = selectionStates;
            this.pointerStates = pointerStates;
            this.selectableEntities = selectableEntities;
        }

        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                Dictionary<Entity, uint> selectionStates = new();
                Dictionary<Entity, PointerAction> pointerStates = new();
                List<(uint entity, LocalToWorld ltw)> selectableEntities = new();
                systemContainer.Write(new SelectionSystem(selectionStates, pointerStates, selectableEntities));
            }
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
            Update(world);
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                selectableEntities.Dispose();
                pointerStates.Dispose();
                selectionStates.Dispose();
            }
        }

        private readonly void Update(World world)
        {
            FindSelectableEntities(world);

            ComponentQuery<IsPointer> pointerQuery = new(world);
            foreach (var p in pointerQuery)
            {
                if (!world.IsEnabled(p.entity)) continue;

                Entity pointer = new(world, p.entity);
                ref IsPointer component = ref p.component1;
                Vector2 pointerPosition = component.position;
                float lastDepth = float.MinValue;
                uint currentHoveringOver = default;
                bool hasPrimaryIntent = component.HasPrimaryIntent;
                bool hasSecondaryIntent = component.HasSecondaryIntent;
                bool primaryIntentStarted = false;
                bool secondaryIntentStarted = false;
                if (pointerStates.TryAdd(pointer, component.action))
                {
                    primaryIntentStarted = hasPrimaryIntent;
                    secondaryIntentStarted = hasSecondaryIntent;
                }
                else
                {
                    ref PointerAction action = ref pointerStates[pointer];
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

                    action = component.action;
                }

                //find currently hovering over entity
                foreach ((uint selectableEntity, LocalToWorld ltw) in selectableEntities)
                {
                    Vector3 position = ltw.Position;
                    Vector3 scale = ltw.Scale;
                    ref WorldRotation worldRotationComponent = ref world.TryGetComponent<WorldRotation>(selectableEntity, out bool hasWorldRotation);
                    if (hasWorldRotation)
                    {
                        scale = Vector3.Transform(scale, worldRotationComponent.value);
                    }

                    Vector2 offset = new Vector2(position.X, position.Y) + new Vector2(scale.X, scale.Y);
                    Vector2 min = Vector2.Min(new(position.X, position.Y), offset);
                    Vector2 max = Vector2.Max(new(position.X, position.Y), offset);
                    UIBounds bounds = new(min, max);
                    if (bounds.Contains(pointerPosition))
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
                ref rint hoveringOverReference = ref component.hoveringOverReference;
                uint oldHoveringOver = hoveringOverReference == default ? default : world.GetReference(p.entity, hoveringOverReference);
                if (oldHoveringOver != currentHoveringOver)
                {
                    if (oldHoveringOver != default && world.ContainsEntity(oldHoveringOver))
                    {
                        ref IsSelectable oldSelectable = ref world.GetComponent<IsSelectable>(oldHoveringOver);
                        oldSelectable = default;
                    }

                    if (currentHoveringOver != default)
                    {
                        ref IsSelectable newSelectable = ref world.GetComponent<IsSelectable>(currentHoveringOver);
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
                    ref IsSelectable selectable = ref world.GetComponent<IsSelectable>(currentHoveringOver);
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

        private readonly void FindSelectableEntities(World world)
        {
            selectableEntities.Clear();
            ComponentQuery<IsSelectable, LocalToWorld> selectableQuery = new(world);
            foreach (var s in selectableQuery)
            {
                if (world.IsEnabled(s.entity))
                {
                    selectableEntities.Add((s.entity, s.component2));
                }
            }
        }
    }
}