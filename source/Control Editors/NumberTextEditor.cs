using Data;
using InteractionKit.Functions;
using Rendering.Components;
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

            textField.BeginEditing = new(&BeginEditing);
            textField.Validation = new(&Validate);
            textField.Submit = new(&Submit);
            textField.Cancel = new(&Cancel);
            input.AddEntity(textField);
        }

        [UnmanagedCallersOnly]
        private static void Validate(TextValidation.Input input)
        {
            USpan<char> newText = input.NewText;
            USpan<char> validatedText = stackalloc char[(int)newText.Length];
            uint validatedLength = 0;
            for (uint i = 0; i < newText.Length; i++)
            {
                char c = newText[i];
                if (c == '.' || c == '-' || (c >= '0' && c <= '9'))
                {
                    validatedText[validatedLength++] = c;
                }
            }

            input.SetNewText(validatedText.Slice(0, validatedLength));
        }

        [UnmanagedCallersOnly]
        private static Boolean Submit(Entity entity, Settings settings)
        {
            TextField textField = entity.As<TextField>();
            USpan<char> newText = textField.Value;
            if (!float.TryParse(newText.AsSystemSpan(), out _))
            {
                textField.SetText(['0']);
            }

            return true;
        }

        [UnmanagedCallersOnly]
        private static void BeginEditing(Entity entity)
        {
            //store original state
            TextField textField = entity.As<TextField>();
            USpan<char> originalText = textField.Value;
            if (!entity.ContainsArray<TextCharacter>())
            {
                entity.CreateArray(originalText.As<TextCharacter>());
            }
            else
            {
                entity.ResizeArray<TextCharacter>(originalText.Length).CopyFrom(originalText.As<TextCharacter>());
            }
        }

        [UnmanagedCallersOnly]
        private static void Cancel(Entity entity)
        {
            //revert
            TextField textField = entity.As<TextField>();
            USpan<TextCharacter> originalText = entity.GetArray<TextCharacter>();
            textField.SetText(originalText.As<char>());
        }
    }
}