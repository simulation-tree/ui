using InteractionKit.Functions;
using Simulation;

namespace InteractionKit.Components
{
    public struct IsVirtualWindow
    {
        public rint headerReference;
        public rint containerReference;
        public rint titleLabelReference;
        public rint closeButtonReference;
        public VirtualWindowCloseFunction closeCallback;

        public IsVirtualWindow(rint headerReference, rint containerReference, rint titleLabelReference, rint closeButtonReference, VirtualWindowCloseFunction closeCallback)
        {
            this.headerReference = headerReference;
            this.containerReference = containerReference;
            this.titleLabelReference = titleLabelReference;
            this.closeButtonReference = closeButtonReference;
            this.closeCallback = closeCallback;
        }
    }
}