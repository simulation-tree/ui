namespace InteractionKit
{
    public static class SelectablesFunctions
    {
        public static bool IsSelected<T>(this T selectable, Pointer pointer) where T : unmanaged, ISelectable
        {
            Selectable currentlySelected = pointer.Selected;
            return selectable.Value == currentlySelected.transform.entity.value;
        }
    }
}