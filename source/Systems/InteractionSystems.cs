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
        public readonly PointerDraggingSelectableSystem pointerDragging;
        public readonly ToggleSystem toggles;
        public readonly ScrollHandleDragSystem scrollHandles;
        public readonly ScrollViewSystem scrollViews;

        public InteractionSystems(World world) : base(world)
        {
            canvases = new(world);
            selections = new(world);
            triggers = new(world);
            automationParameters = new(world);
            componentMixing = new(world);
            pointerDragging = new(world);
            toggles = new(world);
            scrollHandles = new(world);
            scrollViews = new(world);
        }

        public override void Dispose()
        {
            scrollViews.Dispose();
            scrollHandles.Dispose();
            toggles.Dispose();
            pointerDragging.Dispose();
            componentMixing.Dispose();
            automationParameters.Dispose();
            triggers.Dispose();
            selections.Dispose();
            canvases.Dispose();
            base.Dispose();
        }
    }
}