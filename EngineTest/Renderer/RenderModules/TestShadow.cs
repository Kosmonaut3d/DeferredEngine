using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    public class TestShadow
    {
        public RenderTarget2D Output;
        private GraphicsDevice _graphicsDevice;
        private QuadRenderer _quadRenderer;

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _quadRenderer = new QuadRenderer();
        }

        public void Draw(RenderTarget2D colorMap, Matrix _view, Camera camera, Matrix _projection)
        {
            if(Output == null) CreateRT();

            _graphicsDevice.SetRenderTarget(Output);

            _graphicsDevice.BlendState = BlendState.Opaque;

            Shaders.TestShadowEffect_AccumulationMap.SetValue(colorMap);

            float translation = GameSettings.tr;
            Vector3 direction = camera.Forward;
            direction.Normalize();

            Vector3 normal = Vector3.Cross(direction, camera.Up);

            Matrix translate = Matrix.CreateTranslation(normal * translation);

            Matrix newView = Matrix.CreateLookAt(camera.Position + normal*translation, camera.Lookat, camera.Up) ;

            Matrix viewProjection = newView*_projection;

            Matrix transform = Matrix.Invert(_view) * viewProjection;


            Shaders.TestShadowEffect_CurrentToPrevious.SetValue(transform);

            Shaders.TestShadowEffect.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(_graphicsDevice, Microsoft.Xna.Framework.Vector2.One * -1, Vector2.One);
            
        }

        public void CreateRT()
        {
            Output = new RenderTarget2D(_graphicsDevice, GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight,false, SurfaceFormat.Color, DepthFormat.None);
        }

        public void UpdateResolution()
        {
            Output.Dispose();
            CreateRT();
        }
    }
}
