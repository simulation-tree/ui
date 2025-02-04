using System.Runtime.InteropServices;
using Transforms.Components;
using UI.Functions;
using Worlds;

namespace UI.ControlEditors
{
    public unsafe readonly struct BooleanEditor : IControlEditor
    {
        readonly InitializeControlField IControlEditor.InitializeControlField => new(&InitializeControlField);

        [UnmanagedCallersOnly]
        private static void InitializeControlField(InitializeControlField.Input input)
        {
            Canvas canvas = input.Canvas;
            float singleLineHeight = canvas.Settings.SingleLineHeight;
            ControlField controlField = input.ControlField;
            controlField.TryGetComponent(out bool initialValue);
            Toggle toggle = new(canvas, initialValue);
            toggle.SetParent(controlField);
            toggle.Size = new(singleLineHeight, singleLineHeight);
            toggle.BackgroundColor = new(0.2f, 0.2f, 0.2f, 1);
            toggle.CheckmarkColor = new(1, 1, 1, 1);
            toggle.Anchor = Anchor.BottomLeft;
            toggle.Position = new(100f, 0f);
            toggle.Pivot = new(0f, 0f, 0f);
            toggle.Callback = new(&BooleanToggled);
            input.AddEntity(toggle);
        }

        [UnmanagedCallersOnly]
        private static void BooleanToggled(Toggle toggle, Boolean newValue)
        {
            ControlField controlField = toggle.Parent.As<ControlField>();
            Entity target = controlField.Target;
            target.SetComponent<bool>(newValue);
        }
    }
}