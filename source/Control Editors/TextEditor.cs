using Data;
using InteractionKit.Functions;
using Rendering.Components;
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

            Schema schema = canvas.GetWorld().Schema;
            Entity target = input.Target;
            if (input.isComponentType)
            {
                ComponentType componentType = input.ComponentType;
                if (componentType == schema.GetComponent<FixedString>())
                {
                    textField.SetText(target.GetComponent<FixedString>());
                }
            }
            else
            {
                ArrayElementType arrayElementType = input.ArrayElementType;
                if (arrayElementType == schema.GetArrayElement<char>())
                {
                    textField.SetText(target.GetArray<char>());
                }
            }

            textField.BeginEditing = new(&BeginEditing);
            textField.Submit = new(&Submit);
            textField.Cancel = new(&Cancel);
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