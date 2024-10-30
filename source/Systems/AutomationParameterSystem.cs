using Automations;
using Automations.Components;
using Collections;
using InteractionKit.Components;
using Simulation;
using Simulation.Functions;
using System;
using System.Runtime.InteropServices;

namespace InteractionKit.Systems
{
    public readonly struct AutomationParameterSystem : ISystem
    {
        private readonly ComponentQuery<IsSelectable, IsStateful> selectablesQuery;
        private readonly ComponentQuery<IsPointer> pointerQuery;
        private readonly List<Entity> selectedEntities;

        readonly unsafe InitializeFunction ISystem.Initialize => new(&Initialize);
        readonly unsafe IterateFunction ISystem.Update => new(&Update);
        readonly unsafe FinalizeFunction ISystem.Finalize => new(&Finalize);

        [UnmanagedCallersOnly]
        private static void Initialize(SystemContainer container, World world)
        {
        }

        [UnmanagedCallersOnly]
        private static void Update(SystemContainer container, World world, TimeSpan delta)
        {
            ref AutomationParameterSystem system = ref container.Read<AutomationParameterSystem>();
            system.Update(world);
        }

        [UnmanagedCallersOnly]
        private static void Finalize(SystemContainer container, World world)
        {
            if (container.World == world)
            {
                ref AutomationParameterSystem system = ref container.Read<AutomationParameterSystem>();
                system.CleanUp();
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