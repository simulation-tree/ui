using Worlds;

namespace UI.Components
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