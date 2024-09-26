using Simulation;
using Unmanaged;

namespace InteractionKit.Components
{
    public struct IsControlField
    {
        public rint labelReference;
        public rint controlReference;
        public rint entityReference;
        public RuntimeType componentType;

        public IsControlField(rint labelReference, rint controlReference, rint entityReference, RuntimeType componentType)
        {
            this.labelReference = labelReference;
            this.controlReference = controlReference;
            this.entityReference = entityReference;
            this.componentType = componentType;
        }
    }
}