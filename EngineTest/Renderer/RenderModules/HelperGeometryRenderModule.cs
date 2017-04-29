using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper.HelperGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    public class HelperGeometryRenderModule
    {
        //Lines
        private Effect _shader; //= Globals.content.Load<Effect>("Shaders/Editor/LineEffect");
        private EffectParameter _worldViewProjParam;
        private EffectParameter _globalColorParam;
        private EffectPass _vertexColorPass;
        private EffectPass _globalColorPass;

        public void Initialize()
        {
            _worldViewProjParam = _shader.Parameters["WorldViewProj"];
            _globalColorParam = _shader.Parameters["GlobalColor"];

            //Passes
            _vertexColorPass = _shader.Techniques["VertexColor"].Passes[0];
            _globalColorPass = _shader.Techniques["GlobalColor"].Passes[0];

        }

        public HelperGeometryRenderModule(ContentManager content, string shaderPath)
        {
            _shader = content.Load<Effect>(shaderPath);
        }

        public void Draw(GraphicsDevice graphics, Matrix viewProjection)
        {
            HelperGeometryManager.GetInstance()
                .Draw(graphics, viewProjection, _worldViewProjParam, _globalColorParam, _vertexColorPass,
                    _globalColorPass);
        }
    }
}
