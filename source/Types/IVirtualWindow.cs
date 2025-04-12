using Transforms;
using UI.Functions;
using Unmanaged;

namespace UI
{
    public interface IVirtualWindow
    {
        ASCIIText256 Title { get; }

        /// <summary>
        /// Callback for handling what happens when virtual window's close button is pressed.
        /// <para>
        /// If <c>default</c>, the <see cref="VirtualWindow"/> will be destroyed.
        /// </para>
        /// </summary>
        VirtualWindowClose CloseCallback { get; }

        void OnCreated(Transform transform, Canvas canvas);
    }
}