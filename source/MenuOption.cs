using System;
using UI.Components;
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
            Span<char> buffer = stackalloc char[256];
            int length = ToString(buffer);
            return buffer.Slice(0, length).ToString();
        }

        public readonly int ToString(Span<char> buffer)
        {
            int length = 0;
            byte depth = optionPath.Depth;
            uint entity = rootMenu.value;
            World world = rootMenu.world;
            for (int d = 0; d < depth; d++)
            {
                int optionIndex = optionPath[d];
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