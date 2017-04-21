using Microsoft.Xna.Framework;

namespace DeferredEngine.Renderer.RenderModules.Default
{
    public interface IRenderModule
    {
        void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection);
    }
}