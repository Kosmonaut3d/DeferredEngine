using System;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.RenderModules.Default;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    //Just a template
    public class SubsurfaceScatterRenderModule : IRenderModule, IDisposable
    {
        private Effect _shader;
        
        private EffectParameter _albedoMapParameter;
        private EffectParameter _normalMapParameter;
        private EffectParameter _frustumCornersParameter;
        private EffectParameter _worldParam;
        private EffectParameter _worldViewProjParam;
        private EffectParameter _worldViewITParam;

        private EffectPass _pass1;

        public Vector3[] FrustumCorners
        {
            set { _frustumCornersParameter.SetValue(value); }
        }
        
        public Texture2D NormalMap { set { _normalMapParameter.SetValue(value); } }
        public Texture2D AlbedoMap { set { _albedoMapParameter.SetValue(value); } }

        public Matrix World { set { _worldParam.SetValue(value);} }
        public Matrix WorldViewProj { set { _worldViewProjParam.SetValue(value); } }
        public Matrix WorldViewIT { set { _worldViewITParam.SetValue(value); } }

        public SubsurfaceScatterRenderModule(ContentManager content, string shaderPath)
        {
            Load(content, shaderPath);
            Initialize();
        }

        public void Initialize()
        {
            _worldParam = _shader.Parameters["World"];
            _worldViewProjParam = _shader.Parameters["WorldViewProj"];
            _worldViewITParam = _shader.Parameters["WorldViewIT"];

            _albedoMapParameter = _shader.Parameters["AlbedoMap"];
            _normalMapParameter = _shader.Parameters["NormalMap"];
            _frustumCornersParameter = _shader.Parameters["FrustumCorners"];

            _pass1 = _shader.Techniques["Default"].Passes[0];
        }

        public void Load(ContentManager content, string shaderPath)
        {
            _shader = content.Load<Effect>(shaderPath);

        }

        public RenderTarget2D Draw(GraphicsDevice graphicsDevice, RenderTarget2D input, RenderTarget2D output, MeshMaterialLibrary meshMat, Matrix viewProjection)
        {
            graphicsDevice.SetRenderTarget(output);
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            meshMat.Draw(MeshMaterialLibrary.RenderType.SubsurfaceScattering, viewProjection, renderModule: this);
            
            return output;
        }

        public void Dispose()
        {
            _shader?.Dispose();
        }

        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            //Matrix worldView = localWorldMatrix * (Matrix)view;
            World = localWorldMatrix;
            WorldViewProj = localWorldMatrix * viewProjection;
            WorldViewIT = Matrix.Transpose(localWorldMatrix);
            
            _pass1.Apply();
            //_WorldViewProj.SetValue(localWorldMatrix * viewProjection);

            //worldView = Matrix.Transpose(Matrix.Invert(worldView));
            //_WorldViewIT.SetValue(worldView);
        }
    }
}
