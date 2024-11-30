using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct IsControlField
    {
        public rint labelReference;
        public rint controlReference;
        public rint entityReference;
        public ComponentType componentType;

        public IsControlField(rint labelReference, rint controlReference, rint entityReference, ComponentType componentType)
        {
            this.labelReference = labelReference;
            this.controlReference = controlReference;
            this.entityReference = entityReference;
            this.componentType = componentType;
        }
    }
}