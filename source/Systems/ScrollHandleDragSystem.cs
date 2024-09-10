using InteractionKit.Components;
using InteractionKit.Events;
using Simulation;
using System;
using System.Numerics;
using Transforms.Components;
using Unmanaged.Collections;

namespace InteractionKit.Systems
{
    public class ScrollHandleDragSystem : SystemBase
    {
        private readonly ComponentQuery<IsPointer> pointerQuery;
        private readonly ComponentQuery<IsScrollBar> scrollBarQuery;
        private readonly UnmanagedDictionary<uint, uint> scrollBarHandles;
        private uint currentScrollBar;
        private uint currentPointer;
        private Vector2 dragOffset;

        public ScrollHandleDragSystem(World world) : base(world)
        {
            pointerQuery = new();
            scrollBarQuery = new();
            scrollBarHandles = new();
            Subscribe<InteractionUpdate>(Update);
        }

        public override void Dispose()
        {
            scrollBarHandles.Dispose();
            scrollBarQuery.Dispose();
            pointerQuery.Dispose();
            base.Dispose();
        }

        private void Update(InteractionUpdate update)
        {
            scrollBarQuery.Update(world);
            foreach (var s in scrollBarQuery)
            {
                uint scrollBarEntity = s.entity;
                rint scrollHandleReference = s.Component1.scrollHandleReference;
                uint scrollHandleEntity = world.GetReference(scrollBarEntity, scrollHandleReference);
                scrollBarHandles.Add(scrollHandleEntity, scrollBarEntity);
            }

            pointerQuery.Update(world);
            foreach (var p in pointerQuery)
            {
                uint pointerEntity = p.entity;
                rint selectedReference = p.Component1.selectedReference;
                Vector2 pointerPosition = p.Component1.position;
                bool pressed = p.Component1.HasPrimaryIntent;
                if (pressed && currentPointer == default)
                {
                    uint scrollHandle = world.GetReference(pointerEntity, selectedReference);
                    if (scrollHandle != default && world.ContainsEntity(scrollHandle))
                    {
                        if (scrollBarHandles.TryGetValue(scrollHandle, out uint scrollBar))
                        {
                            LocalToWorld scrollHandleLtw = world.GetComponent<LocalToWorld>(scrollHandle);
                            Vector3 scrollHandlePosition = scrollHandleLtw.Position;
                            Vector2 scrollBarPercentage = default;
                            scrollBarPercentage.X = pointerPosition.X - scrollHandlePosition.X;
                            scrollBarPercentage.Y = pointerPosition.Y - scrollHandlePosition.Y;
                            currentScrollBar = scrollHandle;
                            currentPointer = pointerEntity;
                            dragOffset = scrollBarPercentage;
                        }
                    }
                }
                else if (!pressed)
                {
                    currentPointer = default;
                    currentScrollBar = default;
                }
            }

            if (currentScrollBar != default && world.ContainsEntity(currentScrollBar))
            {
                LocalToWorld scrollBarLtw = world.GetComponent<LocalToWorld>(currentScrollBar);
                Vector2 scrollBarSize = new(scrollBarLtw.Scale.X, scrollBarLtw.Scale.Y);

                uint scrollRegion = world.GetParent(currentScrollBar);
                LocalToWorld scrollRegionLtw = world.GetComponent<LocalToWorld>(scrollRegion);
                Vector3 scrollRegionSize = scrollRegionLtw.Scale;
                Vector3 scrollRegionPosition = scrollRegionLtw.Position;
                Vector2 scrollRegionMin = new(scrollRegionPosition.X, scrollRegionPosition.Y);
                Vector2 scrollRegionMax = scrollRegionMin + new Vector2(scrollRegionSize.X, scrollRegionSize.Y);
                Vector2 scrollAreaMax = scrollRegionMax;
                scrollAreaMax.X -= scrollBarSize.X;
                scrollAreaMax.Y -= scrollBarSize.Y;

                IsPointer pointer = world.GetComponent<IsPointer>(currentPointer);
                Vector2 pointerPosition = pointer.position - dragOffset;
                pointerPosition = Clamp(pointerPosition, scrollRegionMin, scrollAreaMax);
                Vector2 scrollRegionPercentage = default;
                scrollRegionPercentage.X = (pointerPosition.X - scrollRegionMin.X) / (scrollRegionMax.X - scrollRegionMin.X);
                scrollRegionPercentage.Y = (pointerPosition.Y - scrollRegionMin.Y) / (scrollRegionMax.Y - scrollRegionMin.Y);

                //update values to match
                ref Position position = ref world.GetComponentRef<Position>(currentScrollBar);
                position.value.X = scrollRegionPercentage.X;
                position.value.Y = scrollRegionPercentage.Y;

                ref IsScrollBar scrollBar = ref world.GetComponentRef<IsScrollBar>(world.GetParent(scrollRegion));
                scrollBar.value.X = scrollRegionPercentage.X * (1f / (1f - (scrollBarSize.X / scrollRegionSize.X)));
                scrollBar.value.Y = scrollRegionPercentage.Y * (1f / (1f - (scrollBarSize.Y / scrollRegionSize.Y)));
            }

            scrollBarHandles.Clear();
        }

        private static Vector2 Clamp(Vector2 input, Vector2 min, Vector2 max)
        {
            if (input.X < min.X)
            {
                input.X = min.X;
            }
            else if (input.X > max.X)
            {
                input.X = max.X;
            }

            if (input.Y < min.Y)
            {
                input.Y = min.Y;
            }
            else if (input.Y > max.Y)
            {
                input.Y = max.Y;
            }

            return input;
        }
    }
}