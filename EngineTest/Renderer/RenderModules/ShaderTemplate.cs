using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    //Just a template
    public class ShaderTemplate : IShader
    {
        private Effect _shader;
        private EffectParameter _parameter1;
        private EffectPass _pass1;

        public ShaderTemplate(ContentManager content, string shaderPath)
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
    }
}
