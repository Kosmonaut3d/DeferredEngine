using System;
using DeferredEngine.Entities;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules.Signed_Distance_Fields
{
    //Just a template
    public class DistanceFieldRenderModule : IDisposable
    {
        private Effect _shader;
        private EffectPass _basicPass;
        private EffectPass _generateSDFPass;
        private EffectParameter _frustumCornersParam;
        private EffectParameter _cameraPositonParam;
        private EffectParameter _depthMapParam;
        private EffectParameter _volumeTexParam;
        private EffectParameter _volumeTexPositionParam;
        private EffectParameter _volumeTexSizeParam;
        private EffectParameter _volumeTexResolution;
        private EffectParameter _volumeTexInverseMatrix;
        private EffectParameter _volumeTexScale;
        private EffectParameter _triangleTexResolution;
        private EffectParameter _triangleAmount;

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


        public Vector3 VolumeTexScale { set { _volumeTexScale.SetValue(value); } }

        public Vector3 VolumeTexResolution { set { _volumeTexResolution.SetValue(value); } }
        public Matrix VolumeTexInverseMatrix { set { _volumeTexInverseMatrix.SetValue(value); } }

        public DistanceFieldRenderModule(ContentManager content, string shaderPath)
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
            _volumeTexInverseMatrix = _shader.Parameters["VolumeTexInverseMatrix"];
            _volumeTexScale = _shader.Parameters["VolumeTexScale"];

            _triangleTexResolution = _shader.Parameters["TriangleTexResolution"];
            _triangleAmount = _shader.Parameters["TriangleAmount"];

            _basicPass = _shader.Techniques["Basic"].Passes[0];
            _generateSDFPass = _shader.Techniques["GenerateSDF"].Passes[0];
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
            VolumeTexInverseMatrix = volumeTextureEntity.RotationMatrix;
            VolumeTexScale = volumeTextureEntity.Scale;

            _basicPass.Apply();
            fullScreenTriangle.Draw(graphicsDevice);
            //quadRenderer.RenderFullscreenQuad(graphicsDevice);
        }

        public RenderTarget2D CreateSDFTexture(GraphicsDevice graphics, Texture2D triangleData, int xsteps, int ysteps, int zsteps, VolumeTextureEntity volumeTex, FullScreenTriangle fullScreenTriangle, int trianglesLength)
        {
            RenderTarget2D output = new RenderTarget2D(graphics, xsteps * zsteps, ysteps, false, SurfaceFormat.Single, DepthFormat.None);

            graphics.SetRenderTarget(output);

            //Offset isntead of position!
            VolumeTexResolution = new Vector3(xsteps, ysteps, zsteps);
            VolumeTexSize = volumeTex.Size;
            VolumeTexPosition = volumeTex.Offset;
            VolumeTex = triangleData;

            _triangleTexResolution.SetValue(new Vector2(triangleData.Width, triangleData.Height));
            _triangleAmount.SetValue((float) trianglesLength);

            _generateSDFPass.Apply();
            fullScreenTriangle.Draw(graphics);

            return output;
        }
    }
}
