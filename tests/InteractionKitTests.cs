using Automations;
using Automations.Components;
using Cameras.Components;
using Data.Components;
using Fonts.Components;
using InteractionKit.Components;
using Meshes;
using Meshes.Components;
using Rendering;
using Rendering.Components;
using Simulation.Tests;
using Textures.Components;
using Transforms.Components;
using Types;

namespace InteractionKit.Tests
{
    public abstract class InteractionKitTests : SimulationTests
    {
        static InteractionKitTests()
        {
            TypeLayout.Register<ComponentMix>();
            TypeLayout.Register<First>();
            TypeLayout.Register<Second>();
            TypeLayout.Register<Result>();
            TypeLayout.Register<FirstFloat>();
            TypeLayout.Register<SecondFloat>();
            TypeLayout.Register<ResultFloat>();
            TypeLayout.Register<FirstVector>();
            TypeLayout.Register<SecondVector>();
            TypeLayout.Register<ResultVector>();
            TypeLayout.Register<AssetReferences>();
            TypeLayout.Register<UISettings>();
            TypeLayout.Register<TextEditState>();
            TypeLayout.Register<MaterialSettings>();
            TypeLayout.Register<IsStateMachine>();
            TypeLayout.Register<AvailableState>();
            TypeLayout.Register<Transition>();
            TypeLayout.Register<KeyframeValue16>();
            TypeLayout.Register<KeyframeTime>();
            TypeLayout.Register<IsAutomation>();
            TypeLayout.Register<IsMesh>();
            TypeLayout.Register<IsTextureRequest>();
            TypeLayout.Register<IsMaterial>();
            TypeLayout.Register<MeshVertexIndex>();
            TypeLayout.Register<MeshVertexPosition>();
            TypeLayout.Register<MeshVertexUV>();
            TypeLayout.Register<MeshVertexColor>();
            TypeLayout.Register<MeshVertexNormal>();
            TypeLayout.Register<IsDataRequest>();
            TypeLayout.Register<MaterialPushBinding>();
            TypeLayout.Register<MaterialComponentBinding>();
            TypeLayout.Register<MaterialTextureBinding>();
            TypeLayout.Register<Color>();
            TypeLayout.Register<LocalToWorld>();
            TypeLayout.Register<CameraMatrices>();
            TypeLayout.Register<CameraSettings>();
            TypeLayout.Register<IsFontRequest>();
            TypeLayout.Register<IsDestination>();
            TypeLayout.Register<DestinationExtension>();
            TypeLayout.Register<IsViewport>();
            TypeLayout.Register<IsCamera>();
            TypeLayout.Register<Position>();
            TypeLayout.Register<Rotation>();
            TypeLayout.Register<Scale>();
            TypeLayout.Register<WorldRotation>();
            TypeLayout.Register<Anchor>();
            TypeLayout.Register<Pivot>();
            TypeLayout.Register<IsTransform>();
            TypeLayout.Register<IsCanvas>();
            TypeLayout.Register<IsTextMeshRequest>();
            TypeLayout.Register<TextCharacter>();
            TypeLayout.Register<LabelCharacter>();
            TypeLayout.Register<ColorTint>();
            TypeLayout.Register<BaseColor>();
            TypeLayout.Register<IsTextRenderer>();
            TypeLayout.Register<IsLabel>();
            TypeLayout.Register<IsSelectable>();
        }

        protected override void SetUp()
        {
            base.SetUp();
            world.Schema.RegisterComponent<ComponentMix>();
            world.Schema.RegisterComponent<First>();
            world.Schema.RegisterComponent<Second>();
            world.Schema.RegisterComponent<Result>();
            world.Schema.RegisterComponent<FirstFloat>();
            world.Schema.RegisterComponent<SecondFloat>();
            world.Schema.RegisterComponent<ResultFloat>();
            world.Schema.RegisterComponent<FirstVector>();
            world.Schema.RegisterComponent<SecondVector>();
            world.Schema.RegisterComponent<ResultVector>();
            world.Schema.RegisterComponent<AssetReferences>();
            world.Schema.RegisterComponent<UISettings>();
            world.Schema.RegisterComponent<TextEditState>();
            world.Schema.RegisterComponent<IsStateMachine>();
            world.Schema.RegisterComponent<IsAutomation>();
            world.Schema.RegisterComponent<IsMesh>();
            world.Schema.RegisterComponent<IsDataRequest>();
            world.Schema.RegisterComponent<IsTextureRequest>();
            world.Schema.RegisterComponent<IsMaterial>();
            world.Schema.RegisterComponent<Color>();
            world.Schema.RegisterComponent<LocalToWorld>();
            world.Schema.RegisterComponent<CameraMatrices>();
            world.Schema.RegisterComponent<CameraSettings>();
            world.Schema.RegisterComponent<IsFontRequest>();
            world.Schema.RegisterComponent<IsViewport>();
            world.Schema.RegisterComponent<IsDestination>();
            world.Schema.RegisterComponent<IsCamera>();
            world.Schema.RegisterComponent<Position>();
            world.Schema.RegisterComponent<Rotation>();
            world.Schema.RegisterComponent<Scale>();
            world.Schema.RegisterComponent<WorldRotation>();
            world.Schema.RegisterComponent<Anchor>();
            world.Schema.RegisterComponent<Pivot>();
            world.Schema.RegisterComponent<IsCanvas>();
            world.Schema.RegisterComponent<IsTextMeshRequest>();
            world.Schema.RegisterComponent<ColorTint>();
            world.Schema.RegisterComponent<BaseColor>();
            world.Schema.RegisterComponent<IsTextRenderer>();
            world.Schema.RegisterComponent<IsLabel>();
            world.Schema.RegisterComponent<IsSelectable>();
            world.Schema.RegisterArrayElement<MaterialSettings>();
            world.Schema.RegisterArrayElement<AvailableState>();
            world.Schema.RegisterArrayElement<Transition>();
            world.Schema.RegisterArrayElement<KeyframeValue16>();
            world.Schema.RegisterArrayElement<KeyframeTime>();
            world.Schema.RegisterArrayElement<MeshVertexIndex>();
            world.Schema.RegisterArrayElement<MeshVertexPosition>();
            world.Schema.RegisterArrayElement<MeshVertexUV>();
            world.Schema.RegisterArrayElement<MeshVertexColor>();
            world.Schema.RegisterArrayElement<MeshVertexNormal>();
            world.Schema.RegisterArrayElement<MaterialPushBinding>();
            world.Schema.RegisterArrayElement<MaterialComponentBinding>();
            world.Schema.RegisterArrayElement<MaterialTextureBinding>();
            world.Schema.RegisterArrayElement<DestinationExtension>();
            world.Schema.RegisterArrayElement<TextCharacter>();
            world.Schema.RegisterArrayElement<LabelCharacter>();
            world.Schema.RegisterTag<IsTransform>();
        }
    }
}
