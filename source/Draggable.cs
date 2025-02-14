using UI.Components;
using Worlds;

namespace UI
{
    public readonly partial struct Draggable : IEntity
    {
        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsDraggable>();
        }
    }
}