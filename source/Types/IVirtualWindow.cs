using InteractionKit.Functions;
using Unmanaged;

namespace InteractionKit
{
    public interface IVirtualWindow
    {
        FixedString Title { get; }
        VirtualWindowClose CloseCallback { get; }

        void OnCreated(VirtualWindow window, Canvas canvas);
    }
}