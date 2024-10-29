using Simulation;
using Simulation.Functions;
using System;
using System.Runtime.InteropServices;

namespace InteractionKit.Systems
{
    public readonly struct InteractionSystems : ISystem
    {
        readonly unsafe InitializeFunction ISystem.Initialize => new(&Initialize);
        readonly unsafe IterateFunction ISystem.Update => new(&Update);
        readonly unsafe FinalizeFunction ISystem.Finalize => new(&Finalize);

        [UnmanagedCallersOnly]
        private static void Initialize(SystemContainer container, World world)
        {
            if (container.World == world)
            {
                container.Simulator.AddSystem<CanvasSystem>();
                container.Simulator.AddSystem<SelectionSystem>();
                container.Simulator.AddSystem<VirtualWindowsScrollViewSystem>();
                container.Simulator.AddSystem<InvokeTriggersSystem>();
                container.Simulator.AddSystem<AutomationParameterSystem>();
                container.Simulator.AddSystem<ComponentMixingSystem>();
                container.Simulator.AddSystem<PointerDraggingSelectableSystem>();
                container.Simulator.AddSystem<ToggleSystem>();
                container.Simulator.AddSystem<ScrollHandleMovingSystem>();
                container.Simulator.AddSystem<ScrollViewSystem>();
                container.Simulator.AddSystem<TextFieldEditingSystem>();
            }
        }

        [UnmanagedCallersOnly]
        private static void Update(SystemContainer container, World world, TimeSpan delta)
        {
        }

        [UnmanagedCallersOnly]
        private static void Finalize(SystemContainer container, World world)
        {
            if (container.World == world)
            {
                container.Simulator.RemoveSystem<TextFieldEditingSystem>();
                container.Simulator.RemoveSystem<ScrollViewSystem>();
                container.Simulator.RemoveSystem<ScrollHandleMovingSystem>();
                container.Simulator.RemoveSystem<ToggleSystem>();
                container.Simulator.RemoveSystem<PointerDraggingSelectableSystem>();
                container.Simulator.RemoveSystem<ComponentMixingSystem>();
                container.Simulator.RemoveSystem<AutomationParameterSystem>();
                container.Simulator.RemoveSystem<InvokeTriggersSystem>();
                container.Simulator.RemoveSystem<VirtualWindowsScrollViewSystem>();
                container.Simulator.RemoveSystem<SelectionSystem>();
                container.Simulator.RemoveSystem<CanvasSystem>();
            }
        }
    }
}