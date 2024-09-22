using InteractionKit.Components;
using InteractionKit.Events;
using Simulation;
using System.Numerics;
using Transforms.Components;
using Unmanaged;

namespace InteractionKit.Systems
{
    public class VirtualWindowsScrollViewSystem : SystemBase
    {
        private readonly ComponentQuery<IsVirtualWindow> query;

        public VirtualWindowsScrollViewSystem(World world) : base(world)
        {
            query = new();
            Subscribe<InteractionUpdate>(Update);
        }

        public override void Dispose()
        {
            Unsubscribe<InteractionUpdate>();
            query.Dispose();
            base.Dispose();
        }

        private void Update(InteractionUpdate update)
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