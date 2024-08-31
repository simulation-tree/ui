using InteractionKit.Components;
using InteractionKit.Events;
using InteractionKit.Functions;
using Simulation;
using System;
using Unmanaged.Collections;

namespace InteractionKit.Systems
{
    public class InvokeTriggersSystem : SystemBase
    {
        private readonly Query<Trigger> invokeQuery;
        private readonly UnmanagedArray<uint> currentEntities;
        private readonly UnmanagedDictionary<int, UnmanagedList<uint>> entitiesPerTrigger;
        private readonly UnmanagedDictionary<int, (FilterFunction, CallbackFunction)> functions;

        public InvokeTriggersSystem(World world) : base(world)
        {
            invokeQuery = new(world);
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
            invokeQuery.Update();
            foreach (var x in invokeQuery)
            {
                uint entity = x.entity;
                FilterFunction condition = x.Component1.filter;
                CallbackFunction callback = x.Component1.callback;
                int hash = HashCode.Combine(condition, callback);
                if (!entitiesPerTrigger.TryGetValue(hash, out UnmanagedList<uint> entities))
                {
                    entities = new();
                    entitiesPerTrigger.Add(hash, entities);
                    functions.Add(hash, (condition, callback));
                }

                entities.Add(entity);
            }

            foreach (int functionHash in entitiesPerTrigger.Keys)
            {
                (FilterFunction condition, CallbackFunction callback) = functions[functionHash];
                UnmanagedList<uint> entities = entitiesPerTrigger[functionHash];
                currentEntities.Resize(entities.Count);
                currentEntities.CopyFrom(entities.AsSpan());
                condition.Invoke(world, currentEntities.AsSpan());
                for (uint i = 0; i < currentEntities.Length; i++)
                {
                    uint entity = currentEntities[i];
                    if (entity != default)
                    {
                        callback.Invoke(world, entity);
                    }
                }

                entities.Clear();
            }
        }
    }
}