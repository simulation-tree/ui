using Worlds;

namespace InteractionKit.Components
{
    [ArrayElement]
    public struct LabelCharacter
    {
        public char value;
        
        public LabelCharacter(char value)
        {
            this.value = value;
        }

        public static implicit operator LabelCharacter(char value) => new(value);
        public static implicit operator char(LabelCharacter value) => value.value;
    }
}