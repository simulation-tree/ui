using System;
using Worlds;

namespace InteractionKit.Components
{
    [Component]
    public struct IsCanvas
    {
        public rint cameraReference;
        public rint settingsReference;
        public uint renderMask;
        public uint selectionMask;

#if NET
        [Obsolete("Default constructor not supported", true)]
        public IsCanvas()
        {
            throw new NotSupportedException();
        }
#endif

        public IsCanvas(rint cameraReference, rint settingsReference, uint renderMask = uint.MaxValue, uint selectionMask = uint.MaxValue)
        {
            this.cameraReference = cameraReference;
            this.settingsReference = settingsReference;
            this.renderMask = renderMask;
            this.selectionMask = selectionMask;
        }
    }
}