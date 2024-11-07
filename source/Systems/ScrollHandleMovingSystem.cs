using Collections;
using InteractionKit.Components;
using Simulation;
using Simulation.Functions;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms.Components;

namespace InteractionKit.Systems
{
    public struct ScrollHandleMovingSystem : ISystem
    {
        private readonly ComponentQuery<IsPointer> pointerQuery;
        private readonly ComponentQuery<IsScrollBar> scrollBarQuery;
        private readonly List<uint> scrollHandleEntities;
        private readonly List<uint> scrollRegionEntities;
        private uint currentScrollHandleEntity;
        private uint currentPointer;
        private Vector2 dragOffset;

        readonly unsafe InitializeFunction ISystem.Initialize => new(&Initialize);
        readonly unsafe IterateFunction ISystem.Iterate => new(&Update);
        readonly unsafe FinalizeFunction ISystem.Finalize => new(&Finalize);

        [UnmanagedCallersOnly]
        private static void Initialize(SystemContainer container, World world)
        {
        }

        [UnmanagedCallersOnly]
        private static void Update(SystemContainer container, World world, TimeSpan delta)
        {
            ref ScrollHandleMovingSystem system = ref container.Read<ScrollHandleMovingSystem>();
            system.Update(world);
        }

        [UnmanagedCallersOnly]
        private static void Finalize(SystemContainer container, World world)
        {
            if (container.World == world)
            {
                ref ScrollHandleMovingSystem system = ref container.Read<ScrollHandleMovingSystem>();
                system.CleanUp();
            }
        }

        public ScrollHandleMovingSystem()
        {
            pointerQuery = new();
            scrollBarQuery = new();
            scrollHandleEntities = new();
            scrollRegionEntities = new();
        }

        private void CleanUp()
        {
            scrollRegionEntities.Dispose();
            scrollHandleEntities.Dispose();
            scrollBarQuery.Dispose();
            pointerQuery.Dispose();
        }

        private void Update(World world)
        {
            scrollBarQuery.Update(world, true);
            foreach (var s in scrollBarQuery)
            {
                uint scrollBarEntity = s.entity;
                rint scrollHandleReference = s.Component1.scrollHandleReference;
                uint scrollHandleEntity = world.GetReference(scrollBarEntity, scrollHandleReference);
                uint scrollRegionEntity = world.GetParent(scrollHandleEntity);
                scrollHandleEntities.Add(scrollHandleEntity);
                scrollRegionEntities.Add(scrollRegionEntity);
            }

            pointerQuery.Update(world);
            foreach (var p in pointerQuery)
            {
                uint pointerEntity = p.entity;
                rint hoveringOverReference = p.Component1.hoveringOverReference;
                uint hoveringOverEntity = hoveringOverReference == default ? default : world.GetReference(pointerEntity, hoveringOverReference);
                Vector2 pointerPosition = p.Component1.position;
                Vector2 pointerScroll = p.Component1.scroll;
                bool pressed = p.Component1.HasPrimaryIntent;
                if (pressed && currentPointer == default)
                {
                    if (scrollHandleEntities.Contains(hoveringOverEntity))
                    {
                        //start dragging
                        LocalToWorld scrollHandleLtw = world.GetComponent<LocalToWorld>(hoveringOverEntity);
                        Vector3 scrollHandlePosition = scrollHandleLtw.Position;
                        Vector2 scrollBarPercentage = default;
                        scrollBarPercentage.X = pointerPosition.X - scrollHandlePosition.X;
                        scrollBarPercentage.Y = pointerPosition.Y - scrollHandlePosition.Y;
                        currentScrollHandleEntity = hoveringOverEntity;
                        currentPointer = pointerEntity;
                        dragOffset = scrollBarPercentage;
                    }
                    else if (scrollRegionEntities.Contains(hoveringOverEntity))
                    {
                        //teleport scroll value
                        uint scrollBarEntity = world.GetParent(hoveringOverEntity);
                        rint scrollHandleReference = world.GetComponent<IsScrollBar>(scrollBarEntity).scrollHandleReference;
                        uint scrollHandleEntity = world.GetReference(scrollBarEntity, scrollHandleReference);
                        Vector2 value = GetScrollBarValue(world, scrollHandleEntity, pointerPosition);
                        ref IsScrollBar component = ref world.GetComponentRef<IsScrollBar>(scrollBarEntity);
                        component.value = value;
                    }
                }
                else if (!pressed)
                {
                    if (scrollHandleEntities.Contains(hoveringOverEntity))
                    {
                        //move scrollbar by scrolling when hovering over the handle
                        uint scrollRegionEntity = world.GetParent(hoveringOverEntity);
                        uint scrollBarEntity = world.GetParent(scrollRegionEntity);
                        ref IsScrollBar component = ref world.GetComponentRef<IsScrollBar>(scrollBarEntity);
                        component.value += pointerScroll * component.axis;
                        component.value = Clamp(component.value, Vector2.Zero, Vector2.One);
                    }
                    else if (scrollRegionEntities.Contains(hoveringOverEntity))
                    {
                        //move when hovering over the region
                        uint scrollBarEntity = world.GetParent(hoveringOverEntity);
                        ref IsScrollBar component = ref world.GetComponentRef<IsScrollBar>(scrollBarEntity);
                        component.value += pointerScroll * component.axis;
                        component.value = Clamp(component.value, Vector2.Zero, Vector2.One);
                    }

                    currentPointer = default;
                    currentScrollHandleEntity = default;
                }
            }

            //move scrollbar value by dragging
            if (scrollHandleEntities.Contains(currentScrollHandleEntity))
            {
                IsPointer pointer = world.GetComponent<IsPointer>(currentPointer);
                Vector2 pointerPosition = pointer.position - dragOffset;
                Vector2 value = GetScrollBarValue(world, currentScrollHandleEntity, pointerPosition);

                uint scrollRegionEntity = world.GetParent(currentScrollHandleEntity);
                uint scrollBarEntity = world.GetParent(scrollRegionEntity);
                ref IsScrollBar component = ref world.GetComponentRef<IsScrollBar>(scrollBarEntity);
                component.value = value;
            }

            //update scroll bar visuals
            foreach (var s in scrollBarQuery)
            {
                rint scrollHandleReference = s.Component1.scrollHandleReference;
                uint scrollHandleEntity = world.GetReference(s.entity, scrollHandleReference);
                Vector2 value = s.Component1.value;
                UpdateScrollBarVisual(world, scrollHandleEntity, value);
            }

            scrollHandleEntities.Clear();
            scrollRegionEntities.Clear();
        }

