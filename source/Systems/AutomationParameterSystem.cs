using Automations;
using Automations.Components;
using InteractionKit.Components;
using InteractionKit.Events;
using Simulation;
using Unmanaged.Collections;

namespace InteractionKit.Systems
{
    public class AutomationParameterSystem : SystemBase
    {
        private readonly ComponentQuery<IsSelectable, IsStateful> selectablesQuery;
        private readonly ComponentQuery<IsPointer> pointerQuery;
        private readonly UnmanagedList<uint> selectedEntities;

        public AutomationParameterSystem(World world) : base(world)
        {
            selectablesQuery = new();
            pointerQuery = new();
            selectedEntities = new();
            Subscribe<InteractionUpdate>(Update);
        }

        public override void Dispose()
        {
            selectedEntities.Dispose();
            pointerQuery.Dispose();
            selectablesQuery.Dispose();
            base.Dispose();
        }

        private void Update(InteractionUpdate update)
        {
            FindSelectedEntities();
            UpdateSelectableParameters();
        }

        private void FindSelectedEntities()
        {
            selectedEntities.Clear();
            pointerQuery.Update(world);
            foreach (var p in pointerQuery)
            {
                ref IsPointer pointer = ref p.Component1;
                if (pointer.hoveringOverReference != default)
                {
                    uint hoveringOverEntity = world.GetReference(p.entity, pointer.hoveringOverReference);
                    selectedEntities.Add(hoveringOverEntity);
                }
            }
        }

        private void UpdateSelectableParameters()
        {
            selectablesQuery.Update(world);
            foreach (var x in selectablesQuery)
            {
                IsSelectable selectable = x.Component1;
                bool pressed = (selectable.state & IsSelectable.State.WasPrimaryInteractedWith) != 0;
                pressed |= (selectable.state & IsSelectable.State.IsPrimaryInteractedWith) != 0;
                bool selected = selectedEntities.Contains(x.entity);
                Stateful stateful = new(world, x.entity);
                stateful.SetParameter("selected", selected ? 1 : 0);
                stateful.SetParameter("pressed", pressed ? 1 : 0);
            }
        }
    }
}