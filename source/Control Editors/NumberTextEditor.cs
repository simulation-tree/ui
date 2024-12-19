using Data;
using InteractionKit.Functions;
using System.Runtime.InteropServices;
using Unmanaged;
using Worlds;

namespace InteractionKit.ControlEditors
{
    public unsafe readonly struct NumberTextEditor : IControlEditor
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
                else if (componentType.Is<float>())
                {
                    USpan<char> buffer = stackalloc char[32];
                    float value = target.GetComponent<float>();
                    uint length = value.ToString(buffer);
                    textField.SetText(buffer.Slice(0, length));
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

            textField.Validation = new(&Validate);
            textField.Submit = new(&Submit);
            input.AddEntity(textField);
        }

        [UnmanagedCallersOnly]
        private static void Validate(TextValidation.Input input)
        {
            USpan<char> newText = input.NewText;
            uint lastValidIndex = 0;
            bool readDecimal = false;
            for (uint i = 0; i < newText.Length; i++)
            {
                char c = newText[i];
                if (c == '.' || c == '-' || (c >= '0' && c <= '9'))
                {
                    if (c == '.')
                    {
                        if (readDecimal)
                        {
                            break;
                        }

                        readDecimal = true;
                    }

                    lastValidIndex++;
                }
                else
                {
                    break;
                }
            }

            USpan<char> validNewText = newText.Slice(0, lastValidIndex);
            if (float.TryParse(validNewText.AsSystemSpan(), out _))
            {
                input.SetText(validNewText);
            }
            else
            {
                input.SetText(['0']);
            }
        }

        [UnmanagedCallersOnly]
        private static Boolean Submit(Entity textField, Settings settings)
        {
            return true;
        }
    }
}