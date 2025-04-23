using System;
using UI.Components;
using Worlds;

namespace UI
{
    public readonly partial struct Token : ISelectable
    {
        public Token(World world, ReadOnlySpan<char> value)
        {
            this.world = world;
            this.value = world.CreateEntity(new IsToken(value));
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsToken>();
        }
    }
}