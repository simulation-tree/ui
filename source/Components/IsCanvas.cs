using Rendering;
using System;
using Worlds;

namespace UI.Components
{
    [Component]
    public struct IsCanvas
    {
        public rint cameraReference;
        public rint settingsReference;
        public LayerMask renderMask;
        public LayerMask selectionMask;

#if NET
        [Obsolete("Default constructor not supported", true)]
        public IsCanvas()
        {
            throw new NotSupportedException();
        }
#endif

        public IsCanvas(rint cameraReference, rint settingsReference, LayerMask renderMask, LayerMask selectionMask)
        {
            this.cameraReference = cameraReference;
            this.settingsReference = settingsReference;
            this.renderMask = renderMask;
            this.selectionMask = selectionMask;
        }
    }
}