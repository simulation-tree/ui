using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public readonly struct IsControlField
    {
        public readonly rint labelReference;
        public readonly rint entityReference;
        public readonly byte typeIndex;
        public readonly bool isComponentType;

        public IsControlField(rint labelReference, rint entityReference, byte typeIndex, bool isComponentType)
        {
            this.labelReference = labelReference;
            this.entityReference = entityReference;
            this.typeIndex = typeIndex;
            this.isComponentType = isComponentType;
        }
    }
}