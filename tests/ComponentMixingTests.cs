using InteractionKit.Components;
using InteractionKit.Systems;
using Simulation.Tests;
using System;
using System.Numerics;
using Worlds;

namespace InteractionKit.Tests
{
    public class ComponentMixingTests : SimulationTests
    {
        protected override void SetUp()
        {
            base.SetUp();
            ComponentType.Register<ComponentMix>();
            ComponentType.Register<First>();
            ComponentType.Register<Second>();
            ComponentType.Register<Result>();
            ComponentType.Register<FirstFloat>();
            ComponentType.Register<SecondFloat>();
            ComponentType.Register<ResultFloat>();
            ComponentType.Register<FirstVector>();
            ComponentType.Register<SecondVector>();
            ComponentType.Register<ResultVector>();
            Simulator.AddSystem<ComponentMixingSystem>();
        }

        [Test]
        public void IntegerAddition()
        {
            uint entity = World.CreateEntity();
            World.AddComponent(entity, new First(7));
            World.AddComponent(entity, new Second(9123));
            World.AddComponent(entity, ComponentMix.Create<First, Second, Result>(ComponentMix.Operation.UnsignedAdd));

            Simulator.Update(TimeSpan.FromSeconds(0.1f));

            Assert.That(World.ContainsComponent<Result>(entity), Is.True);
            int first = World.GetComponent<First>(entity).value;
            int second = World.GetComponent<Second>(entity).value;
            int result = World.GetComponent<Result>(entity).value;
            Assert.That(result, Is.EqualTo(first + second));

            ref ComponentMix mix = ref World.GetComponentRef<ComponentMix>(entity);
            mix.operation = ComponentMix.Operation.SignedAdd;
            World.SetComponent(entity, new First(-424));

            Simulator.Update(TimeSpan.FromSeconds(0.1f));

            first = World.GetComponent<First>(entity).value;
            second = World.GetComponent<Second>(entity).value;
            result = World.GetComponent<Result>(entity).value;
            Assert.That(result, Is.EqualTo(first + second));
        }

        [Component]
        public struct Result
        {
            public int value;

            public Result(int value)
            {
                this.value = value;
            }
        }

        [Component]
        public struct First
        {
            public int value;

            public First(int value)
            {
                this.value = value;
            }
        }

        [Component]
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
            uint entity = World.CreateEntity();
            World.AddComponent(entity, new FirstFloat(7.5f));
            World.AddComponent(entity, new SecondFloat(0.5f));
            World.AddComponent(entity, ComponentMix.Create<FirstFloat, SecondFloat, ResultFloat>(ComponentMix.Operation.FloatingMultiply));

            Simulator.Update(TimeSpan.FromSeconds(0.1f));

            Assert.That(World.ContainsComponent<ResultFloat>(entity), Is.True);
            float first = World.GetComponent<FirstFloat>(entity).value;
            float second = World.GetComponent<SecondFloat>(entity).value;
            float result = World.GetComponent<ResultFloat>(entity).value;
            Assert.That(result, Is.EqualTo(first * second));
            ref ComponentMix mix = ref World.GetComponentRef<ComponentMix>(entity);
            mix.operation = ComponentMix.Operation.FloatingAdd;
            World.SetComponent(entity, new FirstFloat(-424.5f));

            Simulator.Update(TimeSpan.FromSeconds(0.1f));

            first = World.GetComponent<FirstFloat>(entity).value;
            second = World.GetComponent<SecondFloat>(entity).value;
            result = World.GetComponent<ResultFloat>(entity).value;
            Assert.That(result, Is.EqualTo(first + second));
        }

        [Component]
        public struct FirstFloat
        {
            public float value;

            public FirstFloat(float value)
            {
                this.value = value;
            }
        }

        [Component]
        public struct SecondFloat
        {
            public float value;

            public SecondFloat(float value)
            {
                this.value = value;
            }
        }

        [Component]
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
            uint entity = World.CreateEntity();
            World.AddComponent(entity, new FirstVector(new Vector3(1f, 2f, 3f)));
            World.AddComponent(entity, new SecondVector(new Vector3(0.5f, 0.5f, 0.5f)));
            World.AddComponent(entity, ComponentMix.Create<FirstVector, SecondVector, ResultVector>(ComponentMix.Operation.FloatingMultiply, 3));

            Simulator.Update(TimeSpan.FromSeconds(0.1f));

            Assert.That(World.ContainsComponent<ResultVector>(entity), Is.True);
            Vector3 first = World.GetComponent<FirstVector>(entity).value;
            Vector3 second = World.GetComponent<SecondVector>(entity).value;
            Vector3 result = World.GetComponent<ResultVector>(entity).value;
            Assert.That(result, Is.EqualTo(first * second));
        }

        [Component]
        public struct FirstVector
        {
            public Vector3 value;

            public FirstVector(Vector3 value)
            {
                this.value = value;
            }
        }

        [Component]
        public struct SecondVector
        {
            public Vector3 value;

            public SecondVector(Vector3 value)
            {
                this.value = value;
            }
        }

        [Component]
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
