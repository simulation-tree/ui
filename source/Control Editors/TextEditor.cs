using Data;
using InteractionKit.Functions;
using System.Runtime.InteropServices;
using Unmanaged;
using Worlds;

namespace InteractionKit.ControlEditors
{
    public unsafe readonly struct TextEditor : IControlEditor
    {
        readonly InitializeControlField IControlEditor.InitializeControlField => new(&InitializeControlField);

        [UnmanagedCallersOnly]
        private static void InitializeControlField(InitializeControlField.Input input)
        {
            Canvas canvas = input.Canvas;
            float singleLineHeight = canvas.GetSettings().SingleLineHeight;
            ControlField controlField = input.ControlField;
            TextField textField = new(canvas);
            textField.SetParent(controlField);
            textField.Size = new(1f, singleLineHeight);
            textField.Anchor = new("100", "100%", "0", "100%", "100%", "0");
            textField.Pivot = new(0f, 1f, 0f);
            textField.BackgroundColor = new(0.2f, 0.2f, 0.2f);
            textField.TextColor = Color.White;

            Entity target = input.Target;
            if (input.isComponentType)
            {
                ComponentType componentType = input.ComponentType;
                if (componentType.Is<FixedString>())
                {
                    textField.SetText(target.GetComponent<FixedString>());
                }
            }
            else
            {
                ArrayType arrayType = input.ArrayType;
                if (arrayType.Is<char>())
                {
                    textField.SetText(target.GetArray<char>());
                }
            }

            textField.Submit = new(&Submit);
            input.AddEntity(textField);
        }

        [UnmanagedCallersOnly]
        private static Boolean Submit(Entity textField, Settings settings)
        {
            if (settings.PressedCharacters.Contains(Settings.ShiftCharacter))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}