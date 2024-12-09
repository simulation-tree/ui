using Fonts;
using InteractionKit.Components;
using Rendering.Components;
using Simulation;
using System;
using System.Diagnostics;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Unmanaged;
using Worlds;

namespace InteractionKit.Systems
{
    public partial struct TextFieldEditingSystem : ISystem
    {
        private static readonly char[] controlCharacters = [' ', '.', ',', '_', '-', '+', '*', '/'];

        private FixedString lastPressedCharacters;
        private FixedString currentCharacters;
        private DateTime nextPress;
        private bool lastAnyPointerPressed;

        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
            Update(world);
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
        }

        private void Update(World world)
        {
            ComponentQuery<IsTextField, LocalToWorld> textLabelQuery = new(world);
            foreach (var r in textLabelQuery)
            {
                uint entity = r.entity;
                ref IsTextField component = ref r.component1;
                ref LocalToWorld ltw = ref r.component2;
                uint labelEntity = world.GetReference(entity, component.textLabelReference);
                uint cursorEntity = world.GetReference(entity, component.cursorReference);
                uint highlightEntity = world.GetReference(entity, component.highlightReference);
                Canvas canvas = new Entity(world, entity).GetCanvas();
                Vector3 position = ltw.Position;
                Vector3 scale = ltw.Scale;
                Vector2 destinationSize = canvas.Size;
                Vector4 region = new(position.X, destinationSize.Y - position.Y - scale.Y, scale.X, scale.Y);
                world.SetComponent(labelEntity, new RendererScissor(region));
                world.SetComponent(cursorEntity, new RendererScissor(region));
                world.SetComponent(highlightEntity, new RendererScissor(region));
            }

            if (world.TryGetFirst(out Settings settings))
            {
                ComponentQuery<IsPointer> pointerQuery = new(world);
                bool currentAnyPointerPressed = false;
                foreach (var p in pointerQuery)
                {
                    currentAnyPointerPressed |= p.component1.HasPrimaryIntent;
                }

                bool anyPointerPressed = false;
                if (currentAnyPointerPressed != lastAnyPointerPressed)
                {
                    lastAnyPointerPressed = currentAnyPointerPressed;
                    anyPointerPressed = currentAnyPointerPressed;
                }

                FixedString pressedCharacters = default;
                pressedCharacters.CopyFrom(settings.PressedCharacters);
                bool pressedEscape = pressedCharacters.Contains(Settings.EscapeCharacter);
                bool editingAny = false;
                bool startedEditing = false;
                DateTime now = DateTime.UtcNow;
                USpan<char> pressedBuffer = stackalloc char[(int)FixedString.Capacity];
                ulong ticks = (ulong)((now - DateTime.UnixEpoch).TotalSeconds * 3f);

                ComponentQuery<IsTextField, IsSelectable> textFieldQuery = new(world);
                foreach (var t in textFieldQuery)
                {
                    uint textFieldEntity = t.entity;
                    ref IsTextField component = ref t.component1;
                    ref IsSelectable selectable = ref t.component2;
                    bool startEditing = selectable.WasPrimaryInteractedWith;
                    if (startEditing && !startedEditing)
                    {
                        ComponentQuery<IsTextField> otherTextFieldQuery = new(world);
                        foreach (var otherT in otherTextFieldQuery)
                        {
                            ref IsTextField otherComponent = ref otherT.component1;
                            otherComponent.editing = false;
                        }

                        StartEditing(world, textFieldEntity);
                        startedEditing = true;
                    }
                    else if (pressedEscape && component.editing)
                    {
                        component.editing = false;
                    }

                    rint cursorReference = component.cursorReference;
                    uint cursorEntity = world.GetReference(textFieldEntity, cursorReference);
                    rint textLabelReference = component.textLabelReference;
                    uint textLabelEntity = world.GetReference(textFieldEntity, textLabelReference);
                    Label textLabel = new(world, textLabelEntity);
                    if (component.editing)
                    {
                        editingAny = true;
                        bool enableCursor = (ticks + textFieldEntity) % 2 == 0;
                        world.SetEnabled(cursorEntity, enableCursor);
                        bool charactersChanged = false;
                        if (lastPressedCharacters != pressedCharacters)
                        {
                            currentCharacters = default;
                            for (uint i = 0; i < pressedCharacters.Length; i++)
                            {
                                char c = pressedCharacters[i];
                                if (!lastPressedCharacters.Contains(c))
                                {
                                    currentCharacters.Append(c);
                                }
                            }

                            lastPressedCharacters = pressedCharacters;
                            charactersChanged = true;
                        }

                        if (currentCharacters != default && (now >= nextPress || charactersChanged))
                        {
                            nextPress = now + TimeSpan.FromMilliseconds(50);
                            for (uint i = 0; i < currentCharacters.Length; i++)
                            {
                                char c = currentCharacters[i];
                                HandleCharacter(world, textLabel, c, settings);
                            }
                        }

                        if (charactersChanged)
                        {
                            nextPress = now + TimeSpan.FromMilliseconds(500);
                        }

                        //update cursor to match position
                        UpdateCursorToMatchPosition(world, textLabel, cursorEntity, settings);
                    }
                    else
                    {
                        world.SetEnabled(cursorEntity, false);
                    }

                    rint highlightReference = component.highlightReference;
                    uint highlightEntity = world.GetReference(textFieldEntity, highlightReference);
                    UpdateHighlightToMatchPosition(world, textLabel, highlightEntity, settings);
                }

                if (!editingAny)
                {
                    settings.TextSelection = default;
                }

                if (anyPointerPressed && !startedEditing)
                {
                    //stop editing because we didnt press a text field
                    settings.TextSelection = default;
                    foreach (var t in textFieldQuery)
                    {
                        ref IsTextField component = ref t.component1;
                        component.editing = false;
                    }
                }
            }
        }

