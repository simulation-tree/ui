using InteractionKit.Functions;

namespace InteractionKit
{
    public readonly struct ControlEditor
    {
        public readonly InitializeControlField initializeControlField;

        public ControlEditor(InitializeControlField initializeControlField)
        {
            this.initializeControlField = initializeControlField;
        }

        public static ControlEditor Get<T>() where T : unmanaged, IControlEditor
        {
            T template = default;
            return new(template.InitializeControlField);
        }
    }
}