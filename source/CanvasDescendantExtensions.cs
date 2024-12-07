using InteractionKit.Components;
using System;
using Worlds;

namespace InteractionKit
{
    public static class CanvasDescendantExtensions
    {
        public static Canvas GetCanvas<T>(this T entity) where T : unmanaged, ICanvasDescendant
        {
            World world = entity.GetWorld();
            uint parent = world.GetParent(entity.GetEntityValue());
            while (parent != default)
            {
                if (world.ContainsComponent<IsCanvas>(parent))
                {
                    return new Entity(world, parent).As<Canvas>();
                }

                parent = world.GetParent(parent);
            }

            throw new InvalidOperationException($"Entity `{entity}` is not a descendant of a canvas entity");
        }

        public static Canvas GetCanvas(this Entity entity)
        {
            World world = entity.GetWorld();
            uint value = entity.GetEntityValue();
            while (value != default)
            {
                if (world.ContainsComponent<IsCanvas>(value))
                {
                    return new Entity(world, value).As<Canvas>();
                }

                value = world.GetParent(value);
            }

            throw new InvalidOperationException($"Entity `{entity}` is not a descendant of a canvas entity");
        }
    }
}