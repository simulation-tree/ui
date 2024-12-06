using Collections;
using InteractionKit.Components;
using Simulation;
using System;
using Worlds;

namespace InteractionKit.Systems
{
    public readonly partial struct InvokeTriggersSystem : ISystem
    {
        private readonly Array<Entity> currentEntities;
        private readonly Dictionary<int, List<Entity>> entitiesPerTrigger;
        private readonly Dictionary<int, IsTrigger> functions;

        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
            Update(world);
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                foreach (int functionHash in entitiesPerTrigger.Keys)
                {
                    entitiesPerTrigger[functionHash].Dispose();
                }

                functions.Dispose();
                entitiesPerTrigger.Dispose();
                currentEntities.Dispose();
            }
        }

        public InvokeTriggersSystem()
        {
            currentEntities = new();
            entitiesPerTrigger = new();
            functions = new();
        }

        private readonly void Update(World world)
        {
            //find new entities
            ComponentQuery<IsTrigger> invokeQuery = new(world);
            foreach (var x in invokeQuery)
            {
                if (!world.IsEnabled(x.entity)) continue;

                Entity entity = new(world, x.entity);
                ref IsTrigger trigger = ref x.component1;
                int triggerHash = trigger.GetHashCode();
                if (!entitiesPerTrigger.TryGetValue(triggerHash, out List<Entity> entities))
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
                List<Entity> entities = entitiesPerTrigger[functionHash];

                //remove entities that no longer exist
                for (uint i = entities.Count - 1; i != uint.MaxValue; i--)
                {
                    Entity entity = entities[i];
                    if (!world.ContainsEntity(entity))
                    {
                        entities.RemoveAt(i);
                    }
                }

                currentEntities.Length = entities.Count;
                currentEntities.CopyFrom(entities.AsSpan());
                trigger.filter.Invoke(currentEntities.AsSpan(), trigger.identifier);
                for (uint i = 0; i < currentEntities.Length; i++)
                {
                    Entity entity = currentEntities[i];
                    if (entity != default && trigger.callback != default)
                    {
                        trigger.callback.Invoke(entity);
                    }
                }

                entities.Clear();
            }
        }
    }
}