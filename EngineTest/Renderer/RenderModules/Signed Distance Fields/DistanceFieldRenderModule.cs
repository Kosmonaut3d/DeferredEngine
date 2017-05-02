
using System;
using System.Collections.Generic;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
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
        private EffectPass _generateSDFPass;
        private EffectPass _volumePass;
        private EffectPass _distancePass;
        private EffectParameter _frustumCornersParam;
        private EffectParameter _cameraPositonParam;
        private EffectParameter _depthMapParam;
        private EffectParameter _volumeTexParam;
        private EffectParameter _meshOffset;
        private EffectParameter _volumeTexSizeParam;
        private EffectParameter _volumeTexResolution;

        private EffectParameter _instanceInverseMatrixArrayParam;
        private EffectParameter _instanceScaleArrayParam;
        private EffectParameter _instancesCountParam;

        private const int InstanceMaxCount = 40;

        private Matrix[] _instanceInverseMatrixArray = new Matrix[40];
        private Vector3[] _instanceScaleArray = new Vector3[40];
        private int _instancesCount = 0;

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

        public Vector3 MeshOffset
        {
            set { _meshOffset.SetValue(value); }
        }

        public Vector3 VolumeTexSize { set { _volumeTexSizeParam.SetValue(value); } }
        
        public Vector3 VolumeTexResolution { set { _volumeTexResolution.SetValue(value); } }
        

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
            _volumeTexSizeParam = _shader.Parameters["VolumeTexSize"];
            _volumeTexResolution = _shader.Parameters["VolumeTexResolution"];

            _instanceInverseMatrixArrayParam = _shader.Parameters["InstanceInverseMatrix"];
            _instanceScaleArrayParam = _shader.Parameters["InstanceScale"];
            _instancesCountParam = _shader.Parameters["InstancesCount"];

            _meshOffset = _shader.Parameters["MeshOffset"];
            _triangleTexResolution = _shader.Parameters["TriangleTexResolution"];
            _triangleAmount = _shader.Parameters["TriangleAmount"];

            _distancePass = _shader.Techniques["Distance"].Passes[0];
            _volumePass = _shader.Techniques["Volume"].Passes[0];
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
        
        public void Draw(GraphicsDevice graphicsDevice, Camera camera, FullScreenTriangle fullScreenTriangle)
        {
            CameraPosition = camera.Position;

            if (GameSettings.sdf_drawvolume)
                _volumePass.Apply(); 
            else
                _distancePass.Apply(); 
            fullScreenTriangle.Draw(graphicsDevice);
            //quadRenderer.RenderFullscreenQuad(graphicsDevice);
        }

        public void UpdateDistanceFieldTransformations(List<BasicEntity> entities)
        {
            if (!GameSettings.sdf_draw) return;

            int i = 0;
            for (var index = 0; index < entities.Count; index++)
            {
                BasicEntity entity = entities[index];

                if (entity.SignedDistanceField.IsUsed)
                {
                    _instanceInverseMatrixArray[i] = entity.WorldTransform.InverseWorld;
                    _instanceScaleArray[i] = entity.WorldTransform.Scale;

                    // Should only be done once

                    VolumeTex = entity.SignedDistanceField.SdfTexture;
                    VolumeTexResolution = entity.SignedDistanceField.TextureResolution;
                    VolumeTexSize = entity.SignedDistanceField.VolumeSize;

                    i++;
                    if (i >= InstanceMaxCount) break;
                }
                
            }

            _instancesCount = i;

            //Submit
            _instanceInverseMatrixArrayParam.SetValue(_instanceInverseMatrixArray);
            _instanceScaleArrayParam.SetValue(_instanceScaleArray);
            _instancesCountParam.SetValue((float)_instancesCount);

        }



        public RenderTarget2D CreateSDFTexture(GraphicsDevice graphics, Texture2D triangleData, int xsteps, int ysteps, int zsteps, SignedDistanceField sdf, FullScreenTriangle fullScreenTriangle, int trianglesLength)
        {
            RenderTarget2D output = new RenderTarget2D(graphics, xsteps * zsteps, ysteps, false, SurfaceFormat.Single, DepthFormat.None);

            graphics.SetRenderTarget(output);

            //Offset isntead of position!
            VolumeTexResolution = new Vector3(xsteps, ysteps, zsteps);
            VolumeTexSize = sdf.VolumeSize;
            MeshOffset = sdf.Offset;
            VolumeTex = triangleData;

            _triangleTexResolution.SetValue(new Vector2(triangleData.Width, triangleData.Height));
            _triangleAmount.SetValue((float) trianglesLength);

            _generateSDFPass.Apply();
            fullScreenTriangle.Draw(graphics);

            return output;
        }

    }
}
