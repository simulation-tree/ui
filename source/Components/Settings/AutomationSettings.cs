using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct AutomationSettings
    {
        public rint stateMachineReference;
        public rint idleAutomationReference;
        public rint selectedAutomationReference;
        public rint pressedAutomationReference;

        public AutomationSettings(rint stateMachineReference, rint idleAutomationReference, rint selectedAutomationReference, rint pressedAutomationReference)
        {
            this.stateMachineReference = stateMachineReference;
            this.idleAutomationReference = idleAutomationReference;
            this.selectedAutomationReference = selectedAutomationReference;
            this.pressedAutomationReference = pressedAutomationReference;
        }
    }
}
