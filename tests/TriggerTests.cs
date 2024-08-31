using InteractionKit.Components;
using InteractionKit.Events;
using InteractionKit.Functions;
using InteractionKit.Systems;
using Simulation;
using System.Runtime.InteropServices;
using Unmanaged;

namespace InteractionKit.Tests
{
    public class TriggerTests
    {
        [TearDown]
        public void CleanUp()
        {
            Allocations.ThrowIfAny();
        }

        private void Simulate(World world)
        {
            world.Submit(new InteractionUpdate());
            world.Poll();
        }

        [Test]
        public unsafe void CheckTrigger()
        {
            using World world = new();
            using InvokeTriggersSystem invokeTriggers = new(world);

            uint triggerA = world.CreateEntity();
            uint triggerB = world.CreateEntity();
            uint triggerC = world.CreateEntity();
            world.AddComponent(triggerA, new Trigger(new(&BadCondition), new(&Callback)));
            world.AddComponent(triggerB, new Trigger(new(&BadCondition), new(&Callback)));
            world.AddComponent(triggerC, new Trigger(new(&BadCondition), new(&Callback)));

            world.AddComponent(triggerA, (byte)1);
            world.AddComponent(triggerB, (byte)2);
            world.AddComponent(triggerC, (int)3);

            Simulate(world);

            Assert.That(world.ContainsComponent<byte>(triggerA), Is.True);
            Assert.That(world.ContainsComponent<byte>(triggerB), Is.True);

            world.GetComponentRef<Trigger>(triggerA).filter = new(&SelectFirstEntity);
            world.GetComponentRef<Trigger>(triggerB).filter = new(&SelectFirstEntity);
            world.GetComponentRef<Trigger>(triggerC).filter = new(&SelectFirstEntity);
            Simulate(world);

            Assert.That(world.ContainsComponent<byte>(triggerA), Is.False);
            Assert.That(world.ContainsComponent<byte>(triggerB), Is.True);

            [UnmanagedCallersOnly]
            static void BadCondition(FilterFunction.Input input)
            {
                foreach (ref uint entity in input.Entities)
                {
                    entity = default;
                }
            }

            [UnmanagedCallersOnly]
            static void SelectFirstEntity(FilterFunction.Input input)
            {
                foreach (ref uint entity in input.Entities)
                {
                    if ((uint)entity != 1)
                    {
                        entity = default;
                    }
                }
            }

            [UnmanagedCallersOnly]
            static void Callback(World world, uint entity)
            {
                world.RemoveComponent<byte>(entity);
            }
        }
    }
}
