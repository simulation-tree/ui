using InteractionKit.Components;
using InteractionKit.Events;
using Simulation;
using Unmanaged.Collections;

namespace InteractionKit.Systems
{
    public class InvokeTriggersSystem : SystemBase
    {
        private readonly ComponentQuery<IsTrigger> invokeQuery;
        private readonly UnmanagedArray<uint> currentEntities;
        private readonly UnmanagedDictionary<int, UnmanagedList<uint>> entitiesPerTrigger;
        private readonly UnmanagedDictionary<int, IsTrigger> functions;

        public InvokeTriggersSystem(World world) : base(world)
        {
            invokeQuery = new();
            currentEntities = new();
            entitiesPerTrigger = new();
            functions = new();
            Subscribe<InteractionUpdate>(Update);
        }

        public override void Dispose()
        {
            foreach (int functionHash in entitiesPerTrigger.Keys)
            {
                entitiesPerTrigger[functionHash].Dispose();
            }

            functions.Dispose();
            entitiesPerTrigger.Dispose();
            currentEntities.Dispose();
            invokeQuery.Dispose();
            base.Dispose();
        }

        private void Update(InteractionUpdate update)
        {
            invokeQuery.Update(world);
            foreach (var x in invokeQuery)
            {
                uint entity = x.entity;
                IsTrigger trigger = x.Component1;
                int triggerHash = trigger.GetHashCode();
                if (!entitiesPerTrigger.TryGetValue(triggerHash, out UnmanagedList<uint> entities))
                {
                    entities = new();
                    entitiesPerTrigger.Add(triggerHash, entities);
                    functions.Add(triggerHash, trigger);
                }

                entities.Add(entity);
            }

            foreach (int functionHash in entitiesPerTrigger.Keys)
            {
                IsTrigger trigger = functions[functionHash];
                UnmanagedList<uint> entities = entitiesPerTrigger[functionHash];
                currentEntities.Resize(entities.Count);
                currentEntities.CopyFrom(entities.AsSpan());
                trigger.filter.Invoke(world, currentEntities.AsSpan(), trigger.identifier);
                for (uint i = 0; i < currentEntities.Length; i++)
                {
                    uint entity = currentEntities[i];
                    if (entity != default)
                    {
                        trigger.callback.Invoke(world, entity);
                    }
                }

                entities.Clear();
            }
        }
    }
}