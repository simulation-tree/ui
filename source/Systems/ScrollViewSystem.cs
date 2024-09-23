using InteractionKit.Components;
using InteractionKit.Events;
using Rendering;
using Rendering.Components;
using Simulation;
using System;
using System.Numerics;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit.Systems
{
    public class ScrollViewSystem : SystemBase
    {
        private readonly ComponentQuery<IsView, LocalToWorld> viewQuery;
        private readonly ComponentQuery<IsView, ViewScrollBarLink> scrollBarLinkQuery;
        private readonly ComponentQuery<IsPointer> pointerQuery;

        public ScrollViewSystem(World world) : base(world)
        {
            viewQuery = new();
            scrollBarLinkQuery = new();
            pointerQuery = new();
            Subscribe<InteractionUpdate>(Update);
        }

        public override void Dispose()
        {
            pointerQuery.Dispose();
            scrollBarLinkQuery.Dispose();
            viewQuery.Dispose();
            base.Dispose();
        }

        private void Update(InteractionUpdate update)
        {
            pointerQuery.Update(world);
            scrollBarLinkQuery.Update(world);
            viewQuery.Update(world);
            foreach (var v in viewQuery)
            {
                uint scrollViewEntity = v.entity;
                LocalToWorld ltw = v.Component2;
                rint contentReference = v.Component1.contentReference;
                uint contentEntity = world.GetReference(scrollViewEntity, contentReference);
                Vector3 viewPosition = ltw.Position;
                Vector3 viewScale = ltw.Scale;
                Destination destination = GetCanvas(scrollViewEntity).Camera.Destination;
                if (destination == default || destination.IsDestroyed()) continue;

                LocalToWorld contentLtw = world.GetComponent<LocalToWorld>(contentEntity);
                Vector3 contentScale = contentLtw.Scale;
                if (scrollBarLinkQuery.TryIndexOf(scrollViewEntity, out uint scrollBarIndex))
                {
                    ViewScrollBarLink scrollBarLink = scrollBarLinkQuery[scrollBarIndex].Component2;
                    rint scrollBarReference = scrollBarLink.scrollBarReference;
                    uint scrollBarEntity = world.GetReference(scrollViewEntity, scrollBarReference);
                    ref IsScrollBar scrollBar = ref world.GetComponentRef<IsScrollBar>(scrollBarEntity);

                    //let points scroll the bar
                    foreach (var p in pointerQuery)
                    {
                        uint pointerEntity = p.entity;
                        Vector2 pointerPosition = p.Component1.position;
                        bool hoveredOver = pointerPosition.X >= viewPosition.X && pointerPosition.X <= viewPosition.X + viewScale.X &&
                                           pointerPosition.Y >= viewPosition.Y && pointerPosition.Y <= viewPosition.Y + viewScale.Y;
                        if (hoveredOver)
                        {
                            scrollBar.value += p.Component1.scroll * scrollBar.axis;
                            if (scrollBar.value.X < 0)
                            {
                                scrollBar.value.X = 0;
                            }

                            if (scrollBar.value.Y < 0)
                            {
                                scrollBar.value.Y = 0;
                            }

                            if (scrollBar.value.X > 1)
                            {
                                scrollBar.value.X = 1;
                            }

                            if (scrollBar.value.Y > 1)
                            {
                                scrollBar.value.Y = 1;
                            }
                        }
                    }

                    Vector2 value = scrollBar.value;
                    value.X *= (contentScale.X - viewScale.X);
                    value.Y *= (contentScale.Y - viewScale.Y);
                    v.Component1.value = value;
                }

                (uint width, uint height) destinationSize = destination.Size;
                Vector4 region = new(0, 0, 1, 1);
                region.X = viewPosition.X;
                region.Y = destinationSize.height - (viewPosition.Y + viewScale.Y);
                region.Z = viewScale.X;
                region.W = viewScale.Y;
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

                Vector2 scrollValue = v.Component1.value;
                if (float.IsNaN(scrollValue.X))
                {
                    scrollValue.X = 0f;
                }

                if (float.IsNaN(scrollValue.Y))
                {
                    scrollValue.Y = 0f;
                }

                ref Position position = ref world.GetComponentRef<Position>(contentEntity);
                position.value.X = 1 - scrollValue.X;
                position.value.Y = 1 - scrollValue.Y;
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

                UpdateScissors(child, region);
            }
        }
    }
}