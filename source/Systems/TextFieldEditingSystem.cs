using Fonts;
using InteractionKit.Components;
using InteractionKit.Events;
using Simulation;
using System;
using System.Diagnostics;
using System.Numerics;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit.Systems
{
    public class TextFieldEditingSystem : SystemBase
    {
        private readonly ComponentQuery<IsTextField> query;
        private FixedString lastPressedCharacters;
        private FixedString currentCharacters;
        private DateTime nextPress;

        public TextFieldEditingSystem(World world) : base(world)
        {
            query = new();
            Subscribe<InteractionUpdate>(Update);
        }

        public override void Dispose()
        {
            Unsubscribe<InteractionUpdate>();
            query.Dispose();
            base.Dispose();
        }

        private void Update(InteractionUpdate update)
        {
            Settings settings = world.GetFirst<Settings>();
            FixedString pressedCharacters = default;
            pressedCharacters.CopyFrom(settings.PressedCharacters);

            DateTime now = DateTime.UtcNow;
            USpan<char> pressedBuffer = stackalloc char[(int)FixedString.MaxLength];
            ulong ticks = (ulong)((now - DateTime.UnixEpoch).TotalSeconds * 3f);
            query.Update(world, true);
            foreach (var t in query)
            {
                IsTextField component = t.Component1;
                rint cursorReference = component.cursorReference;
                uint cursorEntity = world.GetReference(t.entity, cursorReference);
                if (component.editing)
                {
                    bool enableCursor = (ticks + t.entity) % 2 == 0;
                    world.SetEnabled(cursorEntity, enableCursor);
                    rint textLabelReference = component.textLabelReference;
                    uint textLabelEntity = world.GetReference(t.entity, textLabelReference);
                    Label textLabel = new(world, textLabelEntity);
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
                        nextPress = now + TimeSpan.FromMilliseconds(60);
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
                    UpdateCursorToMatchPosition(world, t.entity, settings);
                }
                else
                {
                    world.SetEnabled(cursorEntity, false);
                }
            }
        }

        private static void UpdateCursorToMatchPosition(World world, uint textFieldEntity, Settings settings)
        {
            uint pixelSize = 32;
            rint textLabelReference = world.GetComponent<IsTextField>(textFieldEntity).textLabelReference;
            uint textLabelEntity = world.GetReference(textFieldEntity, textLabelReference);
            Label textLabel = new(world, textLabelEntity);
            Font font = textLabel.Font;
            USpan<char> text = textLabel.Text;

            (uint start, uint length) range = settings.EditRange;
            USpan<char> textToCursor = text.Slice(0, range.start);
            USpan<char> tempText = stackalloc char[(int)textToCursor.Length + 1];
            textToCursor.CopyTo(tempText);
            tempText[tempText.Length - 1] = ' ';
            Vector2 totalSize = font.CalulcateSize(tempText, pixelSize);

            rint cursorReference = world.GetComponent<IsTextField>(textFieldEntity).cursorReference;
            uint cursorEntity = world.GetReference(textFieldEntity, cursorReference);
            ref Position cursorPosition = ref world.GetComponentRef<Position>(cursorEntity);
            LocalToWorld ltw = world.GetComponent<LocalToWorld>(cursorEntity);
            Vector3 worldPosition = ltw.Position + new Vector3(totalSize.X, 0, 0);
            Matrix4x4.Invert(ltw.value, out Matrix4x4 inverseLtw);
            Vector3 localPosition = Vector3.Transform(worldPosition, inverseLtw);
            localPosition.Z = cursorPosition.value.Z;
            cursorPosition.value = localPosition;
        }

        private static void HandleCharacter(World world, Label textLabel, char character, Settings settings)
        {
            USpan<char> text = textLabel.Text;
            (uint start, uint length) range = settings.EditRange;
            if (character == Settings.GroupSeparatorCharacter)
            {
                //skip
            }
            else if (character == Settings.MoveLeftCharacter)
            {
                if (range.start > 0)
                {
                    bool groupSeparator = settings.PressedCharacters.Contains(Settings.GroupSeparatorCharacter);
                    if (groupSeparator)
                    {
                        //todo: handle characters other than alphanumeric too, and multiples of the same in a row
                        if (text.Slice(0, range.start - 1).TryLastIndexOf(' ', out uint lastIndex))
                        {
                            range.start = lastIndex + 1;
                        }
                        else
                        {
                            range.start = 0;
                        }
                    }
                    else
                    {
                        range.start--;
                    }

                    settings.EditRange = range;
                }
            }
            else if (character == Settings.MoveRightCharacter)
            {
                if (range.start < text.Length)
                {
                    bool groupSeparator = settings.PressedCharacters.Contains(Settings.GroupSeparatorCharacter);
                    if (groupSeparator)
                    {
                        if (text.Slice(range.start + 1).TryIndexOf(' ', out uint index))
                        {
                            range.start += index + 1;
                        }
                        else
                        {
                            range.start = text.Length;
                        }
                    }
                    else
                    {
                        range.start++;
                    }

                    settings.EditRange = range;
                }
            }
            else if (character == Settings.StartOfTextCharacter)
            {
                //move cursor to start
                bool groupSeparator = settings.PressedCharacters.Contains(Settings.GroupSeparatorCharacter);
                settings.EditRange = (0, 0);
            }
            else if (character == Settings.EndOfTextCharacter)
            {
                //move cursor to end
                bool groupSeparator = settings.PressedCharacters.Contains(Settings.GroupSeparatorCharacter);
                settings.EditRange = (text.Length, 0);
            }
            else if (character == Settings.ShiftCharacter)
            {
                //do nothing
            }
            else if (character == '\b')
            {
                if (text.Length == 0 || range.start == 0)
                {
                    return;
                }

                if (range.start == text.Length)
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
                    text.Slice(0, range.start - 1).CopyTo(newText);
                    //copy remaining
                    text.Slice(range.start).CopyTo(newText.Slice(range.start - 1));
                    textLabel.SetText(newText);
                }

                settings.EditRange = (range.start - 1, range.length);
            }
            else
            {
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
                USpan<char> firstPart = text.Slice(0, range.start);
                firstPart.CopyTo(newText);
                newText[range.start] = character;
                if (range.start + 1 < newText.Length)
                {
                    USpan<char> secondPart = text.Slice(range.start);
                    secondPart.CopyTo(newText.Slice(range.start + 1));
                }

                textLabel.SetText(newText);
                settings.EditRange = (range.start + 1, range.length);
            }
        }
    }
}