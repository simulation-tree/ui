using Simulation;

namespace InteractionKit
{
    public static class SelectablesFunctions
    {
        public static bool IsSelected<T>(this T selectable, Pointer pointer) where T : unmanaged, ISelectable
        {
            Entity currentlySelected = pointer.HoveringOver;
            return selectable.Value == currentlySelected.value;
        }
    }
}