using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct TextEditState
    {
        public TextSelection value;

        public TextEditState(TextSelection value)
        {
            this.value = value;
        }
    }
}
