using UI.Components;
using UI.Functions;
using Worlds;

namespace UI
{
    public readonly partial struct LabelProcessor : IEntity
    {
        public LabelProcessor(World world, TryProcessLabel function)
        {
            this.world = world;
            value = world.CreateEntity(new IsLabelProcessor(function));
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsLabelProcessor>();
        }

        public static LabelProcessor Create(World world, TryProcessLabel function)
        {
            return new(world, function);
        }
    }
}