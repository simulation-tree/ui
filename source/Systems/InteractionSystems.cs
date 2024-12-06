using Simulation;
using System;
using Worlds;

namespace InteractionKit.Systems
{
    public readonly partial struct InteractionSystems : ISystem
    {
        private readonly Simulator simulator;

        private InteractionSystems(Simulator simulator)
        {
            this.simulator = simulator;
            simulator.AddSystem(new CanvasSystem());
            simulator.AddSystem(new SelectionSystem());
            simulator.AddSystem(new VirtualWindowsScrollViewSystem());
            simulator.AddSystem(new InvokeTriggersSystem());
            simulator.AddSystem(new AutomationParameterSystem());
            simulator.AddSystem(new ComponentMixingSystem());
            simulator.AddSystem(new PointerDraggingSelectableSystem());
            simulator.AddSystem(new ToggleSystem());
            simulator.AddSystem(new ScrollHandleMovingSystem());
            simulator.AddSystem(new ScrollViewSystem());
            simulator.AddSystem(new TextFieldEditingSystem());
        }

        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                systemContainer.allocation.Write(new InteractionSystems(systemContainer.Simulator));
            }
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
        }

        void IDisposable.Dispose()
        {
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