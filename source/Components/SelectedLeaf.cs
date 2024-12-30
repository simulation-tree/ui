using Worlds;

namespace InteractionKit.Components
{
    [ArrayElement]
    public struct SelectedLeaf
    {
        public rint nodeReference;

        public SelectedLeaf(rint nodeReference)
        {
            this.nodeReference = nodeReference;
        }
    }
}