using Worlds;

namespace UI.Components
{
    public readonly struct IsControlField
    {
        public readonly rint labelReference;
        public readonly rint entityReference;
        public readonly DataType dataType;

        public IsControlField(rint labelReference, rint entityReference, DataType dataType)
        {
            this.labelReference = labelReference;
            this.entityReference = entityReference;
            this.dataType = dataType;
        }
    }
}