using System;
using DeferredEngine.Entities;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules.Default
{
    //Just a template
    public class VolumeProjectionRenderModule : IDisposable
    {
        private Effect _shader;
        private EffectPass _basicPass;
        private EffectParameter _frustumCornersParam;
        private EffectParameter _cameraPositonParam;
        private EffectParameter _depthMapParam;
        private EffectParameter _volumeTexParam;
        private EffectParameter _volumeTexPositionParam;
        private EffectParameter _volumeTexSizeParam;
        private EffectParameter _volumeTexResolution;

        public Vector3[] FrustumCornersWorldSpace
        {
            set { _frustumCornersParam.SetValue(value); }
        }
        public Vector3 CameraPosition { set { _cameraPositonParam.SetValue(value); } }

        public Texture2D DepthMap { set { _depthMapParam.SetValue(value); } }

        private Texture2D _volumeTex;
        public Texture2D VolumeTex
        {
            set
            {
                if (_volumeTex != value)
                {
                    _volumeTex = value;
                    _volumeTexParam.SetValue(value);
                }
            }
        }

        public Vector3 VolumeTexPosition
        {
            set { _volumeTexPositionParam.SetValue(value); }
        }

        public Vector3 VolumeTexSize { set { _volumeTexSizeParam.SetValue(value); } }

        public Vector3 VolumeTexResolution { set { _volumeTexResolution.SetValue(value); } }


        public VolumeProjectionRenderModule(ContentManager content, string shaderPath)
        {
            Load(content, shaderPath);
            Initialize();
        }

        public void Initialize()
        {
            _frustumCornersParam = _shader.Parameters["FrustumCorners"];
            _cameraPositonParam = _shader.Parameters["CameraPosition"];
            _depthMapParam = _shader.Parameters["DepthMap"];
            _volumeTexParam = _shader.Parameters["VolumeTex"];
            _volumeTexPositionParam = _shader.Parameters["VolumeTexPositionWS"];
            _volumeTexSizeParam = _shader.Parameters["VolumeTexSize"];
            _volumeTexResolution = _shader.Parameters["VolumeTexResolution"];

            _basicPass = _shader.Techniques["Basic"].Passes[0];
        }
        
        public void Load(ContentManager content, string shaderPath)
        {
            _shader = content.Load<Effect>(shaderPath);

        }
       
        public void Dispose()
        {
            _shader?.Dispose();
        }
        
        public void Draw(GraphicsDevice graphicsDevice, Camera camera, VolumeTextureEntity volumeTextureEntity, FullScreenTriangle fullScreenTriangle)
        {
            CameraPosition = camera.Position;

            VolumeTex = volumeTextureEntity.Texture;
            VolumeTexPosition = volumeTextureEntity.Position;
            VolumeTexResolution = volumeTextureEntity.Resolution;
            VolumeTexSize = volumeTextureEntity.Size;

            _basicPass.Apply();
            fullScreenTriangle.Draw(graphicsDevice);
            //quadRenderer.RenderFullscreenQuad(graphicsDevice);
        }
    }
}
