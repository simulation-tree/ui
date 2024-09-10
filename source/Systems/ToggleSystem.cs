using InteractionKit.Components;
using InteractionKit.Events;
using Simulation;
using System;
using System.Numerics;
using Unmanaged.Collections;

namespace InteractionKit.Systems
{
    public class ToggleSystem : SystemBase
    {
        private readonly ComponentQuery<IsToggle> toggleQuery;
        private readonly ComponentQuery<IsPointer> pointerQuery;
        private readonly UnmanagedList<uint> pressedPointers;

        public ToggleSystem(World world) : base(world)
        {
            toggleQuery = new();
            pointerQuery = new();
            pressedPointers = new();
            Subscribe<InteractionUpdate>(Update);
        }

        public override void Dispose()
        {
            pressedPointers.Dispose();
            pointerQuery.Dispose();
            toggleQuery.Dispose();
            base.Dispose();
        }

        private void Update(InteractionUpdate update)
        {
            pointerQuery.Update(world);
            toggleQuery.Update(world);
            foreach (var p in pointerQuery)
            {
                uint pointerEntity = p.entity;
                bool pressed = p.Component1.HasPrimaryIntent;
                bool wasPressed = pressedPointers.Contains(pointerEntity);
                if (wasPressed != pressed)
                {
                    if (pressed)
                    {
                        rint selectedReference = p.Component1.selectedReference;
                        uint selectedEntity = world.GetReference(pointerEntity, selectedReference);
                        if (selectedEntity != default && toggleQuery.TryIndexOf(selectedEntity, out uint index))
                        {
                            ref IsToggle toggle = ref toggleQuery[index].Component1;
                            toggle.value = !toggle.value;

                            rint checkmarkReference = toggle.checkmarkReference;
                            uint checkmarkEntity = world.GetReference(selectedEntity, checkmarkReference);
                            world.SetEnabled(checkmarkEntity, toggle.value);
                        }

                        pressedPointers.Add(pointerEntity);
                    }
                    else
                    {
                        pressedPointers.TryRemove(pointerEntity);
                    }
                }
            }
        }
    }
}