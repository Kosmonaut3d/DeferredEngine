
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
        private RenderTarget2D _atlasRenderTarget2D;

        private ShaderManager _shaderManagerReference;

        private Effect _shader;
        private int _shaderIndex;
        private EffectPass _generateSDFPass;
        private EffectPass _volumePass;
        private EffectPass _distancePass;
        private EffectParameter _frustumCornersParam;
        private EffectParameter _cameraPositonParam;
        private EffectParameter _depthMapParam;
        private EffectParameter _volumeTexParam;
        private EffectParameter _meshOffset;
        private EffectParameter _volumeTexSizeParam;
        private EffectParameter _volumeTexResolutionParam;

        private EffectParameter _instanceInverseMatrixArrayParam;
        private EffectParameter _instanceScaleArrayParam;
        private EffectParameter _instanceSDFIndexArrayParam;
        private EffectParameter _instancesCountParam;

        private const int InstanceMaxCount = 40;

        private Matrix[] _instanceInverseMatrixArray = new Matrix[InstanceMaxCount];
        private Vector3[] _instanceScaleArray = new Vector3[InstanceMaxCount];
        private float[] _instanceSDFIndexArray = new float[InstanceMaxCount];
        private int _instancesCount = 0;

        private Vector3[] _volumeTexSizeArray = new Vector3[40];
        private Vector4[] _volumeTexResolutionArray = new Vector4[40];

        private SignedDistanceField[] _signedDistanceFieldDefinitions = new SignedDistanceField[40];
        private int _signedDistanceFieldDefinitionsCount = 0;

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
        

        public DistanceFieldRenderModule(ShaderManager shaderManager, string shaderPath)
        {
            Load(shaderManager, shaderPath);
            Initialize();
        }

        public void Initialize()
        {
            _frustumCornersParam = _shader.Parameters["FrustumCorners"];
            _cameraPositonParam = _shader.Parameters["CameraPosition"];
            _depthMapParam = _shader.Parameters["DepthMap"];

            _volumeTexParam = _shader.Parameters["VolumeTex"];
            _volumeTexSizeParam = _shader.Parameters["VolumeTexSize"];
            _volumeTexResolutionParam = _shader.Parameters["VolumeTexResolution"];

            _instanceInverseMatrixArrayParam = _shader.Parameters["InstanceInverseMatrix"];
            _instanceScaleArrayParam = _shader.Parameters["InstanceScale"];
            _instanceSDFIndexArrayParam = _shader.Parameters["InstanceSDFIndex"];
            _instancesCountParam = _shader.Parameters["InstancesCount"];

            _meshOffset = _shader.Parameters["MeshOffset"];
            _triangleTexResolution = _shader.Parameters["TriangleTexResolution"];
            _triangleAmount = _shader.Parameters["TriangleAmount"];

            _distancePass = _shader.Techniques["Distance"].Passes[0];
            _volumePass = _shader.Techniques["Volume"].Passes[0];
            _generateSDFPass = _shader.Techniques["GenerateSDF"].Passes[0];
        }

        public void Load(ShaderManager shaderManager, string shaderPath)
        {
            _shaderIndex = shaderManager.AddShader(shaderPath);

            _shader = shaderManager.GetShader(_shaderIndex);

            _shaderManagerReference = shaderManager;
        }

        public void Dispose()
        {
            _shader?.Dispose();
        }

        public void Draw(GraphicsDevice graphicsDevice, Camera camera, FullScreenTriangle fullScreenTriangle)
        {
            CameraPosition = camera.Position;

            //CheckUpdate
            CheckForShaderChanges();

            if (GameSettings.sdf_drawvolume)
                _volumePass.Apply();
            else
                _distancePass.Apply();
            fullScreenTriangle.Draw(graphicsDevice);
            //quadRenderer.RenderFullscreenQuad(graphicsDevice);
        }

        private void CheckForShaderChanges()
        {
            if (_shaderManagerReference.GetShaderHasChanged(_shaderIndex))
            {
                _shader = _shaderManagerReference.GetShader(_shaderIndex);
                Initialize();
            }
        }

        public void UpdateDistanceFieldTransformations(List<BasicEntity> entities, List<SignedDistanceField> sdfDefinitions, DeferredEnvironmentMapRenderModule environmentMapRenderModule, GraphicsDevice graphics, SpriteBatch spriteBatch, LightAccumulationModule lightAccumulationModule)
        {
            if (!GameSettings.sdf_draw) return;
            
            //First of all let's build the atlas
            UpdateAtlas(sdfDefinitions, graphics, spriteBatch, environmentMapRenderModule, lightAccumulationModule);

            int i = 0;
            for (var index = 0; index < entities.Count; index++)
            {
                BasicEntity entity = entities[index];

                if (entity.SignedDistanceField.IsUsed)
                {
                    _instanceInverseMatrixArray[i] = entity.WorldTransform.InverseWorld;
                    _instanceScaleArray[i] = entity.WorldTransform.Scale;
                    _instanceSDFIndexArray[i] = entity.SignedDistanceField.ArrayIndex;

                    i++;

                    if (i >= InstanceMaxCount) break;
                }

            }

            _instancesCount = i;

            //TODO: Check for change

            //Submit
            //Instances

            _instanceInverseMatrixArrayParam.SetValue(_instanceInverseMatrixArray);
            _instanceScaleArrayParam.SetValue(_instanceScaleArray);
            _instanceSDFIndexArrayParam.SetValue(_instanceSDFIndexArray);
            _instancesCountParam.SetValue((float)_instancesCount);

            lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_InstanceInverseMatrix.SetValue(_instanceInverseMatrixArray);
            lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_InstanceScale.SetValue(_instanceScaleArray);
            lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_InstanceSDFIndex.SetValue(_instanceSDFIndexArray);
            lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_InstancesCount.SetValue((float)_instancesCount);

            environmentMapRenderModule.ParamInstanceInverseMatrix.SetValue(_instanceInverseMatrixArray);
            environmentMapRenderModule.ParamInstanceScale.SetValue(_instanceScaleArray);
            environmentMapRenderModule.ParamInstanceSDFIndex.SetValue(_instanceSDFIndexArray);
            environmentMapRenderModule.ParamInstancesCount.SetValue((float)_instancesCount);
        }

        private void UpdateAtlas(List<SignedDistanceField> sdfDefinitionsPassed, GraphicsDevice graphics,
            SpriteBatch spriteBatch, DeferredEnvironmentMapRenderModule environmentMapRenderModule, LightAccumulationModule lightAccumulationModule)
        {
            if (sdfDefinitionsPassed.Count < 1) return;

            bool updateAtlas = false;

            if (_signedDistanceFieldDefinitions == null || sdfDefinitionsPassed.Count !=
                _signedDistanceFieldDefinitionsCount)
            {
                _signedDistanceFieldDefinitionsCount = 0;
                updateAtlas = true;
            }
            

            {
                for (int i = 0; i < sdfDefinitionsPassed.Count; i++)
                {
                    bool found = false;
                    for (int j = 0; j < _signedDistanceFieldDefinitionsCount; j++)
                    {
                        if (sdfDefinitionsPassed[i] == _signedDistanceFieldDefinitions[j])
                        {
                            found = true;
                            break;

                            if(sdfDefinitionsPassed[i].NeedsToBeGenerated) throw new Exception("test");
                        }
                    }

                    if (!found)
                    {
                        _signedDistanceFieldDefinitions[_signedDistanceFieldDefinitionsCount] = sdfDefinitionsPassed[i];
                        sdfDefinitionsPassed[i].ArrayIndex = _signedDistanceFieldDefinitionsCount;
                        _signedDistanceFieldDefinitionsCount++;

                        updateAtlas = true;
                    }
                }
            }

            //Now build the atlas

            if (!updateAtlas) return;

            _atlasRenderTarget2D?.Dispose();

            int x = 0, y = 0;
            //Count size
            for (int i = 0; i < _signedDistanceFieldDefinitionsCount; i++)
            {
                x = (int) Math.Max(_signedDistanceFieldDefinitions[i].SdfTexture.Width, x);
                _signedDistanceFieldDefinitions[i].TextureResolution.W = y;
                y += _signedDistanceFieldDefinitions[i].SdfTexture.Height;

                _volumeTexResolutionArray[i] = _signedDistanceFieldDefinitions[i].TextureResolution;
                _volumeTexSizeArray[i] = _signedDistanceFieldDefinitions[i].VolumeSize;
            }

            //todo: Check if we can use half here
            _atlasRenderTarget2D = new RenderTarget2D(graphics, x, y, false, SurfaceFormat.Single, DepthFormat.None);

            graphics.SetRenderTarget(_atlasRenderTarget2D);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp);
            for (int i = 0; i < _signedDistanceFieldDefinitionsCount; i++)
            {
                spriteBatch.Draw(_signedDistanceFieldDefinitions[i].SdfTexture, 
                    new Rectangle(0, (int) _signedDistanceFieldDefinitions[i].TextureResolution.W, _signedDistanceFieldDefinitions[i].SdfTexture.Width, _signedDistanceFieldDefinitions[i].SdfTexture.Height), Color.White);
            }
            spriteBatch.End();


            //Atlas
            VolumeTex = _atlasRenderTarget2D;
            _volumeTexSizeParam.SetValue(_volumeTexSizeArray);
            _volumeTexResolutionParam.SetValue(_volumeTexResolutionArray);

            lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_VolumeTexParam.SetValue(_atlasRenderTarget2D);
            lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_VolumeTexSizeParam.SetValue(_volumeTexSizeArray);
            lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_VolumeTexResolution.SetValue(_volumeTexResolutionArray);

            environmentMapRenderModule.ParamVolumeTexParam.SetValue(_atlasRenderTarget2D);
            environmentMapRenderModule.ParamVolumeTexSizeParam.SetValue(_volumeTexSizeArray);
            environmentMapRenderModule.ParamVolumeTexResolution.SetValue(_volumeTexResolutionArray);
        }


        public RenderTarget2D CreateSDFTexture(GraphicsDevice graphics, Texture2D triangleData, int xsteps, int ysteps, int zsteps, SignedDistanceField sdf, FullScreenTriangle fullScreenTriangle, int trianglesLength)
        {
            RenderTarget2D output = new RenderTarget2D(graphics, xsteps * zsteps, ysteps, false, SurfaceFormat.Single, DepthFormat.None);

            graphics.SetRenderTarget(output);

            //Offset isntead of position!
            _volumeTexResolutionArray[0] = new Vector4(xsteps, ysteps, zsteps, 0);
            _volumeTexSizeArray[0] = sdf.VolumeSize;

            _volumeTexSizeParam.SetValue(_volumeTexSizeArray);
            _volumeTexResolutionParam.SetValue(_volumeTexResolutionArray);

            MeshOffset = sdf.Offset;
            VolumeTex = triangleData;

            _triangleTexResolution.SetValue(new Vector2(triangleData.Width, triangleData.Height));
            _triangleAmount.SetValue((float)trianglesLength);

            _generateSDFPass.Apply();
            fullScreenTriangle.Draw(graphics);

            _signedDistanceFieldDefinitionsCount = -1;

            return output;
        }

        public Texture2D GetAtlas()
        {
            return _atlasRenderTarget2D;
        }
    }
}
