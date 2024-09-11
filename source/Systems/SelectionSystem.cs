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
        private readonly UnmanagedList<uint> pressedStates;
        private readonly UnmanagedList<uint> pressedPointers;

        public SelectionSystem(World world) : base(world)
        {
            selectableQuery = new();
            pointersQuery = new();
            selectionStates = new();
            pressedStates = new();
            pressedPointers = new();
            Subscribe<InteractionUpdate>(Update);
        }

        public override void Dispose()
        {
            pressedPointers.Dispose();
            pressedStates.Dispose();
            selectionStates.Dispose();
            pointersQuery.Dispose();
            selectableQuery.Dispose();
            base.Dispose();
        }

        private void Update(InteractionUpdate update)
        {
            //reset pointers first
            pressedPointers.Clear();
            pointersQuery.Update(world);
            foreach (var p in pointersQuery)
            {
                ref rint selectedReference = ref p.Component1.selectedReference;
                if (selectedReference != default)
                {
                    world.RemoveReference(p.entity, selectedReference);
                    selectedReference = default;
                }

                bool pressed = p.Component1.HasPrimaryIntent;
                if (pressed && pressedStates.TryAdd(p.entity))
                {
                    pressedPointers.Add(p.entity);
                }
                else if (!pressed)
                {
                    pressedStates.TryRemove(p.entity);
                }
            }

            selectableQuery.Update(world, true);
            foreach (var x in selectableQuery)
            {
                uint selectableEntity = x.entity;
                LocalToWorld ltw = x.Component2;
                Vector3 min = ltw.Position;
                Vector3 max = ltw.Position + ltw.Scale;
                IsSelectable.State state = x.Component1.state;
                state &= ~IsSelectable.State.WasPrimaryInteractedWith;
                bool selected = false;
                bool primaryInteracted = false;
                foreach (var p in pointersQuery)
                {
                    ref IsPointer pointer = ref p.Component1;
                    bool contains = pointer.position.X >= min.X && pointer.position.X <= max.X && pointer.position.Y >= min.Y && pointer.position.Y <= max.Y;
                    if (contains)
                    {
                        selected = true;
                        pointer.selectedReference = world.AddReference(p.entity, selectableEntity);
                        bool wasPressed = pressedPointers.Contains(p.entity);
                        if (wasPressed)
                        {
                            state |= IsSelectable.State.WasPrimaryInteractedWith;
                        }
                        else if (pointer.HasPrimaryIntent)
                        {
                            state |= IsSelectable.State.IsPrimaryInteractedWith;
                            primaryInteracted = true;
                        }
                    }
                }

                if (selected)
                {
                    state |= IsSelectable.State.Selected;
                }
                else
                {
                    state &= ~IsSelectable.State.Selected;
                }

                if (!primaryInteracted)
                {
                    state &= ~IsSelectable.State.IsPrimaryInteractedWith;
                }

                ref IsSelectable selectable = ref x.Component1;
                selectable.state = state;
            }

            //todo: handle pressing Tab to switch to the next selectable
            //todo: handle using arrow keys to switch to an adjacent selectable
        }
    }
}