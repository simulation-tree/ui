using Collections;
using InteractionKit.Components;
using Simulation;
using System;
using Worlds;

namespace InteractionKit.Systems
{
    public readonly partial struct ToggleSystem : ISystem
    {
        private readonly List<uint> pressedPointers;
        private readonly List<uint> toggleEntities;

        private ToggleSystem(List<uint> pressedPointers, List<uint> toggleEntities)
        {
            this.pressedPointers = pressedPointers;
            this.toggleEntities = toggleEntities;
        }

        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                List<uint> pressedPointers = new();
                List<uint> toggleEntities = new();
                systemContainer.Write(new ToggleSystem(pressedPointers, toggleEntities));
            }
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
            Update(world);
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                toggleEntities.Dispose();
                pressedPointers.Dispose();
            }
        }

        private readonly void Update(World world)
        {
            FindToggleEntities(world);

            ComponentQuery<IsPointer> pointerQuery = new(world);
            pointerQuery.ExcludeDisabled(true);
            foreach (var p in pointerQuery)
            {
                ref IsPointer pointer = ref p.component1;
                uint entity = p.entity;
                bool pressed = pointer.HasPrimaryIntent;
                bool wasPressed = pressedPointers.Contains(entity);
                if (wasPressed != pressed)
                {
                    if (pressed)
                    {
                        rint hoveringOverReference = pointer.hoveringOverReference;
                        if (hoveringOverReference != default)
                        {
                            uint selectedEntity = world.GetReference(entity, hoveringOverReference);
                            if (toggleEntities.Contains(selectedEntity))
                            {
                                ref IsToggle toggle = ref world.GetComponent<IsToggle>(selectedEntity);
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

                        pressedPointers.Add(entity);
                    }
                    else
                    {
                        pressedPointers.TryRemoveBySwapping(entity);
                    }
                }
            }
        }

        private readonly void FindToggleEntities(World world)
        {
            toggleEntities.Clear();
            ComponentQuery<IsToggle> query = new(world);
            foreach (var t in query)
            {
                toggleEntities.Add(t.entity);
            }
        }
    }
}