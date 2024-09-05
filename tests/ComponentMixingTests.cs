using InteractionKit.Components;
using InteractionKit.Events;
using InteractionKit.Systems;
using Simulation;
using System.Numerics;
using Unmanaged;

namespace InteractionKit.Tests
{
    public class ComponentMixingTests
    {
        [TearDown]
        public void CleanUp()
        {
            Allocations.ThrowIfAny();
        }

        private void Simulate(World world)
        {
            world.Submit(new MixingUpdate());
            world.Poll();
        }

        [Test]
        public void IntegerAddition()
        {
            using World world = new();
            using ComponentMixingSystem componentMixing = new(world);

            uint entity = world.CreateEntity();
            world.AddComponent(entity, new First(7));
            world.AddComponent(entity, new Second(9123));
            world.AddComponent(entity, ComponentMix.Create<First, Second, Result>(ComponentMix.Operation.UnsignedAdd));

            Simulate(world);

            Assert.That(world.ContainsComponent<Result>(entity), Is.True);
            int first = world.GetComponent<First>(entity).value;
            int second = world.GetComponent<Second>(entity).value;
            int result = world.GetComponent<Result>(entity).value;
            Assert.That(result, Is.EqualTo(first + second));

            ref ComponentMix mix = ref world.GetComponentRef<ComponentMix>(entity);
            mix.operation = ComponentMix.Operation.SignedAdd;
            world.SetComponent(entity, new First(-424));

            Simulate(world);

            first = world.GetComponent<First>(entity).value;
            second = world.GetComponent<Second>(entity).value;
            result = world.GetComponent<Result>(entity).value;
            Assert.That(result, Is.EqualTo(first + second));
        }

        public struct Result
        {
            public int value;

            public Result(int value)
            {
                this.value = value;
            }
        }

        public struct First
        {
            public int value;

            public First(int value)
            {
                this.value = value;
            }
        }

        public struct Second
        {
            public int value;

            public Second(int value)
            {
                this.value = value;
            }
        }

        [Test]
        public void FloatingMultiply()
        {
            using World world = new();
            using ComponentMixingSystem componentMixing = new(world);
            uint entity = world.CreateEntity();
            world.AddComponent(entity, new FirstFloat(7.5f));
            world.AddComponent(entity, new SecondFloat(0.5f));
            world.AddComponent(entity, ComponentMix.Create<FirstFloat, SecondFloat, ResultFloat>(ComponentMix.Operation.FloatingMultiply));
            Simulate(world);
            Assert.That(world.ContainsComponent<ResultFloat>(entity), Is.True);
            float first = world.GetComponent<FirstFloat>(entity).value;
            float second = world.GetComponent<SecondFloat>(entity).value;
            float result = world.GetComponent<ResultFloat>(entity).value;
            Assert.That(result, Is.EqualTo(first * second));
            ref ComponentMix mix = ref world.GetComponentRef<ComponentMix>(entity);
            mix.operation = ComponentMix.Operation.FloatingAdd;
            world.SetComponent(entity, new FirstFloat(-424.5f));
            Simulate(world);
            first = world.GetComponent<FirstFloat>(entity).value;
            second = world.GetComponent<SecondFloat>(entity).value;
            result = world.GetComponent<ResultFloat>(entity).value;
            Assert.That(result, Is.EqualTo(first + second));
        }

        public struct FirstFloat
        {
            public float value;

            public FirstFloat(float value)
            {
                this.value = value;
            }
        }

        public struct SecondFloat
        {
            public float value;

            public SecondFloat(float value)
            {
                this.value = value;
            }
        }

        public struct ResultFloat
        {
            public float value;

            public ResultFloat(float value)
            {
                this.value = value;
            }
        }

        [Test]
        public void MixTwoVectors()
        {
            using World world = new();
            using ComponentMixingSystem componentMixing = new(world);
            uint entity = world.CreateEntity();
            world.AddComponent(entity, new FirstVector(new Vector3(1f, 2f, 3f)));
            world.AddComponent(entity, new SecondVector(new Vector3(0.5f, 0.5f, 0.5f)));
            world.AddComponent(entity, ComponentMix.Create<FirstVector, SecondVector, ResultVector>(ComponentMix.Operation.FloatingMultiply, 3));
            Simulate(world);
            Assert.That(world.ContainsComponent<ResultVector>(entity), Is.True);
            Vector3 first = world.GetComponent<FirstVector>(entity).value;
            Vector3 second = world.GetComponent<SecondVector>(entity).value;
            Vector3 result = world.GetComponent<ResultVector>(entity).value;
            Assert.That(result, Is.EqualTo(first * second));
        }

        public struct FirstVector
        {
            public Vector3 value;

            public FirstVector(Vector3 value)
            {
                this.value = value;
            }
        }

        public struct SecondVector
        {
            public Vector3 value;

            public SecondVector(Vector3 value)
            {
                this.value = value;
            }
        }

        public struct ResultVector
        {
            public Vector3 value;

            public ResultVector(Vector3 value)
            {
                this.value = value;
            }
        }
    }
}
