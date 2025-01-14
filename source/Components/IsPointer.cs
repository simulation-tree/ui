using Rendering;
using System;
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
        public LayerMask selectionMask;

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

#if NET
        [Obsolete("Default constructor not available", true)]
        public IsPointer()
        {
            throw new NotSupportedException();
        }
#endif

        public IsPointer(Vector2 position, LayerMask selectionMask)
        {
            this.position = position;
            action = default;
            scroll = default;
            this.selectionMask = selectionMask;
        }

        public IsPointer(Vector2 position)
        {
            this.position = position;
            action = default;
            scroll = default;
            selectionMask = LayerMask.All;
        }
    }
}