using InteractionKit.Components;
using Simulation;
using Simulation.Functions;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms.Components;
using Unmanaged;
using Worlds;

namespace InteractionKit.Systems
{
    public readonly struct VirtualWindowsScrollViewSystem : ISystem
    {
        private readonly ComponentQuery<IsVirtualWindow> query;

        readonly unsafe StartSystem ISystem.Start => new(&Start);
        readonly unsafe UpdateSystem ISystem.Update => new(&Update);
        readonly unsafe FinishSystem ISystem.Finish => new(&Finish);

        [UnmanagedCallersOnly]
        private static void Start(SystemContainer container, World world)
        {
        }

        [UnmanagedCallersOnly]
        private static void Update(SystemContainer container, World world, TimeSpan delta)
        {
            ref VirtualWindowsScrollViewSystem system = ref container.Read<VirtualWindowsScrollViewSystem>();
            system.Update(world);
        }

        [UnmanagedCallersOnly]
        private static void Finish(SystemContainer container, World world)
        {
            if (container.World == world)
            {
                ref VirtualWindowsScrollViewSystem system = ref container.Read<VirtualWindowsScrollViewSystem>();
                system.CleanUp();
            }
        }

        public VirtualWindowsScrollViewSystem()
        {
            query = new();
        }

        private readonly void CleanUp()
        {
            query.Dispose();
        }

        private readonly void Update(World world)
        {
            query.Update(world, true);
            foreach (var v in query)
            {
                uint virtualWindowEntity = v.entity;
                IsVirtualWindow component = v.Component1;
                VirtualWindow virtualWindow = new Entity(world, virtualWindowEntity).As<VirtualWindow>();
                rint scrollBarReference = component.scrollBarReference;
                rint viewReference = component.viewReference;
                uint scrollBarEntity = world.GetReference(virtualWindowEntity, scrollBarReference);
                uint viewEntity = world.GetReference(virtualWindowEntity, viewReference);
                ScrollBar scrollBar = new Entity(world, scrollBarEntity).As<ScrollBar>();
                View view = new Entity(world, viewEntity).As<View>();
                Entity content = view.Content;
                float minY = float.MaxValue;
                float maxY = float.MinValue;
                USpan<uint> children = content.Children;
                for (uint i = 0; i < children.Length; i++)
                {
                    Entity child = new(world, children[i]);
                    if (child.TryGetComponent(out LocalToWorld ltw))
                    {
                        float y = ltw.Position.Y;
                        if (y < minY)
                        {
                            minY = y;
                        }

                        if (y > maxY)
                        {
                            maxY = y;
                        }
                    }
                }

                Vector2 windowSize = virtualWindow.Size;
                Vector2 axis = scrollBar.Axis;
                if (axis.X > axis.Y)
                {
                    //horizontal
                    float handlePercentageSize = windowSize.X / (maxY - minY);
                    view.ContentSize = new(maxY - minY, windowSize.Y);
                    scrollBar.HandlePercentageSize = handlePercentageSize;
                }
                else if (axis.Y > axis.X)
                {
                    //vertical
                    float handlePercentageSize = windowSize.Y / (maxY - minY);
                    view.ContentSize = new(windowSize.X, maxY - minY);
                    scrollBar.HandlePercentageSize = handlePercentageSize;
                }
                else
                {
                    //both
                }
            }
        }
    }
}