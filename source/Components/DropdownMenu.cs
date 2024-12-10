using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct DropdownMenu
    {
        public rint dropdownReference;

        public DropdownMenu(rint dropdownReference)
        {
            this.dropdownReference = dropdownReference;
        }
    }
}