        private static Vector2 GetScrollBarValue(World world, uint scrollHandleEntity, Vector2 pointerPosition)
        {
            uint scrollRegionEntity = world.GetParent(scrollHandleEntity);
            uint scrollBarEntity = world.GetParent(scrollRegionEntity);
            IsScrollBar component = world.GetComponent<IsScrollBar>(scrollBarEntity);
            LocalToWorld scrollHandleLtw = world.GetComponent<LocalToWorld>(scrollHandleEntity);
            Vector3 scrollHandleSize = scrollHandleLtw.Scale;
            Vector2 axis = component.axis;

            LocalToWorld scrollRegionLtw = world.GetComponent<LocalToWorld>(scrollRegionEntity);
            Vector3 scrollRegionSize = scrollRegionLtw.Scale;
            Vector3 scrollRegionPosition = scrollRegionLtw.Position;
            Vector2 scrollRegionMin = new(scrollRegionPosition.X, scrollRegionPosition.Y);
            Vector2 scrollRegionMax = scrollRegionMin + new Vector2(scrollRegionSize.X, scrollRegionSize.Y);
            scrollRegionMax.X -= scrollHandleSize.X * axis.X;
            scrollRegionMax.Y -= scrollHandleSize.Y * axis.Y;

            pointerPosition = Clamp(pointerPosition, scrollRegionMin, scrollRegionMax);
            Vector2 scrollRegionPercentage = default;
            scrollRegionPercentage.X = (pointerPosition.X - scrollRegionMin.X) / (scrollRegionMax.X - scrollRegionMin.X);
            scrollRegionPercentage.Y = (pointerPosition.Y - scrollRegionMin.Y) / (scrollRegionMax.Y - scrollRegionMin.Y);

            Vector2 value = default;
            value.X = scrollRegionPercentage.X * axis.X;
            value.Y = scrollRegionPercentage.Y * axis.Y;
            return value;
        }

        private static void UpdateScrollBarVisual(World world, uint scrollHandleEntity, Vector2 value)
        {
            uint scrollRegionEntity = world.GetParent(scrollHandleEntity);
            uint scrollBarEntity = world.GetParent(scrollRegionEntity);
            Vector2 axis = world.GetComponent<IsScrollBar>(scrollBarEntity).axis;

            LocalToWorld scrollBarLtw = world.GetComponent<LocalToWorld>(scrollHandleEntity);
            LocalToWorld scrollRegionLtw = world.GetComponent<LocalToWorld>(scrollRegionEntity);
            Vector3 scrollBarSize = scrollBarLtw.Scale;
            Vector3 scrollRegionSize = scrollRegionLtw.Scale;

            value.X *= 1 - ((scrollBarSize.X / scrollRegionSize.X) * axis.X);
            value.Y *= 1 - ((scrollBarSize.Y / scrollRegionSize.Y) * axis.Y);

            ref Position position = ref world.GetComponentRef<Position>(scrollHandleEntity);
            position.value.X = value.X;// * scrollAreaMax.X;
            position.value.Y = value.Y;// * scrollAreaMax.Y;
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