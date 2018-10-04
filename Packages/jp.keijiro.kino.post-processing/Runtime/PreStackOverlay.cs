using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Kino.PostProcessing
{
    // Spacialization for pre-stack overlay effect
    
    [System.Serializable]
    [PostProcess(typeof(PreStackOverlayRenderer), PostProcessEvent.BeforeStack, "Kino/Pre-Stack Overlay")]
    public sealed class PreStackOverlay : OverlayBase {}

    public sealed class PreStackOverlayRenderer : OverlayRendererBase<PreStackOverlay> {}
}