        private static void StartEditing(World world, uint textFieldEntity)
        {
            Pointer pointer = world.GetFirst<Pointer>();
            TextField textField = new Entity(world, textFieldEntity).As<TextField>();
            textField.Editing = true;
            Vector3 worldPosition = textField.AsEntity().As<Transform>().WorldPosition;
            Vector2 pointerPosition = pointer.Position;
            pointerPosition.X -= worldPosition.X;
            pointerPosition.Y -= worldPosition.Y;

            Label textLabel = textField.TextLabel;
            USpan<char> text = textLabel.Text;
            Settings settings = world.GetFirst<Settings>();
            ref TextSelection range = ref settings.TextSelection;
            USpan<char> tempText = stackalloc char[(int)(text.Length + 1)];
            text.CopyTo(tempText);
            tempText[text.Length] = ' ';
            if (textLabel.Font.TryIndexOf(tempText, 32, pointerPosition / 16f, out uint newIndex))
            {
                bool holdingShift = settings.PressedCharacters.Contains(Settings.ShiftCharacter);
                if (holdingShift)
                {
                    uint start = Math.Min(range.start, range.end);
                    uint end = Math.Max(range.start, range.end);
                    uint length = end - start;
                    if (length == 0)
                    {
                        range.start = range.index;
                    }

                    range.end = newIndex;
                }
                else
                {
                    range.start = 0;
                    range.end = 0;
                }

                range.index = newIndex;
            }

            settings.TextSelection = range;
        }

        private static void UpdateCursorToMatchPosition(World world, Label textLabel, uint cursorEntity, Settings settings)
        {
            uint pixelSize = 32;
            Font font = textLabel.Font;
            USpan<char> text = textLabel.Text;

            ref TextSelection range = ref settings.TextSelection;
            USpan<char> textToCursor = text.Slice(0, range.index);
            USpan<char> tempText = stackalloc char[(int)textToCursor.Length + 1];
            textToCursor.CopyTo(tempText);
            tempText[tempText.Length - 1] = ' ';
            Vector2 totalSize = font.CalulcateSize(tempText, pixelSize);

            ref Position cursorPosition = ref world.GetComponent<Position>(cursorEntity);
            LocalToWorld ltw = world.GetComponent<LocalToWorld>(cursorEntity);
            Vector3 worldPosition = ltw.Position + new Vector3(totalSize.X, 0, 0);
            Matrix4x4.Invert(ltw.value, out Matrix4x4 inverseLtw);
            Vector3 localPosition = Vector3.Transform(worldPosition, inverseLtw);
            localPosition.Z = cursorPosition.value.Z;
            cursorPosition.value = localPosition;
        }

