using System;

namespace InteractionKit.Components
{
    public struct IsSelectable
    {
        public State state;

        public readonly bool IsSelected => (state & State.Selected) != 0;

        [Flags]
        public enum State : byte
        {
            None = 0,
            Selected = 1,
            WasPrimaryInteractedWith = 2,
            WasSecondaryInteractedWith = 4,
            IsPrimaryInteractedWith = 8,
            IsSecondaryInteractedWith = 16,
        }
    }
}