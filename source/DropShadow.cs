using Cameras;
using Data;
using InteractionKit.Components;
using Meshes.NineSliced;
using Rendering;
using Rendering.Components;
using System;
using Transforms;
using Transforms.Components;
using Worlds;

namespace InteractionKit
{
    public readonly struct DropShadow : IEntity
    {
        private readonly Transform transform;

        readonly uint IEntity.Value => transform.GetEntityValue();
        readonly World IEntity.World => transform.GetWorld();
        readonly Definition IEntity.Definition => new Definition().AddComponentTypes<IsTransform, IsRenderer, IsDropShadow>();

#if NET
        [Obsolete("Default constructor not supported", true)]
        public DropShadow()
        {
            throw new NotSupportedException();
        }
#endif

        public DropShadow(Canvas canvas, Entity foreground)
        {
            Settings settings = canvas.GetSettings();
            World world = canvas.GetWorld();
            Camera camera = canvas.Camera;

            Mesh9Sliced dropShadowMesh = new(world, new(30f), new(0.5f));
            dropShadowMesh.AddComponent(new LocalToWorld());

            Material dropShadowMaterial = settings.GetDropShadowMaterial(camera);
            MeshRenderer dropShadowRenderer = new(world, dropShadowMesh, dropShadowMaterial);
            dropShadowRenderer.AddComponent(new Color(0f, 0f, 0f, 0.5f));

            transform = dropShadowRenderer.AsEntity().Become<Transform>();
            transform.LocalPosition = new(0, 0, -0.02f);
            transform.AddComponent(Anchor.BottomLeft);

            rint dropShadowMeshReference = transform.AddReference(dropShadowMesh);
            rint foregroundReference = transform.AddReference(foreground);
            transform.AddComponent(new IsDropShadow(dropShadowMeshReference, foregroundReference));
            transform.SetParent(canvas);
        }

        public readonly void Dispose()
        {
            transform.Dispose();
        }
    }
}