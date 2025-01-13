using InteractionKit.Components;
using System;
using Worlds;

namespace InteractionKit
{
    public static class EntityExtensions
    {
        /// <summary>
        /// Retrieves the canvas that this entity is a descendant of.
        /// <para>
        /// May throw <see cref="InvalidOperationException"/> if this entity is not part of a canvas.
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public static Canvas GetCanvas<T>(this T entity) where T : unmanaged, IEntity
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