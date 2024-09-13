using Automations;
using Cameras.Components;
using Data;
using DefaultPresentationAssets;
using Fonts;
using Meshes;
using Rendering;
using Simulation;
using System;
using System.Numerics;
using Textures;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit
{
    public readonly struct InteractiveContext : IDisposable
    {
        public readonly World world;
        public readonly Camera camera;
        public readonly Canvas canvas;
        public readonly Mesh quadMesh;
        public readonly Texture squareTexture;
        public readonly Texture triangleTexture;
        public readonly Font defaultFont;
        public readonly Material squareMaterial;
        public readonly Material triangleMaterial;
        public readonly Material textMaterial;
        public readonly StateMachine controlStateMachine;
        public readonly Automation idleAutomation;
        public readonly Automation selectedAutomation;
        public readonly Automation pressedAutomation;

#if NET
        [Obsolete("Default constructor not available", true)]
        public InteractiveContext()
        {
            throw new NotSupportedException();
        }
#endif

        public InteractiveContext(Canvas canvas)
        {
            USpan<AvailableState> states = stackalloc AvailableState[3];
            states[0] = new("idle");
            states[1] = new("selected");
            states[2] = new("pressed");

            USpan<Transition> transitions = stackalloc Transition[4];
            transitions[0] = new("idle", "selected", "selected", Transition.Condition.GreaterThan, 0f);
            transitions[1] = new("selected", "pressed", "pressed", Transition.Condition.GreaterThan, 0f);
            transitions[2] = new("pressed", "selected", "pressed", Transition.Condition.LessThan, 1f);
            transitions[3] = new("selected", "idle", "selected", Transition.Condition.LessThan, 1f);

            this.canvas = canvas;
            this.camera = canvas.Camera;
            world = canvas.transform.entity.world;
            controlStateMachine = new(world, states, transitions);

            //automations for each state
            USpan<Keyframe<Vector4>> keyframes = stackalloc Keyframe<Vector4>[1];
            keyframes[0] = new(0f, new Vector4(0.8f, 0.8f, 0.8f, 1f));
            idleAutomation = new Automation<Vector4>(world, keyframes);

            keyframes[0] = new(0f, new Vector4(1.4f, 1.4f, 1.4f, 1f));
            selectedAutomation = new Automation<Vector4>(world, keyframes);

            keyframes[0] = new(0f, new Vector4(0.6f, 0.6f, 0.6f, 1f));
            pressedAutomation = new Automation<Vector4>(world, keyframes);

            //create default quad mesh
            quadMesh = new(world);
            Mesh.Collection<Vector3> positions = quadMesh.CreatePositions();
            positions.Add(new(0, 0, 0));
            positions.Add(new(1, 0, 0));
            positions.Add(new(1, 1, 0));
            positions.Add(new(0, 1, 0));
            Mesh.Collection<Vector2> uvs = quadMesh.CreateUVs();
            uvs.Add(new(0, 0));
            uvs.Add(new(1, 0));
            uvs.Add(new(1, 1));
            uvs.Add(new(0, 1));
            Mesh.Collection<Vector3> normals = quadMesh.CreateNormals();
            normals.Add(new(0, 0, 1));
            normals.Add(new(0, 0, 1));
            normals.Add(new(0, 0, 1));
            normals.Add(new(0, 0, 1));
            quadMesh.AddTriangle(0, 1, 2);
            quadMesh.AddTriangle(2, 3, 0);

            squareTexture = new(world, Address.Get<SquareTexture>());
            triangleTexture = new(world, Address.Get<TriangleTexture>());

            //create default coloured unlit material
            squareMaterial = new(world, Address.Get<UnlitTexturedMaterial>());
            squareMaterial.AddPushBinding<Color>();
            squareMaterial.AddPushBinding<LocalToWorld>();
            squareMaterial.AddComponentBinding<CameraProjection>(0, 0, camera.entity);
            squareMaterial.AddTextureBinding(1, 0, squareTexture);

            triangleMaterial = new(world, Address.Get<UnlitTexturedMaterial>());
            triangleMaterial.AddPushBinding<Color>();
            triangleMaterial.AddPushBinding<LocalToWorld>();
            triangleMaterial.AddComponentBinding<CameraProjection>(0, 0, camera.entity);
            triangleMaterial.AddTextureBinding(1, 0, triangleTexture);

            textMaterial = new(world, Address.Get<TextMaterial>());
            textMaterial.AddComponentBinding<CameraProjection>(1, 0, camera.entity);
            textMaterial.AddPushBinding<Color>();
            textMaterial.AddPushBinding<LocalToWorld>();

            defaultFont = new(world, Address.Get<CascadiaMonoFont>());
        }

        public readonly void Dispose()
        {
            controlStateMachine.Destroy();
        }
    }
}