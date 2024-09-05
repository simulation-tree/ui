using InteractionKit.Components;
using InteractionKit.Events;
using Simulation;
using System.Diagnostics;
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

        public SelectionSystem(World world) : base(world)
        {
            selectableQuery = new();
            pointersQuery = new();
            selectionStates = new();
            Subscribe<InteractionUpdate>(Update);
        }

        public override void Dispose()
        {
            selectionStates.Dispose();
            pointersQuery.Dispose();
            selectableQuery.Dispose();
            base.Dispose();
        }

        private void Update(InteractionUpdate update)
        {
            //reset pointers first
            pointersQuery.Update(world);
            foreach (var p in pointersQuery)
            {
                ref rint selectedReference = ref p.Component1.selectedReference;
                if (selectedReference != default)
                {
                    world.RemoveReference(p.entity, selectedReference);
                    selectedReference = default;
                }
            }

            selectableQuery.Update(world);
            foreach (var x in selectableQuery)
            {
                uint selectableEntity = x.entity;
                LocalToWorld ltw = x.Component2;
                Vector3 min = ltw.Position;
                Vector3 max = ltw.Position + ltw.Scale;
                IsSelectable.State state = x.Component1.state;
                state &= ~IsSelectable.State.WasPrimaryInteractedWith;
                state &= ~IsSelectable.State.WasSecondaryInteractedWith;
                bool selected = false;
                bool primaryInteracted = false;
                bool secondaryInteracted = false;
                foreach (var p in pointersQuery)
                {
                    ref IsPointer pointer = ref p.Component1;
                    bool contains = pointer.position.X >= min.X && pointer.position.X <= max.X && pointer.position.Y >= min.Y && pointer.position.Y <= max.Y;
                    if (contains)
                    {
                        selected = true;
                        pointer.selectedReference = world.AddReference(p.entity, selectableEntity);
                        if (pointer.HasPrimaryIntent)
                        {
                            bool was = (state & IsSelectable.State.IsPrimaryInteractedWith) != 0;
                            if (!was)
                            {
                                state |= IsSelectable.State.WasPrimaryInteractedWith;
                            }

                            primaryInteracted = true;
                        }

                        if (pointer.HasSecondaryIntent)
                        {
                            bool was = (state & IsSelectable.State.IsSecondaryInteractedWith) != 0;
                            if (!was)
                            {
                                state |= IsSelectable.State.WasSecondaryInteractedWith;
                            }

                            secondaryInteracted = true;
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

                if (primaryInteracted)
                {
                    state |= IsSelectable.State.IsPrimaryInteractedWith;
                }
                else
                {
                    state &= ~IsSelectable.State.IsPrimaryInteractedWith;
                }

                if (secondaryInteracted)
                {
                    state |= IsSelectable.State.IsSecondaryInteractedWith;
                }
                else
                {
                    state &= ~IsSelectable.State.IsSecondaryInteractedWith;
                }

                ref IsSelectable selectable = ref x.Component1;
                selectable.state = state;
            }

            //todo: handle pressing Tab to switch to the next selectable
            //todo: handle using arrow keys to switch to an adjacent selectable
        }
    }
}