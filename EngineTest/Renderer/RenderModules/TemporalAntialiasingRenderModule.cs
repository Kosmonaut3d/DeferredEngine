using System;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    //Just a template
    public class TemporalAntialiasingRenderModule : IDisposable
    {
        private Effect _taaShader;

        private EffectParameter _paramDepthMap;
        private EffectParameter _paramAccumulationMap;
        private EffectParameter _paramUpdateMap;
        private EffectParameter _paramCurrentToPrevious;
        private EffectParameter _paramResolution;
        private EffectParameter _paramFrustumCorners;
        private EffectParameter _paramUseTonemap;

        public TemporalAntialiasingRenderModule(ContentManager content, string shaderPath)
        {
            Load(content, shaderPath);
            Initialize();
        }

        private Vector3[] _frustumCorners;
        private Vector2 _resolution;
        private RenderTarget2D _depthMap;
        private EffectPass _taaPass;
        private EffectPass _invTonemapPass;

        private bool _useTonemap;

        public Vector3[] FrustumCorners
        {
            get { return _frustumCorners; }
            set
            {
                _frustumCorners = value; 
                _paramFrustumCorners.SetValue(_frustumCorners);
            }
        }

        public Vector2 Resolution
        {
            get { return _resolution; }
            set
            {
                _resolution = value;
                _paramResolution.SetValue(_resolution);
            }
        }

        public RenderTarget2D DepthMap
        {
            get { return _depthMap; }
            set
            {
                _depthMap = value; 
                _paramDepthMap.SetValue(value);
            }
        }

        public bool UseTonemap
        {
            get { return _useTonemap; }
            set
            {
                if (value != _useTonemap)
                {
                    _useTonemap = value;
                    _paramUseTonemap.SetValue(value);
                }
            }
        }

        public void Initialize()
        {
            _paramAccumulationMap = _taaShader.Parameters["AccumulationMap"];
            _paramUpdateMap = _taaShader.Parameters["UpdateMap"];
            _paramDepthMap = _taaShader.Parameters["DepthMap"];
            _paramCurrentToPrevious = _taaShader.Parameters["CurrentToPrevious"];
            _paramResolution = _taaShader.Parameters["Resolution"];
            _paramFrustumCorners = _taaShader.Parameters["FrustumCorners"];
            _paramUseTonemap = _taaShader.Parameters["UseTonemap"];

            _useTonemap = _paramUseTonemap.GetValueBoolean();

            _taaPass = _taaShader.Techniques["TemporalAntialiasing"].Passes[0];
            _invTonemapPass = _taaShader.Techniques["InverseTonemap"].Passes[0];
        }
        
        public void Load(ContentManager content, string shaderPath)
        {
            _taaShader = content.Load<Effect>(shaderPath);


        }


        public void Draw(GraphicsDevice _graphicsDevice, bool useTonemap, RenderTarget2D currentFrame, RenderTarget2D previousFrames, RenderTarget2D output, Matrix currentViewToPreviousViewProjection, FullScreenTriangle fullScreenTriangle)
        {

            UseTonemap = useTonemap;

            _graphicsDevice.SetRenderTarget(output);
            _graphicsDevice.BlendState = BlendState.Opaque;

            _paramAccumulationMap.SetValue(previousFrames);
            _paramUpdateMap.SetValue(currentFrame);
            _paramCurrentToPrevious.SetValue(currentViewToPreviousViewProjection);

            _taaPass.Apply();
            fullScreenTriangle.Draw(_graphicsDevice);
            
            if (useTonemap)
            {
                _graphicsDevice.SetRenderTarget(currentFrame);
                _paramUpdateMap.SetValue(output);
                _invTonemapPass.Apply();
                fullScreenTriangle.Draw(_graphicsDevice);
            }
        }

        public void Dispose()
        {
            _taaShader?.Dispose();
            _depthMap?.Dispose();
        }
    }
}
