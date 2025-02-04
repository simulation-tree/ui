using UI.Components;
using Unmanaged;
using Worlds;

namespace UI
{
    public readonly struct MenuOption
    {
        public readonly Menu rootMenu;
        public readonly OptionPath optionPath;

        internal MenuOption(Menu rootMenu, OptionPath optionPath)
        {
            this.rootMenu = rootMenu;
            this.optionPath = optionPath;
        }

        public readonly override string ToString()
        {
            USpan<char> buffer = stackalloc char[256];
            uint length = ToString(buffer);
            return buffer.Slice(0, length).ToString();
        }

        public readonly uint ToString(USpan<char> buffer)
        {
            uint length = 0;
            byte depth = optionPath.Depth;
            uint entity = rootMenu.value;
            World world = rootMenu.world;
            for (byte d = 0; d < depth; d++)
            {
                uint optionIndex = optionPath[d];
                IsMenuOption option = world.GetArrayElement<IsMenuOption>(entity, optionIndex);
                length += option.text.CopyTo(buffer.Slice(length));
                if (d < depth - 1)
                {
                    buffer[length++] = '/';
                }

                if (option.childMenuReference != default)
                {
                    entity = world.GetReference(entity, option.childMenuReference);
                }
            }

            return length;
        }
    }
}