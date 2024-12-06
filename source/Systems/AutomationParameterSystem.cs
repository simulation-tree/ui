using Automations;
using Automations.Components;
using Collections;
using InteractionKit.Components;
using Simulation;
using System;
using Worlds;

namespace InteractionKit.Systems
{
    public readonly partial struct AutomationParameterSystem : ISystem
    {
        private readonly List<Entity> selectedEntities;

        public AutomationParameterSystem()
        {
            selectedEntities = new();
        }

        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
            FindSelectedEntities(world);
            UpdateSelectableParameters(world);
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
        }

        void IDisposable.Dispose()
        {
            selectedEntities.Dispose();
        }

        private readonly void FindSelectedEntities(World world)
        {
            selectedEntities.Clear();
            ComponentQuery<IsPointer> pointerQuery = new(world);
            foreach (var p in pointerQuery)
            {
                ref IsPointer pointer = ref p.component1;
                if (pointer.hoveringOverReference != default)
                {
                    uint hoveringOverEntity = world.GetReference(p.entity, pointer.hoveringOverReference);
                    selectedEntities.Add(new(world, hoveringOverEntity));
                }
            }
        }

        private readonly void UpdateSelectableParameters(World world)
        {
            ComponentQuery<IsSelectable, IsStateful> selectablesQuery = new(world);
            foreach (var x in selectablesQuery)
            {
                ref IsSelectable selectable = ref x.component1;
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