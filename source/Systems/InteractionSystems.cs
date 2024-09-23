using Simulation;

namespace InteractionKit.Systems
{
    public class InteractionSystems : SystemBase
    {
        public readonly CanvasSystem canvases;
        public readonly SelectionSystem selections;
        public readonly VirtualWindowsScrollViewSystem virtualWindowsScrollViews;
        public readonly InvokeTriggersSystem triggers;
        public readonly AutomationParameterSystem automationParameters;
        public readonly ComponentMixingSystem componentMixing;
        public readonly PointerDraggingSelectableSystem pointerDragging;
        public readonly ToggleSystem toggles;
        public readonly ScrollHandleMovingSystem scrollHandles;
        public readonly ScrollViewSystem scrollViews;
        public readonly TextFieldEditingSystem textFieldEditing;

        public InteractionSystems(World world) : base(world)
        {
            canvases = new(world);
            selections = new(world);
            virtualWindowsScrollViews = new(world);
            triggers = new(world);
            automationParameters = new(world);
            componentMixing = new(world);
            pointerDragging = new(world);
            toggles = new(world);
            scrollHandles = new(world);
            scrollViews = new(world);
            textFieldEditing = new(world);
        }

        public override void Dispose()
        {
            textFieldEditing.Dispose();
            scrollViews.Dispose();
            scrollHandles.Dispose();
            toggles.Dispose();
            pointerDragging.Dispose();
            componentMixing.Dispose();
            automationParameters.Dispose();
            triggers.Dispose();
            virtualWindowsScrollViews.Dispose();
            selections.Dispose();
            canvases.Dispose();
            base.Dispose();
        }
    }
}