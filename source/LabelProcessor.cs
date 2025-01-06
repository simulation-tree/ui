using InteractionKit.Components;
using InteractionKit.Functions;
using System;
using Worlds;

namespace InteractionKit
{
    public readonly struct LabelProcessor : IEntity
    {
        private readonly Entity entity;

        readonly uint IEntity.Value => entity.GetEntityValue();
        readonly World IEntity.World => entity.GetWorld();

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsLabelProcessor>();
        }

#if NET
        [Obsolete("Default constructor not supported", true)]
        public LabelProcessor()
        {
            throw new NotSupportedException();
        }
#endif

        public LabelProcessor(World world, TryProcessLabel function)
        {
            entity = new Entity<IsLabelProcessor>(world, new IsLabelProcessor(function));
        }

        public readonly void Dispose()
        {
            entity.Dispose();
        }
    }
}