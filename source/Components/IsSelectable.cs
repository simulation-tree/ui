using System;

namespace InteractionKit.Components
{
    public struct IsSelectable
    {
        public State state;

        /// <summary>
        /// <c>true</c> when this entity is the only selected entity.
        /// </summary>
        public readonly bool IsSelected => (state & State.IsSelected) != 0;

        public readonly bool WasPrimaryInteractedWith => (state & State.WasPrimaryInteractedWith) != 0;
        public readonly bool WasSecondaryInteractedWith => (state & State.WasSecondaryInteractedWith) != 0;

        [Flags]
        public enum State : byte
        {
            None = 0,
            IsSelected = 1,
            WasPrimaryInteractedWith = 2,
            WasSecondaryInteractedWith = 4,
            IsPrimaryInteractedWith = 8,
            IsSecondaryInteractedWith = 16
        }
    }
}