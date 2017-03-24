using Microsoft.Xna.Framework;

namespace DeferredEngine.Renderer.RenderModules
{
    public interface IShader
    {
        void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection);
    }
}