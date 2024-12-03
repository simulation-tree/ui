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
        private readonly ComponentQuery<IsSelectable, IsStateful> selectablesQuery;
        private readonly ComponentQuery<IsPointer> pointerQuery;
        private readonly List<Entity> selectedEntities;

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

        public AutomationParameterSystem()
        {
            selectablesQuery = new();
            pointerQuery = new();
            selectedEntities = new();
        }

        private readonly void CleanUp()
        {
            selectedEntities.Dispose();
            pointerQuery.Dispose();
            selectablesQuery.Dispose();
        }

        private readonly void Update(World world)
        {
            FindSelectedEntities(world);
            UpdateSelectableParameters(world);
        }

        private readonly void FindSelectedEntities(World world)
        {
            selectedEntities.Clear();
            pointerQuery.Update(world);
            foreach (var p in pointerQuery)
            {
                ref IsPointer pointer = ref p.Component1;
                if (pointer.hoveringOverReference != default)
                {
                    uint hoveringOverEntity = world.GetReference(p.entity, pointer.hoveringOverReference);
                    selectedEntities.Add(new(world, hoveringOverEntity));
                }
            }
        }

        private readonly void UpdateSelectableParameters(World world)
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