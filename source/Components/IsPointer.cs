using Simulation;
using System.Numerics;

namespace InteractionKit.Components
{
    public struct IsPointer
    {
        public Vector2 position;
        public rint selectedReference;
        public PointerAction action;

        public bool HasPrimaryIntent
        {
            readonly get => (action & PointerAction.Primary) == PointerAction.Primary;
            set => action = value ? action | PointerAction.Primary : action & ~PointerAction.Primary;
        }

        public bool HasSecondaryIntent
        {
            readonly get => (action & PointerAction.Secondary) == PointerAction.Secondary;
            set => action = value ? action | PointerAction.Secondary : action & ~PointerAction.Secondary;
        }

        public IsPointer(Vector2 position, rint selectedReference)
        {
            this.position = position;
            this.selectedReference = selectedReference;
        }
    }
}