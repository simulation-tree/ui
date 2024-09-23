using Fonts;
using Fonts.Components;
using InteractionKit.Components;
using InteractionKit.Events;
using Meshes;
using Simulation;
using System;
using System.Numerics;
using Transforms.Components;
using Unmanaged;
using Unmanaged.Collections;

namespace InteractionKit.Systems
{
    public class TextFieldEditingSystem : SystemBase
    {
        private readonly ComponentQuery<IsTextField, InteractiveContext> query;
        private readonly UnmanagedDictionary<InteractiveContext, FixedString> lastPressedCharacters;
        private readonly UnmanagedDictionary<InteractiveContext, FixedString> pressedCharacters;
        private DateTime nextPress;

        public TextFieldEditingSystem(World world) : base(world)
        {
            query = new();
            lastPressedCharacters = new();
            pressedCharacters = new();
            Subscribe<InteractionUpdate>(Update);
        }

        public override void Dispose()
        {
            Unsubscribe<InteractionUpdate>();
            pressedCharacters.Dispose();
            lastPressedCharacters.Dispose();
            query.Dispose();
            base.Dispose();
        }

        private void Update(InteractionUpdate update)
        {
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
                    InteractiveContext context = t.Component2;
                    uint pressedCharCount = context.GetPressedChars(pressedBuffer);
                    FixedString currentPressedChars = new(pressedBuffer.Slice(0, pressedCharCount));
                    if (!this.lastPressedCharacters.ContainsKey(context))
                    {
                        this.lastPressedCharacters.Add(context, default);
                        this.pressedCharacters.Add(context, default);
                    }

                    rint textLabelReference = component.textLabelReference;
                    uint textLabelEntity = world.GetReference(t.entity, textLabelReference);
                    Label textLabel = new(world, textLabelEntity);
                    ref FixedString lastPressedCharacters = ref this.lastPressedCharacters[context];
                    FixedString previousPressedCharacters = lastPressedCharacters;
                    ref FixedString pressedCharacters = ref this.pressedCharacters[context];
                    bool charactersChanged = false;
                    if (lastPressedCharacters != currentPressedChars)
                    {
                        pressedCharacters.Clear();
                        for (uint i = 0; i < currentPressedChars.Length; i++)
                        {
                            char c = currentPressedChars[i];
                            if (!previousPressedCharacters.Contains(c))
                            {
                                pressedCharacters.Append(c);
                            }
                        }

                        lastPressedCharacters = currentPressedChars;
                        charactersChanged = true;
                    }

                    if (pressedCharacters != default && (now >= nextPress || charactersChanged))
                    {
                        bool select = context.SelectMultiple;
                        nextPress = now + TimeSpan.FromMilliseconds(60);
                        for (uint i = 0; i < pressedCharacters.Length; i++)
                        {
                            char c = pressedCharacters[i];
                            if (c == '\b')
                            {
                                Backspace(world, textLabel);
                            }
                            else
                            {
                                AppendCharacter(world, textLabel, c, select);
                            }
                        }
                    }

                    if (charactersChanged)
                    {
                        nextPress = now + TimeSpan.FromMilliseconds(500);
                    }

                    //update cursor to match position
                    UpdateCursorToMatchPosition(world, t.entity);
                }
                else
                {
                    world.SetEnabled(cursorEntity, false);
                }
            }
        }

        private static void UpdateCursorToMatchPosition(World world, uint textFieldEntity)
        {
            int pixelSize = 32;
            int penX = 0;
            int penY = 0;
            rint textLabelReference = world.GetComponent<IsTextField>(textFieldEntity).textLabelReference;
            uint textLabelEntity = world.GetReference(textFieldEntity, textLabelReference);
            Label textLabel = new(world, textLabelEntity);
            USpan<char> text = textLabel.Text;
            Font font = textLabel.Font;
            Vector2 lastPosition = default;
            for (uint i = 0; i < text.Length; i++)
            {
                char c = text[i];
                Glyph glyph = font[c];
                IsGlyph component = world.GetComponent<IsGlyph>(glyph.GetEntityValue());
                (int x, int y) glyphAdvance = component.advance;
                (int x, int y) glyphOffset = component.offset;
                (int x, int y) glyphSize = component.size;
                (int x, int y) glyphBearing = component.bearing;
                float glyphWidth = glyphSize.x / pixelSize;
                float glyphHeight = glyphSize.y / pixelSize;
                Vector3 origin = new(penX + (glyphOffset.x / pixelSize), penY + (glyphOffset.y / pixelSize), 0);
                origin.Y -= (glyphSize.y - glyphBearing.y) / pixelSize;
                Vector2 size = new(glyphWidth, glyphHeight);
                origin /= 64f;
                size /= 64f;
                lastPosition = new Vector2(origin.X, 0) + size;
                penX += glyphAdvance.x / pixelSize;
            }

            rint cursorReference = world.GetComponent<IsTextField>(textFieldEntity).cursorReference;
            uint cursorEntity = world.GetReference(textFieldEntity, cursorReference);
            ref Position cursorPosition = ref world.GetComponentRef<Position>(cursorEntity);
            cursorPosition.value = new Vector3((lastPosition.X * 16) + 4, 0, cursorPosition.value.Z);
        }

        private static void AppendCharacter(World world, Label textLabel, char character, bool shift)
        {
            USpan<char> text = textLabel.Text;
            USpan<char> newText = stackalloc char[(int)(text.Length + 1)];
            text.CopyTo(newText);
            if (shift)
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
                else
                {
                    character = char.ToUpperInvariant(character);
                }
            }

            newText[text.Length] = character;
            textLabel.SetText(newText);
        }

        private static void Backspace(World world, Label textLabel)
        {
            //remove last char
            USpan<char> text = textLabel.Text;
            if (text.Length == 0)
            {
                return;
            }

            USpan<char> newText = stackalloc char[(int)(text.Length - 1)];
            text.Slice(0, text.Length - 1).CopyTo(newText);
            textLabel.SetText(newText);
        }
    }
}