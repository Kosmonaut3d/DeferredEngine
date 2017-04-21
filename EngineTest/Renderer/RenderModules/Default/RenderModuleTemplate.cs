using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules.Default
{
    //Just a template
    public class RenderModuleTemplate : IRenderModule, IDisposable
    {
        private Effect _shader;
        private EffectParameter _parameter1;
        private EffectPass _pass1;

        public RenderModuleTemplate(ContentManager content, string shaderPath)
        {
            Load(content, shaderPath);
            Initialize();
        }

        public void Initialize()
        {
            _parameter1 = _shader.Parameters["parameter1"];

            _pass1 = _shader.Techniques["technique1"].Passes[0];
        }
        
        public void Load(ContentManager content, string shaderPath)
        {
            _shader = content.Load<Effect>(shaderPath);

        }
       
        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            throw new NotImplementedException();
        }

        public void SetMaterialSettings()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _shader?.Dispose();
        }
    }
}
