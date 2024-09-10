using InteractionKit.Components;
using InteractionKit.Events;
using Rendering;
using Rendering.Components;
using Simulation;
using System;
using System.Diagnostics;
using System.Numerics;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit.Systems
{
    public class ScrollViewSystem : SystemBase
    {
        private readonly ComponentQuery<IsView, LocalToWorld> scrollViewQuery;
        private readonly ComponentQuery<IsView, ViewScrollBarLink> scrollBarQuery;

        public ScrollViewSystem(World world) : base(world)
        {
            scrollViewQuery = new();
            scrollBarQuery = new();
            Subscribe<InteractionUpdate>(Update);
        }

        public override void Dispose()
        {
            scrollBarQuery.Dispose();
            scrollViewQuery.Dispose();
            base.Dispose();
        }

        private void Update(InteractionUpdate update)
        {
            scrollBarQuery.Update(world);
            scrollViewQuery.Update(world);
            foreach (var s in scrollViewQuery)
            {
                uint scrollViewEntity = s.entity;
                LocalToWorld ltw = s.Component2;
                rint contentReference = s.Component1.contentReference;
                uint contentEntity = world.GetReference(scrollViewEntity, contentReference);
                Vector3 scrollViewPosition = ltw.Position;
                Vector3 scrollViewScale = ltw.Scale;
                Destination destination = GetCanvas(scrollViewEntity).Camera.Destination;
                if (destination == default || destination.IsDestroyed()) continue;

                LocalToWorld contentLtw = world.GetComponent<LocalToWorld>(contentEntity);
                Vector3 contentScale = contentLtw.Scale;
                if (scrollBarQuery.TryIndexOf(scrollViewEntity, out uint scrollBarIndex))
                {
                    ViewScrollBarLink scrollBarLink = scrollBarQuery[scrollBarIndex].Component2;
                    rint scrollBarReference = scrollBarLink.scrollBarReference;
                    uint scrollBarEntity = world.GetReference(scrollViewEntity, scrollBarReference);
                    IsScrollBar scrollBar = world.GetComponent<IsScrollBar>(scrollBarEntity);
                    Vector2 value = scrollBar.value;
                    value.X *= (contentScale.X - scrollViewScale.X);
                    value.Y *= (contentScale.Y - scrollViewScale.Y);
                    s.Component1.value = value;
                }

                (uint width, uint height) destinationSize = destination.Size;
                Vector4 region = new(0, 0, 1, 1);
                region.X = scrollViewPosition.X;
                region.Y = destinationSize.height - (scrollViewPosition.Y + scrollViewScale.Y);
                region.Z = scrollViewScale.X;
                region.W = scrollViewScale.Y;
                if (region.X < 0)
                {
                    region.X = 0;
                }

                if (region.Y < 0)
                {
                    region.Y = 0;
                }

                if (region.Z > destinationSize.width)
                {
                    region.Z = destinationSize.width;
                }

                if (region.W > destinationSize.height)
                {
                    region.W = destinationSize.height;
                }

                UpdateScissors(contentEntity, region);

                Vector2 scrollValue = s.Component1.value;
                if (float.IsNaN(scrollValue.X))
                {
                    scrollValue.X = 0f;
                }

                if (float.IsNaN(scrollValue.Y))
                {
                    scrollValue.Y = 0f;
                }

                Vector2 viewPosition = scrollValue;
                viewPosition.X = 1 - viewPosition.X;
                viewPosition.Y = 1 - viewPosition.Y;
                ref Position position = ref world.GetComponentRef<Position>(contentEntity);
                position.value = new(viewPosition, position.value.Z);
            }
        }

        private Canvas GetCanvas(uint entity)
        {
            while (entity != default)
            {
                if (world.ContainsComponent<IsCanvas>(entity))
                {
                    return new Entity(world, entity).As<Canvas>();
                }

                entity = world.GetParent(entity);
            }

            throw new InvalidOperationException($"Entity `{entity}` is not a descendant of a canvas");
        }

        private void UpdateScissors(uint contentEntity, Vector4 region)
        {
            USpan<uint> contentChildren = world.GetChildren(contentEntity);
            foreach (uint child in contentChildren)
            {
                if (world.ContainsComponent<IsRenderer>(child))
                {
                    ref RendererScissor scissor = ref world.TryGetComponentRef<RendererScissor>(child, out bool contains);
                    if (!contains)
                    {
                        world.AddComponent(child, new RendererScissor(region));
                    }
                    else
                    {
                        scissor.region = region;
                    }
                }
            }
        }
    }
}