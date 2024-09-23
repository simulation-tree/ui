using Automations;
using Cameras.Components;
using Data;
using DefaultPresentationAssets;
using Fonts;
using Meshes;
using Rendering;
using Simulation;
using System;
using System.Diagnostics;
using System.Numerics;
using Textures;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct InteractiveContext : IDisposable, IEquatable<InteractiveContext>
    {
        public const char SelectMultipleCharacter = (char)16;

        public readonly Allocation allocation;

        public unsafe readonly bool SelectMultiple
        {
            get
            {
                ref State state = ref StateValue;
                byte pressedCount = state.pressedCount;
                for (byte i = 0; i < pressedCount; i++)
                {
                    if (state.pressedCharacters[i] == SelectMultipleCharacter)
                    {
                        return true;
                    }
                }

                return false;
            }
            set
            {
                ref State state = ref StateValue;
                byte pressedCount = state.pressedCount;
                for (byte i = 0; i < pressedCount; i++)
                {
                    if (state.pressedCharacters[i] == SelectMultipleCharacter)
                    {
                        if (value)
                        {
                            return;
                        }
                        else
                        {
                            state.pressedCharacters[i] = state.pressedCharacters[pressedCount - 1];
                            state.pressedCount--;
                            return;
                        }
                    }
                }

                if (value)
                {
                    state.pressedCharacters[pressedCount] = (byte)SelectMultipleCharacter;
                    state.pressedCount++;
                }
            }
        }

        public readonly Camera Camera => StateValue.camera;
        public readonly Canvas Canvas => StateValue.canvas;
        public readonly Mesh QuadMesh => StateValue.quadMesh;
        public readonly Texture SquareTexture => StateValue.squareTexture;
        public readonly Texture TriangleTexture => StateValue.triangleTexture;
        public readonly Font DefaultFont => StateValue.defaultFont;
        public readonly Material SquareMaterial => StateValue.squareMaterial;
        public readonly Material TriangleMaterial => StateValue.triangleMaterial;
        public readonly Material TextMaterial => StateValue.textMaterial;
        public readonly StateMachine ControlStateMachine => StateValue.controlStateMachine;
        public readonly Automation IdleAutomation => StateValue.idleAutomation;
        public readonly Automation SelectedAutomation => StateValue.selectedAutomation;
        public readonly Automation PressedAutomation => StateValue.pressedAutomation;

        private ref State StateValue => ref allocation.Read<State>();

#if NET
        [Obsolete("Default constructor not available", true)]
        public InteractiveContext()
        {
            throw new NotSupportedException();
        }
#endif

        public InteractiveContext(Canvas canvas)
        {
            World world = canvas.GetWorld();
            State state = new();

            USpan<AvailableState> states = stackalloc AvailableState[3];
            states[0] = new("idle");
            states[1] = new("selected");
            states[2] = new("pressed");

            USpan<Transition> transitions = stackalloc Transition[4];
            transitions[0] = new("idle", "selected", "selected", Transition.Condition.GreaterThan, 0f);
            transitions[1] = new("selected", "pressed", "pressed", Transition.Condition.GreaterThan, 0f);
            transitions[2] = new("pressed", "selected", "pressed", Transition.Condition.LessThan, 1f);
            transitions[3] = new("selected", "idle", "selected", Transition.Condition.LessThan, 1f);

            state.canvas = canvas;
            state.camera = canvas.Camera;
            state.controlStateMachine = new(world, states, transitions);

            //automations for each state
            USpan<Keyframe<Vector4>> keyframes = stackalloc Keyframe<Vector4>[1];
            keyframes[0] = new(0f, new Vector4(0.8f, 0.8f, 0.8f, 1f));
            state.idleAutomation = new Automation<Vector4>(world, keyframes);

            keyframes[0] = new(0f, new Vector4(1.4f, 1.4f, 1.4f, 1f));
            state.selectedAutomation = new Automation<Vector4>(world, keyframes);

            keyframes[0] = new(0f, new Vector4(0.6f, 0.6f, 0.6f, 1f));
            state.pressedAutomation = new Automation<Vector4>(world, keyframes);

            //create default quad mesh
            state.quadMesh = new(world);
            USpan<Vector3> positions = state.quadMesh.CreatePositions(4);
            positions[0] = new(0, 0, 0);
            positions[1] = new(1, 0, 0);
            positions[2] = new(1, 1, 0);
            positions[3] = new(0, 1, 0);
            USpan<Vector2> uvs = state.quadMesh.CreateUVs(4);
            uvs[0] = new(0, 0);
            uvs[1] = new(1, 0);
            uvs[2] = new(1, 1);
            uvs[3] = new(0, 1);
            USpan<Vector3> normals = state.quadMesh.CreateNormals(4);
            normals[0] = new(0, 0, 1);
            normals[1] = new(0, 0, 1);
            normals[2] = new(0, 0, 1);
            normals[3] = new(0, 0, 1);
            state.quadMesh.AddTriangle(0, 1, 2);
            state.quadMesh.AddTriangle(2, 3, 0);

            state.squareTexture = new(world, Address.Get<SquareTexture>());
            state.triangleTexture = new(world, Address.Get<TriangleTexture>());

            //create default coloured unlit material
            state.squareMaterial = new(world, Address.Get<UnlitTexturedMaterial>());
            state.squareMaterial.AddPushBinding<Color>();
            state.squareMaterial.AddPushBinding<LocalToWorld>();
            state.squareMaterial.AddComponentBinding<CameraProjection>(0, 0, state.camera);
            state.squareMaterial.AddTextureBinding(1, 0, state.squareTexture);

            state.triangleMaterial = new(world, Address.Get<UnlitTexturedMaterial>());
            state.triangleMaterial.AddPushBinding<Color>();
            state.triangleMaterial.AddPushBinding<LocalToWorld>();
            state.triangleMaterial.AddComponentBinding<CameraProjection>(0, 0, state.camera);
            state.triangleMaterial.AddTextureBinding(1, 0, state.triangleTexture);

            state.textMaterial = new(world, Address.Get<TextMaterial>());
            state.textMaterial.AddComponentBinding<CameraProjection>(1, 0, state.camera);
            state.textMaterial.AddPushBinding<Color>();
            state.textMaterial.AddPushBinding<LocalToWorld>();

            state.defaultFont = new(world, Address.Get<CascadiaMonoFont>());
            this.allocation = Allocation.Create(state);
        }

        public readonly void Dispose()
        {
            allocation.Dispose();
        }

        public unsafe readonly uint GetPressedChars(USpan<char> characterBuffer)
        {
            State state = StateValue;
            byte count = state.pressedCount;
            if (count == 0)
            {
                return 0;
            }

            uint length = 0;
            for (byte i = 0; i < count; i++)
            {
                byte pressedCharacter = state.pressedCharacters[i];
                if (pressedCharacter == SelectMultipleCharacter)
                {
                    continue;
                }

                characterBuffer[length++] = (char)pressedCharacter;
            }

            return length;
        }

        public unsafe readonly void SetPressedChars(USpan<char> characters)
        {
            ThrowIfPressedCharCountExceedsMax(characters.Length);
            bool selectMultiple = SelectMultiple;
            ref State state = ref StateValue;
            byte count = (byte)characters.Length;
            state.pressedCount = count;
            for (byte i = 0; i < count; i++)
            {
                state.pressedCharacters[i] = (byte)characters[i];
            }

            if (selectMultiple)
            {
                state.pressedCharacters[count] = (byte)SelectMultipleCharacter;
                state.pressedCount++;
            }
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfPressedCharCountExceedsMax(uint length)
        {
            if (length > FixedString.MaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "The length of the pressed keys must be less than or equal to 32.");
            }
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is InteractiveContext context && Equals(context);
        }

        public readonly bool Equals(InteractiveContext other)
        {
            return allocation.Equals(other.allocation);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(allocation);
        }

        internal unsafe struct State
        {
            public fixed byte pressedCharacters[(int)FixedString.MaxLength];
            public byte pressedCount;
            public Camera camera;
            public Canvas canvas;
            public Mesh quadMesh;
            public Texture squareTexture;
            public Texture triangleTexture;
            public Font defaultFont;
            public Material squareMaterial;
            public Material triangleMaterial;
            public Material textMaterial;
            public StateMachine controlStateMachine;
            public Automation idleAutomation;
            public Automation selectedAutomation;
            public Automation pressedAutomation;
        }

        public static bool operator ==(InteractiveContext left, InteractiveContext right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InteractiveContext left, InteractiveContext right)
        {
            return !(left == right);
        }
    }
}