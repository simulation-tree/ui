using Collections;
using InteractionKit.Components;
using Simulation;
using Simulation.Functions;
using System;
using System.Runtime.InteropServices;

namespace InteractionKit.Systems
{
    public readonly struct InvokeTriggersSystem : ISystem
    {
        private readonly ComponentQuery<IsTrigger> invokeQuery;
        private readonly Array<Entity> currentEntities;
        private readonly Dictionary<int, List<Entity>> entitiesPerTrigger;
        private readonly Dictionary<int, IsTrigger> functions;

        readonly unsafe InitializeFunction ISystem.Initialize => new(&Initialize);
        readonly unsafe IterateFunction ISystem.Update => new(&Update);
        readonly unsafe FinalizeFunction ISystem.Finalize => new(&Finalize);

        [UnmanagedCallersOnly]
        private static void Initialize(SystemContainer container, World world)
        {
        }

        [UnmanagedCallersOnly]
        private static void Update(SystemContainer container, World world, TimeSpan delta)
        {
            ref InvokeTriggersSystem system = ref container.Read<InvokeTriggersSystem>();
            system.Update(world);
        }

        [UnmanagedCallersOnly]
        private static void Finalize(SystemContainer container, World world)
        {
            if (container.World == world)
            {
                ref InvokeTriggersSystem system = ref container.Read<InvokeTriggersSystem>();
                system.CleanUp();
            }
        }

        public InvokeTriggersSystem()
        {
            invokeQuery = new();
            currentEntities = new();
            entitiesPerTrigger = new();
            functions = new();
        }

        private void CleanUp()
        {
            foreach (int functionHash in entitiesPerTrigger.Keys)
            {
                entitiesPerTrigger[functionHash].Dispose();
            }

            functions.Dispose();
            entitiesPerTrigger.Dispose();
            currentEntities.Dispose();
            invokeQuery.Dispose();
        }

        private void Update(World world)
        {
            //find new entities
            invokeQuery.Update(world, true);
            foreach (var x in invokeQuery)
            {
                Entity entity = new(world, x.entity);
                IsTrigger trigger = x.Component1;
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