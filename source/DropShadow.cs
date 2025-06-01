using Cameras;
using Data;
using Materials;
using Meshes.NineSliced;
using Rendering;
using Rendering.Components;
using Transforms;
using Transforms.Components;
using UI.Components;
using Worlds;

namespace UI
{
    public readonly partial struct DropShadow : IEntity
    {
        public DropShadow(Canvas canvas, Entity foreground)
        {
            Settings settings = canvas.Settings;
            world = canvas.world;
            Camera camera = canvas.Camera;

            Mesh9Sliced dropShadowMesh = new(world, new(30f), new(0.5f));
            dropShadowMesh.AddComponent(new LocalToWorld());

            Material dropShadowMaterial = settings.GetDropShadowMaterial(camera);
            MeshRenderer dropShadowRenderer = new(world, dropShadowMesh, dropShadowMaterial);
            dropShadowRenderer.AddComponent(new Color(0f, 0f, 0f, 0.5f));

            Transform transform = dropShadowRenderer.Become<Transform>();
            transform.LocalPosition = new(0, 0, Settings.ZScale);
            transform.AddComponent(Anchor.BottomLeft);

            rint dropShadowMeshReference = transform.AddReference(dropShadowMesh);
            rint foregroundReference = transform.AddReference(foreground);
            transform.AddComponent(new IsDropShadow(dropShadowMeshReference, foregroundReference));
            transform.SetParent(canvas);

            value = transform.value;
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsRenderer>();
            archetype.AddComponentType<IsDropShadow>();
            archetype.AddTagType<IsTransform>();
        }
    }
}