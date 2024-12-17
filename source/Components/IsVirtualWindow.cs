using InteractionKit.Functions;
using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct IsVirtualWindow
    {
        public rint headerReference;
        public rint titleLabelReference;
        public rint closeButtonReference;
        public rint scrollBarReference;
        public rint viewReference;
        public VirtualWindowClose closeCallback;

        public IsVirtualWindow(rint headerReference, rint titleLabelReference, rint closeButtonReference, rint scrollBarReference, rint viewReference, VirtualWindowClose closeCallback)
        {
            this.headerReference = headerReference;
            this.titleLabelReference = titleLabelReference;
            this.closeButtonReference = closeButtonReference;
            this.scrollBarReference = scrollBarReference;
            this.viewReference = viewReference;
            this.closeCallback = closeCallback;
        }
    }
}