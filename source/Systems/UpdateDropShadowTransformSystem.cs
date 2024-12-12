using InteractionKit.Components;
using Simulation;
using System;
using System.Numerics;
using Transforms.Components;
using Worlds;

namespace InteractionKit.Systems
{
    public readonly partial struct UpdateDropShadowTransformSystem : ISystem
    {
        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
        }

        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
            bool stop = true;
            if (stop)
            {
                //return;
            }

            ComponentQuery<IsDropShadow> query = new(world);
            using Operation destroyOperation = new();
            foreach (var r in query)
            {
                ref IsDropShadow component = ref r.component1;
                uint foregroundEntity = world.GetReference(r.entity, component.foregroundReference);
                if (foregroundEntity == default || !world.ContainsEntity(foregroundEntity))
                {
                    destroyOperation.SelectEntity(r.entity);
                }
                else
                {
                    world.SetEnabled(r.entity, world.IsEnabled(foregroundEntity));
                }
            }

            if (destroyOperation.Count > 0)
            {
                destroyOperation.DestroySelected();
                world.Perform(destroyOperation);
            }

            const float ShadowDistance = 30f;
            ComponentQuery<IsDropShadow, Position, Scale> queryWihtPositionAndScale = new(world);
            foreach (var r in queryWihtPositionAndScale)
            {
                ref IsDropShadow component = ref r.component1;
                ref Position position = ref r.component2;
                ref Scale scale = ref r.component3;

                //make the dropshadow match the foreground in world space
                rint foregroundReference = component.foregroundReference;
                uint foregroundEntity = world.GetReference(r.entity, foregroundReference);
                ref LocalToWorld foregroundLtw = ref world.GetComponent<LocalToWorld>(foregroundEntity);
                position.value = foregroundLtw.Position + new Vector3(-ShadowDistance, -ShadowDistance, Settings.ZScale * -2f);
                scale.value = foregroundLtw.Scale + new Vector3(ShadowDistance * 2f, ShadowDistance * 2f, 1f);

                //update the ltw of the mesh to match foreground
                rint meshReference = component.meshReference;
                uint meshEntity = world.GetReference(r.entity, meshReference);
                ref LocalToWorld meshLtw = ref world.GetComponent<LocalToWorld>(meshEntity);
                meshLtw = foregroundLtw;
            }
        }
    }
}