using Collections;
using InteractionKit.Components;
using Simulation;
using Simulation.Functions;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms.Components;
using Worlds;

namespace InteractionKit.Systems
{
    public readonly struct SelectionSystem : ISystem
    {
        private readonly ComponentQuery<IsSelectable, LocalToWorld> selectableQuery;
        private readonly ComponentQuery<IsPointer> pointersQuery;
        private readonly Dictionary<Entity, uint> selectionStates;
        private readonly Dictionary<Entity, PointerAction> pointerStates;

        readonly unsafe StartSystem ISystem.Start => new(&Start);
        readonly unsafe UpdateSystem ISystem.Update => new(&Update);
        readonly unsafe FinishSystem ISystem.Finish => new(&Finish);

        [UnmanagedCallersOnly]
        private static void Start(SystemContainer container, World world)
        {
        }

        [UnmanagedCallersOnly]
        private static void Update(SystemContainer container, World world, TimeSpan delta)
        {
            ref SelectionSystem system = ref container.Read<SelectionSystem>();
            system.Update(world);
        }

        [UnmanagedCallersOnly]
        private static void Finish(SystemContainer container, World world)
        {
            if (container.World == world)
            {
                ref SelectionSystem system = ref container.Read<SelectionSystem>();
                system.CleanUp();
            }
        }

        public SelectionSystem()
        {
            selectableQuery = new();
            pointersQuery = new();
            selectionStates = new();
            pointerStates = new();
        }

        private readonly void CleanUp()
        {
            pointerStates.Dispose();
            selectionStates.Dispose();
            pointersQuery.Dispose();
            selectableQuery.Dispose();
        }

        private void Update(World world)
        {
            pointersQuery.Update(world, true);
            selectableQuery.Update(world, true);
            foreach (var p in pointersQuery)
            {
                Entity pointer = new(world, p.entity);
                ref IsPointer component = ref p.Component1;
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
                ref rint hoveringOverReference = ref component.hoveringOverReference;
                uint oldHoveringOver = hoveringOverReference == default ? default : world.GetReference(p.entity, hoveringOverReference);
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