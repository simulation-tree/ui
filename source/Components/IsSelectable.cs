using Rendering;
using System;

namespace UI.Components
{
    public struct IsSelectable
    {
        public State state;
        public LayerMask selectionMask;

        /// <summary>
        /// <c>true</c> when this entity is the only selected entity.
        /// </summary>
        public readonly bool IsSelected => (state & State.IsSelected) != 0;

        public readonly bool WasPrimaryInteractedWith => (state & State.WasPrimaryInteractedWith) != 0;
        public readonly bool WasSecondaryInteractedWith => (state & State.WasSecondaryInteractedWith) != 0;

#if NET
        [Obsolete("Default constructor not supported", true)]
        public IsSelectable()
        {
            throw new NotSupportedException();
        }
#endif

        public IsSelectable(LayerMask selectionMask)
        {
            state = State.None;
            this.selectionMask = selectionMask;
        }

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