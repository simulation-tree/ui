using InteractionKit.Components;
using Simulation;
using System;
using System.Numerics;
using Transforms.Components;
using Unmanaged;
using Worlds;

namespace InteractionKit.Systems
{
    public readonly partial struct VirtualWindowsScrollViewSystem : ISystem
    {
        private readonly ComponentQuery<IsVirtualWindow> query;

        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
            Update(world);
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                CleanUp();
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