        private static void UpdateHighlightToMatchPosition(World world, Label textLabel, uint highlightEntity, Settings settings)
        {
            ref TextSelection range = ref settings.TextSelection;
            uint start = Math.Min(range.start, range.end);
            uint end = Math.Max(range.start, range.end);
            uint length = end - start;
            if (length > 0)
            {
                world.SetEnabled(highlightEntity, true);

                Font font = textLabel.Font;
                USpan<char> text = textLabel.Text;
                USpan<char> textToStart = text.Slice(0, start);
                USpan<char> textToEnd = text.Slice(0, end);
                uint pixelSize = 32;
                Vector2 startPosition = font.CalulcateSize(textToStart, pixelSize);
                Vector2 endPosition = font.CalulcateSize(textToEnd, pixelSize);
                Vector2 totalSize = endPosition - startPosition;

                ref Position highlightPosition = ref world.GetComponent<Position>(highlightEntity);
                highlightPosition.value.X = (startPosition.X * 0.5f) + 2;

                ref Scale highlightScale = ref world.GetComponent<Scale>(highlightEntity);
                highlightScale.value.X = (totalSize.X * 0.5f) + 2; //dividing by 2 because pixelSize/16???
            }
            else
            {
                world.SetEnabled(highlightEntity, false);
            }
        }

        private static bool TryGetPreviousIndex(USpan<char> text, out uint index)
        {
            for (uint i = text.Length - 1; i != uint.MaxValue; i--)
            {
                if (Array.IndexOf(controlCharacters, text[i]) != -1)
                {
                    index = i;
                    return true;
                }
            }

            index = default;
            return false;
        }

        private static bool TryGetNextIndex(USpan<char> text, out uint index)
        {
            for (uint i = 0; i < text.Length; i++)
            {
                if (Array.IndexOf(controlCharacters, text[i]) != -1)
                {
                    index = i;
                    return true;
                }
            }

            index = default;
            return false;
        }

