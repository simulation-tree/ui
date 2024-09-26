namespace InteractionKit.Components
{
    public struct TextEditState
    {
        public uint selectionStart;
        public uint selectionEnd;
        public uint cursorIndex;

        public TextEditState(uint selectionStart, uint selectionEnd, uint cursorIndex)
        {
            this.selectionStart = selectionStart;
            this.selectionEnd = selectionEnd;
            this.cursorIndex = cursorIndex;
        }
    }
}
