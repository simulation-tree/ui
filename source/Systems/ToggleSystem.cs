using Collections;
using InteractionKit.Components;
using Simulation;
using Simulation.Functions;
using System;
using System.Runtime.InteropServices;
using Worlds;

namespace InteractionKit.Systems
{
    public readonly struct ToggleSystem : ISystem
    {
        private readonly ComponentQuery<IsToggle> toggleQuery;
        private readonly ComponentQuery<IsPointer> pointerQuery;
        private readonly List<uint> pressedPointers;

        readonly unsafe StartSystem ISystem.Start => new(&Start);
        readonly unsafe UpdateSystem ISystem.Update => new(&Update);
        readonly unsafe FinishSystem ISystem.Finish => new(&Finish);

        [UnmanagedCallersOnly]
        private static void Start(SystemContainer container, World world)
        {
        }

        [UnmanagedCallersOnly]
        private static void Update(SystemContainer container, World world, TimeSpan delta)
        {
            ref ToggleSystem system = ref container.Read<ToggleSystem>();
            system.Update(world);
        }

        [UnmanagedCallersOnly]
        private static void Finish(SystemContainer container, World world)
        {
            if (container.World == world)
            {
                ref ToggleSystem system = ref container.Read<ToggleSystem>();
                system.CleanUp();
            }
        }

        public ToggleSystem()
        {
            toggleQuery = new();
            pointerQuery = new();
            pressedPointers = new();
        }

        private void CleanUp()
        {
            pressedPointers.Dispose();
            pointerQuery.Dispose();
            toggleQuery.Dispose();
        }

        private void Update(World world)
        {
            pointerQuery.Update(world);
            toggleQuery.Update(world, true);
            foreach (var p in pointerQuery)
            {
                uint pointerEntity = p.entity;
                bool pressed = p.Component1.HasPrimaryIntent;
                bool wasPressed = pressedPointers.Contains(pointerEntity);
                if (wasPressed != pressed)
                {
                    if (pressed)
                    {
                        rint hoveringOverReference = p.Component1.hoveringOverReference;
                        if (hoveringOverReference != default)
                        {
                            uint selectedEntity = world.GetReference(pointerEntity, hoveringOverReference);
                            if (toggleQuery.TryIndexOf(selectedEntity, out uint index))
                            {
                                ref IsToggle toggle = ref toggleQuery[index].Component1;
                                toggle.value = !toggle.value;

                                rint checkmarkReference = toggle.checkmarkReference;
                                uint checkmarkEntity = world.GetReference(selectedEntity, checkmarkReference);
                                world.SetEnabled(checkmarkEntity, toggle.value);

                                if (toggle.callback != default)
                                {
                                    toggle.callback.Invoke(new(world, selectedEntity), toggle.value);
                                }
                            }
                        }

                        pressedPointers.Add(pointerEntity);
                    }
                    else
                    {
                        pressedPointers.TryRemoveBySwapping(pointerEntity);
                    }
                }
            }
        }
    }
}