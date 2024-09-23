using Simulation;

namespace InteractionKit.Components
{
    public struct IsTextField
    {
        public rint textLabelReference;
        public rint cursorReference;
        public rint highlightReference;
        public bool editing;
        public uint selectionStart;
        public uint selectionLength;

        public IsTextField(rint textReference, rint cursorReference, rint highlightReference)
        {
            this.textLabelReference = textReference;
            this.cursorReference = cursorReference;
            this.highlightReference = highlightReference;
        }
    }
}