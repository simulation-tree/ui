namespace InteractionKit.Components
{
    public struct TextEditRange
    {
        public uint start;
        public uint length;

        public TextEditRange(uint start, uint length)
        {
            this.start = start;
            this.length = length;
        }
    }
}
