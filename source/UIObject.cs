using System;
using UI.Components;
using Unmanaged;
using Worlds;

namespace UI
{
    public readonly partial struct UIObject : IEntity
    {
        public UIObject(World world, ASCIIText256 address, TimeSpan timeout = default)
        {
            this.world = world;
            value = world.CreateEntity(new IsUIObjectRequest(address, timeout));
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddTagType<IsUIObject>();
        }
    }
}