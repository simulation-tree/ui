using InteractionKit.Components;
using InteractionKit.Functions;
using InteractionKit.Systems;
using Simulation.Tests;
using System;
using System.Runtime.InteropServices;
using Worlds;

namespace InteractionKit.Tests
{
    public class TriggerTests : SimulationTests
    {
        static TriggerTests()
        {
            TypeLayout.Register<IsTrigger>("IsTrigger");
        }

        protected override void SetUp()
        {
            base.SetUp();
            world.Schema.RegisterComponent<IsTrigger>();
            simulator.AddSystem<InvokeTriggersSystem>();
        }

        [Test]
        public unsafe void CheckTrigger()
        {
            uint triggerA = world.CreateEntity();
            uint triggerB = world.CreateEntity();
            uint triggerC = world.CreateEntity();
            world.AddComponent(triggerA, new IsTrigger(new(&FilterEverythingOut), new(&RemoveByteComponent)));
            world.AddComponent(triggerB, new IsTrigger(new(&FilterEverythingOut), new(&RemoveByteComponent)));
            world.AddComponent(triggerC, new IsTrigger(new(&FilterEverythingOut), new(&RemoveByteComponent)));

            world.AddComponent(triggerA, (byte)1);
            world.AddComponent(triggerB, (byte)2);
            world.AddComponent(triggerC, (int)3);

            simulator.Update(TimeSpan.FromSeconds(0.1f));

            Assert.That(world.ContainsComponent<byte>(triggerA), Is.True);
            Assert.That(world.ContainsComponent<byte>(triggerB), Is.True);

            world.GetComponent<IsTrigger>(triggerA).filter = new(&SelectFirstEntity);
            world.GetComponent<IsTrigger>(triggerB).filter = new(&SelectFirstEntity);
            world.GetComponent<IsTrigger>(triggerC).filter = new(&SelectFirstEntity);

            simulator.Update(TimeSpan.FromSeconds(0.1f));

            Assert.That(world.ContainsComponent<byte>(triggerA), Is.False);
            Assert.That(world.ContainsComponent<byte>(triggerB), Is.True);

            [UnmanagedCallersOnly]
            static void FilterEverythingOut(TriggerFilter.Input input)
            {
                foreach (ref Entity entity in input.Entities)
                {
                    entity = default;
                }
            }

            [UnmanagedCallersOnly]
            static void SelectFirstEntity(TriggerFilter.Input input)
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
