using Simulation;

namespace InteractionKit.Systems
{
    public class InteractionSystems : SystemBase
    {
        public readonly CanvasSystem canvases;
        public readonly SelectionSystem selections;
        public readonly InvokeTriggersSystem triggers;
        public readonly AutomationParameterSystem automationParameters;
        public readonly ComponentMixingSystem componentMixing;

        public InteractionSystems(World world) : base(world)
        {
            canvases = new(world);
            selections = new(world);
            triggers = new(world);
            automationParameters = new(world);
            componentMixing = new(world);
        }

        public override void Dispose()
        {
            componentMixing.Dispose();
            automationParameters.Dispose();
            triggers.Dispose();
            selections.Dispose();
            canvases.Dispose();
            base.Dispose();
        }
    }
}