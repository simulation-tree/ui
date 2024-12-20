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
        public BeginEditing beginEditing;
        public TextValidation validation;
        public Submit submit;
        public Cancel cancel;

        public IsTextField(rint textReference, rint cursorReference, rint highlightReference, BeginEditing beginEditing, TextValidation validation, Submit submit, Cancel cancel)
        {
            this.editing = false;
            this.textLabelReference = textReference;
            this.cursorReference = cursorReference;
            this.highlightReference = highlightReference;
            this.beginEditing = beginEditing;
            this.validation = validation;
            this.submit = submit;
            this.cancel = cancel;
        }
    }
}