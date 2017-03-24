using Microsoft.Xna.Framework;

namespace DeferredEngine.Renderer.RenderModules
{
    public interface IRenderModule
    {
        void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection);
    }
}