        private static void HandleCharacter(World world, Label textLabel, char character, Settings settings)
        {
            USpan<char> text = textLabel.Text;
            ref TextSelection range = ref settings.TextSelection;
            uint start = Math.Min(range.start, range.end);
            uint end = Math.Max(range.start, range.end);
            uint length = end - start;
            bool shift = settings.PressedCharacters.Contains(Settings.ShiftCharacter);
            bool groupSeparator = settings.PressedCharacters.Contains(Settings.GroupSeparatorCharacter);

            if (character == Settings.GroupSeparatorCharacter || character == Settings.ShiftCharacter || character == Settings.EscapeCharacter)
            {
                //skip
            }
            else if (character == Settings.MoveLeftCharacter)
            {
                if (range.index > 0)
                {
                    if (groupSeparator)
                    {
                        if (shift && length == 0)
                        {
                            range.start = range.index;
                        }

                        if (TryGetPreviousIndex(text.Slice(0, range.index - 1), out uint index))
                        {
                            range.index = index + 1;
                        }
                        else
                        {
                            range.index = 0;
                        }

                        if (shift)
                        {
                            range.end = range.index;
                        }
                    }
                    else
                    {
                        if (shift && length == 0)
                        {
                            range.start = range.index;
                        }

                        range.index--;

                        if (shift)
                        {
                            range.end = range.index;
                        }
                    }

                    Trace.WriteLine(range);
                }

                if (!shift)
                {
                    range.start = 0;
                    range.end = 0;
                }
            }
            else if (character == Settings.MoveRightCharacter)
            {
                if (range.index < text.Length)
                {
                    if (groupSeparator)
                    {
                        if (shift && length == 0)
                        {
                            range.start = range.index;
                        }

                        if (TryGetNextIndex(text.Slice(range.index + 1), out uint index))
                        {
                            range.index += index + 1;
                        }
                        else
                        {
                            range.index = text.Length;
                        }

                        if (shift)
                        {
                            range.end = range.index;
                        }
                    }
                    else
                    {
                        if (shift && length == 0)
                        {
                            range.start = range.index;
                        }

                        range.index++;

                        if (shift)
                        {
                            range.end = range.index;
                        }
                    }

                    Trace.WriteLine(range);
                }

                if (!shift)
                {
                    range.start = 0;
                    range.end = 0;
                }
            }
            else if (character == Settings.StartOfTextCharacter)
            {
                //move cursor to start
                if (shift && length == 0)
                {
                    range.start = range.index;
                }

                range.index = 0;

                if (shift)
                {
                    range.end = range.index;
                }
                else
                {
                    range.start = 0;
                    range.end = 0;
                }
            }
            else if (character == Settings.EndOfTextCharacter)
            {
                //move cursor to end
                if (shift && length == 0)
                {
                    range.start = range.index;
                }

                range.index = text.Length;

                if (shift)
                {
                    range.end = range.index;
                }
                else
                {
                    range.start = 0;
                    range.end = 0;
                }
            }
            else if (character == '\b')
            {
                if (text.Length == 0)
                {
                    return;
                }

                if (length > 0)
                {
                    RemoveSelection(textLabel, ref range);
                }
                else
                {
                    if (range.index == 0)
                    {
                        return;
                    }

                    if (range.index == text.Length)
                    {
                        //remove last char
                        USpan<char> newText = stackalloc char[(int)(text.Length - 1)];
                        text.Slice(0, text.Length - 1).CopyTo(newText);
                        textLabel.SetText(newText);
                    }
                    else if (text.Length == 1)
                    {
                        textLabel.SetText(default(FixedString));
                    }
                    else
                    {
                        //remove char at cursor
                        USpan<char> newText = stackalloc char[(int)(text.Length - 1)];
                        //copy first part
                        text.Slice(0, range.index - 1).CopyTo(newText);
                        //copy remaining
                        text.Slice(range.index).CopyTo(newText.Slice(range.index - 1));
                        textLabel.SetText(newText);
                    }

                    range.index--;
                }
            }
            else
            {
                if (length > 0)
                {
                    RemoveSelection(textLabel, ref range);
                    text = textLabel.Text;
                }

                //write char
                bool holdingShift = settings.PressedCharacters.Contains(Settings.ShiftCharacter);
                if (holdingShift)
                {
                    if (character == '1')
                    {
                        character = '!';
                    }
                    else if (character == '2')
                    {
                        character = '@';
                    }
                    else if (character == '3')
                    {
                        character = '#';
                    }
                    else if (character == '4')
                    {
                        character = '$';
                    }
                    else if (character == '5')
                    {
                        character = '%';
                    }
                    else if (character == '6')
                    {
                        character = '^';
                    }
                    else if (character == '7')
                    {
                        character = '&';
                    }
                    else if (character == '8')
                    {
                        character = '*';
                    }
                    else if (character == '9')
                    {
                        character = '(';
                    }
                    else if (character == '0')
                    {
                        character = ')';
                    }
                    else if (character == '-')
                    {
                        character = '_';
                    }
                    else if (character == '=')
                    {
                        character = '+';
                    }
                    else if (character == '[')
                    {
                        character = '{';
                    }
                    else if (character == ']')
                    {
                        character = '}';
                    }
                    else if (character == '\\')
                    {
                        character = '|';
                    }
                    else if (character == ';')
                    {
                        character = ':';
                    }
                    else if (character == '\'')
                    {
                        character = '"';
                    }
                    else if (character == ',')
                    {
                        character = '<';
                    }
                    else if (character == '.')
                    {
                        character = '>';
                    }
                    else if (character == '/')
                    {
                        character = '?';
                    }
                    else if (character == '`')
                    {
                        character = '~';
                    }
                    else
                    {
                        character = char.ToUpperInvariant(character);
                    }
                }

                //insert character into cursor position
                USpan<char> newText = stackalloc char[(int)(text.Length + 1)];
                USpan<char> firstPart = text.Slice(0, range.index);
                firstPart.CopyTo(newText);
                newText[range.index] = character;
                if (range.index + 1 < newText.Length)
                {
                    USpan<char> secondPart = text.Slice(range.index);
                    secondPart.CopyTo(newText.Slice(range.index + 1));
                }

                textLabel.SetText(newText);
                range.index++;
            }

            settings.TextSelection = range;
        }

        private static void RemoveSelection(Label textLabel, ref TextSelection range)
        {
            uint start = Math.Min(range.start, range.end);
            uint end = Math.Max(range.start, range.end);
            uint length = end - start;
            USpan<char> text = textLabel.Text;
            USpan<char> newText = stackalloc char[(int)(text.Length - length)];

            if (start > 0)
            {
                text.Slice(0, start).CopyTo(newText);
            }

            if (end < text.Length)
            {
                USpan<char> endText = text.Slice(end);
                endText.CopyTo(newText.Slice(start));
            }

            textLabel.SetText(newText);
            range.start = 0;
            range.end = 0;
            range.index = start;
        }
    }
}