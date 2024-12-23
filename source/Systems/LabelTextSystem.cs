using Collections;
using InteractionKit.Components;
using InteractionKit.Functions;
using Rendering.Components;
using Simulation;
using System;
using Unmanaged;
using Worlds;

namespace InteractionKit.Systems
{
    public readonly partial struct LabelTextSystem : ISystem
    {
        private readonly Text result;
        private readonly List<TryProcessLabel> processors;

        public LabelTextSystem()
        {
            result = new();
            processors = new();
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                processors.Dispose();
                result.Dispose();
            }
        }

        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                systemContainer.Write(new LabelTextSystem());
            }
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
            processors.Clear();
            ComponentQuery<IsLabelProcessor> processorQuery = new(world);
            foreach (var r in processorQuery)
            {
                processors.Add(r.component1.function);
            }

            ComponentQuery<IsLabel, IsTextRenderer> labelQuery = new(world);
            foreach (var r in labelQuery)
            {
                USpan<char> originalText = world.GetArray<LabelCharacter>(r.entity).As<char>();
                ref rint textMeshReference = ref r.component2.textMeshReference;
                if (textMeshReference != default)
                {
                    result.CopyFrom(originalText);
                    foreach (TryProcessLabel token in processors)
                    {
                        token.Invoke(result.AsSpan(), result);
                    }

                    uint textMeshEntity = world.GetReference(r.entity, textMeshReference);
                    uint arrayLength = world.GetArrayLength<TextCharacter>(textMeshEntity);
                    if (arrayLength != result.Length)
                    {
                        world.ResizeArray<TextCharacter>(textMeshEntity, result.Length);
                    }

                    USpan<char> targetText = world.GetArray<TextCharacter>(textMeshEntity).As<char>();
                    if (!targetText.SequenceEqual(result.AsSpan()))
                    {
                        ref IsTextMeshRequest request = ref world.GetComponent<IsTextMeshRequest>(textMeshEntity);
                        request.version++;
                        result.CopyTo(targetText);
                    }
                }
            }
        }
    }
}