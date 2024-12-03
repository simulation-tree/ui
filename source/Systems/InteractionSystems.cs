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
                systemContainer.Simulator.AddSystem<CanvasSystem>();
                systemContainer.Simulator.AddSystem<SelectionSystem>();
                systemContainer.Simulator.AddSystem<VirtualWindowsScrollViewSystem>();
                systemContainer.Simulator.AddSystem<InvokeTriggersSystem>();
                systemContainer.Simulator.AddSystem<AutomationParameterSystem>();
                systemContainer.Simulator.AddSystem<ComponentMixingSystem>();
                systemContainer.Simulator.AddSystem<PointerDraggingSelectableSystem>();
                systemContainer.Simulator.AddSystem<ToggleSystem>();
                systemContainer.Simulator.AddSystem<ScrollHandleMovingSystem>();
                systemContainer.Simulator.AddSystem<ScrollViewSystem>();
                systemContainer.Simulator.AddSystem<TextFieldEditingSystem>();
            }
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                systemContainer.Simulator.RemoveSystem<TextFieldEditingSystem>();
                systemContainer.Simulator.RemoveSystem<ScrollViewSystem>();
                systemContainer.Simulator.RemoveSystem<ScrollHandleMovingSystem>();
                systemContainer.Simulator.RemoveSystem<ToggleSystem>();
                systemContainer.Simulator.RemoveSystem<PointerDraggingSelectableSystem>();
                systemContainer.Simulator.RemoveSystem<ComponentMixingSystem>();
                systemContainer.Simulator.RemoveSystem<AutomationParameterSystem>();
                systemContainer.Simulator.RemoveSystem<InvokeTriggersSystem>();
                systemContainer.Simulator.RemoveSystem<VirtualWindowsScrollViewSystem>();
                systemContainer.Simulator.RemoveSystem<SelectionSystem>();
                systemContainer.Simulator.RemoveSystem<CanvasSystem>();
            }
        }
    }
}