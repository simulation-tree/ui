using Worlds;

namespace UI.Components
{
    [ArrayElement]
    public readonly struct ControlEntity
    {
        public readonly rint reference;

        public ControlEntity(rint reference)
        {
            this.reference = reference;
        }
    }
}