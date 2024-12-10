using InteractionKit.Components;
using System;
using Worlds;

namespace InteractionKit
{
    public static class CanvasDescendantExtensions
    {
        /// <summary>
        /// Retrieves the canvas that this entity is a descendant of.
        /// <para>
        /// May throw <see cref="InvalidOperationException"/> if this entity is not part of a canvas.
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
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
    }
}