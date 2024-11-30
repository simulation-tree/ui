using Worlds;

namespace InteractionKit
{
    public static class SelectableExtensions
    {
        public static bool IsSelected<T>(this T selectable, Pointer pointer) where T : unmanaged, ISelectable
        {
            Entity currentlySelected = pointer.HoveringOver;
            return selectable.Value == currentlySelected.value;
        }
    }
}