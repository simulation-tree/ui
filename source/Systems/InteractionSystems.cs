using Simulation;
using System;
using Worlds;

namespace InteractionKit.Systems
{
    public readonly partial struct InteractionSystems : ISystem
    {
        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                Simulator simulator = systemContainer.Simulator;
                simulator.AddSystem<CanvasSystem>();
                simulator.AddSystem<SelectionSystem>();
                simulator.AddSystem<VirtualWindowsScrollViewSystem>();
                simulator.AddSystem<InvokeTriggersSystem>();
                simulator.AddSystem<AutomationParameterSystem>();
                simulator.AddSystem<ComponentMixingSystem>();
                simulator.AddSystem<PointerDraggingSelectableSystem>();
                simulator.AddSystem<ToggleSystem>();
                simulator.AddSystem<ScrollHandleMovingSystem>();
                simulator.AddSystem<ScrollViewSystem>();
                simulator.AddSystem<TextFieldEditingSystem>();
            }
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                Simulator simulator = systemContainer.Simulator;
                simulator.RemoveSystem<TextFieldEditingSystem>();
                simulator.RemoveSystem<ScrollViewSystem>();
                simulator.RemoveSystem<ScrollHandleMovingSystem>();
                simulator.RemoveSystem<ToggleSystem>();
                simulator.RemoveSystem<PointerDraggingSelectableSystem>();
                simulator.RemoveSystem<ComponentMixingSystem>();
                simulator.RemoveSystem<AutomationParameterSystem>();
                simulator.RemoveSystem<InvokeTriggersSystem>();
                simulator.RemoveSystem<VirtualWindowsScrollViewSystem>();
                simulator.RemoveSystem<SelectionSystem>();
                simulator.RemoveSystem<CanvasSystem>();
            }
        }
    }
}