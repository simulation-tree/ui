using System;
using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct IsCanvas
    {
        public rint cameraReference;
        public uint renderMask;
        public uint selectionMask;

#if NET
        [Obsolete("Default constructor not supported", true)]
        public IsCanvas()
        {
            throw new NotSupportedException();
        }
#endif

        public IsCanvas(rint cameraReference, uint renderMask, uint selectionMask)
        {
            this.cameraReference = cameraReference;
            this.renderMask = renderMask;
            this.selectionMask = selectionMask;
        }
    }
}