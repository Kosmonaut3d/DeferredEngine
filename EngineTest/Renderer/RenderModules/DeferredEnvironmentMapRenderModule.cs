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
        private EffectParameter _paramDepthMap;
        private EffectParameter _paramFrustumCorners;
        private EffectParameter _paramCameraPositionWS;
        private EffectParameter _paramReflectionCubeMap;
        private EffectParameter _paramResolution;
        private EffectParameter _paramFireflyReduction;
        private EffectParameter _paramFireflyThreshold;
        private EffectParameter _paramTransposeView;
        private EffectParameter _paramSpecularStrength;
        private EffectParameter _paramSpecularStrengthRcp;
        private EffectParameter _paramDiffuseStrength;
        private EffectParameter _paramTime;

        public EffectParameter ParamVolumeTexParam;
        public EffectParameter ParamVolumeTexSizeParam;
        public EffectParameter ParamVolumeTexResolution;
        public EffectParameter ParamInstanceInverseMatrix;
        public EffectParameter ParamInstanceScale;
        public EffectParameter ParamInstanceSDFIndex;
        public EffectParameter ParamInstancesCount;

        public EffectParameter ParamUseSDFAO;


        private EffectPass _passBasic;
        private EffectPass _passSky;
        private bool _fireflyReduction;
        private float _fireflyThreshold;
        private float _specularStrength;
        private float _diffuseStrength;
        private bool _useSDFAO;

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
        public Texture2D DepthMap
        {
            set { _paramDepthMap.SetValue(value); }
        }

        public Texture2D NormalMap
        {
            set { _paramNormalMap.SetValue(value); }
        }
        public Texture2D SSRMap
        {
            set { _paramSSRMap.SetValue(value); }
        }

        public Vector3[] FrustumCornersWS
        {
            set { _paramFrustumCorners.SetValue(value); }
        }

        public Vector3 CameraPositionWS
        {
            set { _paramCameraPositionWS.SetValue(value); }
        }

        public Vector2 Resolution
        {
            set { _paramResolution.SetValue(value); }
        }

        public float Time
        {
            set { _paramTime.SetValue(value); }
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

        public bool UseSDFAO
        {
            get { return _useSDFAO; }
            set
            {
                if (_useSDFAO != value)
                {
                    _useSDFAO = value;
                    ParamUseSDFAO.SetValue(value);
                }
            }
        }

        public void Initialize()
        {
            //Environment
            _paramAlbedoMap = _deferredEnvironmentShader.Parameters["AlbedoMap"];
            _paramNormalMap = _deferredEnvironmentShader.Parameters["NormalMap"];
            _paramDepthMap = _deferredEnvironmentShader.Parameters["DepthMap"];
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
            _paramCameraPositionWS = _deferredEnvironmentShader.Parameters["CameraPositionWS"];
            _paramTime = _deferredEnvironmentShader.Parameters["Time"];
            
            //SDF
            ParamVolumeTexParam = _deferredEnvironmentShader.Parameters["VolumeTex"];
            ParamVolumeTexSizeParam = _deferredEnvironmentShader.Parameters["VolumeTexSize"];
            ParamVolumeTexResolution = _deferredEnvironmentShader.Parameters["VolumeTexResolution"];
            ParamInstanceInverseMatrix = _deferredEnvironmentShader.Parameters["InstanceInverseMatrix"];
            ParamInstanceScale = _deferredEnvironmentShader.Parameters["InstanceScale"];
            ParamInstanceSDFIndex = _deferredEnvironmentShader.Parameters["InstanceSDFIndex"];
            ParamInstancesCount = _deferredEnvironmentShader.Parameters["InstancesCount"];

            ParamUseSDFAO = _deferredEnvironmentShader.Parameters["UseSDFAO"];

            _passSky = _deferredEnvironmentShader.Techniques["Sky"].Passes[0];
            _passBasic = _deferredEnvironmentShader.Techniques["Basic"].Passes[0];
        }
        
        public void Load(ContentManager content, string shaderPath)
        {
            _deferredEnvironmentShader = content.Load<Effect>(shaderPath);

        }

        public void DrawEnvironmentMap(GraphicsDevice graphicsDevice, Camera camera, Matrix view, FullScreenTriangle fullScreenTriangle, EnvironmentSample envSample, GameTime gameTime, bool fireflyReduction, float ffThreshold)
        {
            FireflyReduction = fireflyReduction;
            FireflyThreshold = ffThreshold;
            
            SpecularStrength = envSample.SpecularStrength;
            DiffuseStrength = envSample.DiffuseStrength;
            CameraPositionWS = camera.Position;

            Time = (float)gameTime.TotalGameTime.TotalSeconds % 1000;
            
            graphicsDevice.DepthStencilState = DepthStencilState.None;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            UseSDFAO = envSample.UseSDFAO;
            _paramTransposeView.SetValue(Matrix.Transpose(view));
            _passBasic.Apply();
            fullScreenTriangle.Draw(graphicsDevice);
        }

        public void DrawSky(GraphicsDevice graphicsDevice, FullScreenTriangle quadRenderer)
        {
            graphicsDevice.DepthStencilState = DepthStencilState.None;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            _passSky.Apply();
            quadRenderer.Draw(graphicsDevice);

        }

        public void Dispose()
        {
            _deferredEnvironmentShader?.Dispose();
        }
    }
}
