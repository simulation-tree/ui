using InteractionKit.Components;
using InteractionKit.Functions;
using InteractionKit.Systems;
using Simulation;
using Simulation.Tests;
using System;
using System.Runtime.InteropServices;

namespace InteractionKit.Tests
{
    public class TriggerTests : SimulationTests
    {
        protected override void SetUp()
        {
            base.SetUp();
            Simulator.AddSystem<InvokeTriggersSystem>();
        }

        [Test]
        public unsafe void CheckTrigger()
        {
            uint triggerA = World.CreateEntity();
            uint triggerB = World.CreateEntity();
            uint triggerC = World.CreateEntity();
            World.AddComponent(triggerA, new IsTrigger(new(&FilterEverythingOut), new(&RemoveByteComponent)));
            World.AddComponent(triggerB, new IsTrigger(new(&FilterEverythingOut), new(&RemoveByteComponent)));
            World.AddComponent(triggerC, new IsTrigger(new(&FilterEverythingOut), new(&RemoveByteComponent)));

            World.AddComponent(triggerA, (byte)1);
            World.AddComponent(triggerB, (byte)2);
            World.AddComponent(triggerC, (int)3);

            Simulator.Update(TimeSpan.FromSeconds(0.1f));

            Assert.That(World.ContainsComponent<byte>(triggerA), Is.True);
            Assert.That(World.ContainsComponent<byte>(triggerB), Is.True);

            World.GetComponentRef<IsTrigger>(triggerA).filter = new(&SelectFirstEntity);
            World.GetComponentRef<IsTrigger>(triggerB).filter = new(&SelectFirstEntity);
            World.GetComponentRef<IsTrigger>(triggerC).filter = new(&SelectFirstEntity);

            Simulator.Update(TimeSpan.FromSeconds(0.1f));

            Assert.That(World.ContainsComponent<byte>(triggerA), Is.False);
            Assert.That(World.ContainsComponent<byte>(triggerB), Is.True);

            [UnmanagedCallersOnly]
            static void FilterEverythingOut(FilterFunction.Input input)
            {
                foreach (ref Entity entity in input.Entities)
                {
                    entity = default;
                }
            }

            [UnmanagedCallersOnly]
            static void SelectFirstEntity(FilterFunction.Input input)
            {
                foreach (ref Entity entity in input.Entities)
                {
                    if (entity.GetEntityValue() != 1)
                    {
                        entity = default;
                    }
                }
            }

            [UnmanagedCallersOnly]
            static void RemoveByteComponent(Entity entity)
            {
                entity.RemoveComponent<byte>();
            }
        }
    }
}
