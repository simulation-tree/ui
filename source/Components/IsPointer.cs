using System.Numerics;
using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct IsPointer
    {
        public Vector2 position;
        public rint hoveringOverReference;
        public PointerAction action;
        public Vector2 scroll;

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

        public IsPointer(Vector2 position, rint hoveringOverReference)
        {
            this.position = position;
            this.hoveringOverReference = hoveringOverReference;
        }
    }
}