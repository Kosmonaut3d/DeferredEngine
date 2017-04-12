using System;
using DeferredEngine.Entities;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    //Just a template
    public class DeferredEnvironmentMapRenderModule : IDisposable
    {
        private Effect _deferredEnvironmentShader;
        private EffectParameter _paramAlbedoMap;
        private EffectParameter _paramNormalMap;
        private EffectParameter _paramSSRMap;
        private EffectParameter _paramFrustumCorners;
        private EffectParameter _paramReflectionCubeMap;
        private EffectParameter _paramResolution;
        private EffectParameter _paramFireflyReduction;
        private EffectParameter _paramFireflyThreshold;
        private EffectParameter _paramTransposeView;
        private EffectParameter _paramSpecularStrength;
        private EffectParameter _paramSpecularStrengthRcp;
        private EffectParameter _paramDiffuseStrength;
        private EffectPass _passBasic;
        private EffectPass _passSky;
        private bool _fireflyReduction;
        private float _fireflyThreshold;
        private float _specularStrength;
        private float _diffuseStrength;

        public DeferredEnvironmentMapRenderModule(ContentManager content, string shaderPath)
        {
            Load(content, shaderPath);
            Initialize();
        }

        public RenderTargetCube Cubemap
        {
            set { _paramReflectionCubeMap.SetValue(value); }
        }

        public Texture2D AlbedoMap
        {
            set { _paramAlbedoMap.SetValue(value); }
        }
        public Texture2D NormalMap
        {
            set { _paramNormalMap.SetValue(value); }
        }
        public Texture2D SSRMap
        {
            set { _paramSSRMap.SetValue(value); }
        }

        public Vector3[] FrustumCorners
        {
            set { _paramFrustumCorners.SetValue(value); }
        }

        public Vector2 Resolution
        {
            set { _paramResolution.SetValue(value); }
        }

        public bool FireflyReduction
        {
            get { return _fireflyReduction; }
            set
            {
                if (value != _fireflyReduction)
                {
                    _fireflyReduction = value;
                    _paramFireflyReduction.SetValue(value);
                }
            }
        }

        public float FireflyThreshold
        {
            get { return _fireflyThreshold; }
            set {
                if (Math.Abs(value - _fireflyThreshold) > 0.0001f)
                {
                    _fireflyThreshold = value;
                    _paramFireflyThreshold.SetValue(value);
                }
            }
        }

        public float SpecularStrength
        {
            get { return _specularStrength; }
            set {
                if (Math.Abs(value - _specularStrength) > 0.0001f)
                {
                    _specularStrength = value;
                    _paramSpecularStrength.SetValue(value);
                    _paramSpecularStrengthRcp.SetValue(1.0f / value);
                }
            }
        }

        public float DiffuseStrength
        {
            get { return _diffuseStrength; }
            set
            {
                if (Math.Abs(value - _diffuseStrength) > 0.0001f)
                {
                    _diffuseStrength = value;
                    _paramDiffuseStrength.SetValue(value);
                }
            }
        }

        public void Initialize()
        {
            //Environment
            _paramAlbedoMap = _deferredEnvironmentShader.Parameters["AlbedoMap"];
            _paramNormalMap = _deferredEnvironmentShader.Parameters["NormalMap"];
            _paramFrustumCorners = _deferredEnvironmentShader.Parameters["FrustumCorners"];
            _paramSSRMap = _deferredEnvironmentShader.Parameters["ReflectionMap"];
            _paramReflectionCubeMap = _deferredEnvironmentShader.Parameters["ReflectionCubeMap"];
            _paramResolution = _deferredEnvironmentShader.Parameters["Resolution"];
            _paramFireflyReduction = _deferredEnvironmentShader.Parameters["FireflyReduction"];
            _paramFireflyThreshold = _deferredEnvironmentShader.Parameters["FireflyThreshold"];
            _paramTransposeView = _deferredEnvironmentShader.Parameters["TransposeView"];
            _paramSpecularStrength = _deferredEnvironmentShader.Parameters["EnvironmentMapSpecularStrength"];
            _paramSpecularStrengthRcp = _deferredEnvironmentShader.Parameters["EnvironmentMapSpecularStrengthRcp"];
            _paramDiffuseStrength = _deferredEnvironmentShader.Parameters["EnvironmentMapDiffuseStrength"];

            _passSky = _deferredEnvironmentShader.Techniques["Sky"].Passes[0];
            _passBasic = _deferredEnvironmentShader.Techniques["Basic"].Passes[0];
        }
        
        public void Load(ContentManager content, string shaderPath)
        {
            _deferredEnvironmentShader = content.Load<Effect>(shaderPath);

        }

        public void DrawEnvironmentMap(GraphicsDevice graphicsDevice, Matrix view, QuadRenderer quadRenderer, EnvironmentSample envSample, bool fireflyReduction, float ffThreshold)
        {
            FireflyReduction = fireflyReduction;
            FireflyThreshold = ffThreshold;

            SpecularStrength = envSample.SpecularStrength;
            DiffuseStrength = envSample.DiffuseStrength;
            
            graphicsDevice.DepthStencilState = DepthStencilState.None;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _paramTransposeView.SetValue(Matrix.Transpose(view));
            
            _passBasic.Apply();
            quadRenderer.RenderQuad(graphicsDevice, Vector2.One * -1, Vector2.One);

        }

        public void DrawSky(GraphicsDevice graphicsDevice, QuadRenderer quadRenderer)
        {
            graphicsDevice.DepthStencilState = DepthStencilState.None;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            _passSky.Apply();
            quadRenderer.RenderQuad(graphicsDevice, Vector2.One * -1, Vector2.One);

        }

        public void Dispose()
        {
            _deferredEnvironmentShader?.Dispose();
        }
    }
}
