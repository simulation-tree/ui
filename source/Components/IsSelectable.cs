using System;

namespace InteractionKit.Components
{
    public struct IsSelectable
    {
        public State state;

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