using Simulation;

namespace InteractionKit.Components
{
    public struct AdjacentSelectable
    {
        public rint up;
        public rint down;
        public rint left;
        public rint right;

        public AdjacentSelectable(rint up, rint down, rint left, rint right)
        {
            this.up = up;
            this.down = down;
            this.left = left;
            this.right = right;
        }
    }
}