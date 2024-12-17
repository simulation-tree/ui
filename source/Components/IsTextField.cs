using InteractionKit.Functions;
using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct IsTextField
    {
        public rint textLabelReference;
        public rint cursorReference;
        public rint highlightReference;
        public bool editing;
        public TextValidation validation;

        public IsTextField(rint textReference, rint cursorReference, rint highlightReference, TextValidation validation)
        {
            this.textLabelReference = textReference;
            this.cursorReference = cursorReference;
            this.highlightReference = highlightReference;
            this.editing = false;
        }
    }
}