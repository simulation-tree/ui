using Collections;
using InteractionKit.Components;
using Simulation;
using System;
using Worlds;

namespace InteractionKit.Systems
{
    public readonly partial struct ToggleSystem : ISystem
    {
        private readonly ComponentQuery<IsToggle> toggleQuery;
        private readonly ComponentQuery<IsPointer> pointerQuery;
        private readonly List<uint> pressedPointers;

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
                CleanUp();
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