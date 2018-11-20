using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules.DeferredLighting
{
    public class PointLightRenderModule : IDisposable
    {

        public Effect _pointLightShader;

        public EffectTechnique deferredPointLightUnshadowed;
        public EffectTechnique deferredPointLightUnshadowedVolumetric;
        public EffectTechnique deferredPointLightShadowedSDF;
        public EffectTechnique deferredPointLightShadowed;
        public EffectTechnique deferredPointLightShadowedVolumetric;
        public EffectTechnique deferredPointLightWriteStencil;

        public EffectParameter deferredPointLightParameterShadowMap;

        public EffectParameter deferredPointLightParameterResolution;
        public EffectParameter deferredPointLightParameter_WorldView;
        public EffectParameter deferredPointLightParameter_WorldViewProjection;
        public EffectParameter deferredPointLightParameter_InverseView;

        public EffectParameter deferredPointLightParameter_LightPosition;
        public EffectParameter deferredPointLightParameter_LightColor;
        public EffectParameter deferredPointLightParameter_LightRadius;
        public EffectParameter deferredPointLightParameter_LightIntensity;
        public EffectParameter deferredPointLightParameter_ShadowMapSize;
        public EffectParameter deferredPointLightParameter_ShadowMapRadius;
        public EffectParameter deferredPointLightParameter_Inside;
        public EffectParameter deferredPointLightParameter_Time;
        public EffectParameter deferredPointLightParameter_FarClip;
        public EffectParameter deferredPointLightParameter_LightVolumeDensity;

        public EffectParameter deferredPointLightParameter_VolumeTexParam;
        public EffectParameter deferredPointLightParameter_VolumeTexSizeParam;
        public EffectParameter deferredPointLightParameter_VolumeTexResolution;
        public EffectParameter deferredPointLightParameter_InstanceInverseMatrix;
        public EffectParameter deferredPointLightParameter_InstanceScale;
        public EffectParameter deferredPointLightParameter_InstanceSDFIndex;
        public EffectParameter deferredPointLightParameter_InstancesCount;

        public EffectParameter deferredPointLightParameter_NoiseMap;
        public EffectParameter deferredPointLightParameter_AlbedoMap;
        public EffectParameter deferredPointLightParameter_NormalMap;
        public EffectParameter deferredPointLightParameter_DepthMap;

        private int _shaderIndex;
        private ShaderManager _shaderManagerReference;

        private DepthStencilState _stencilCullPass1;
        private DepthStencilState _stencilCullPass2;

        public PointLightRenderModule(ShaderManager shaderManager, string shaderPath)
        {
            Load(shaderManager, shaderPath);

            InitializeShader();

            _stencilCullPass1 = new DepthStencilState()
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false,
                DepthBufferFunction = CompareFunction.LessEqual,
                StencilFunction = CompareFunction.Always,
                StencilDepthBufferFail = StencilOperation.IncrementSaturation,
                StencilPass = StencilOperation.Keep,
                StencilFail = StencilOperation.Keep,
                CounterClockwiseStencilFunction = CompareFunction.Always,
                CounterClockwiseStencilDepthBufferFail = StencilOperation.Keep,
                CounterClockwiseStencilPass = StencilOperation.Keep,
                CounterClockwiseStencilFail = StencilOperation.Keep,
                StencilMask = 0,
                ReferenceStencil = 0,
                StencilEnable = true,
            };

            _stencilCullPass2 = new DepthStencilState()
            {
                DepthBufferEnable = false,
                DepthBufferWriteEnable = false,
                DepthBufferFunction = CompareFunction.GreaterEqual,
                CounterClockwiseStencilFunction = CompareFunction.Equal,
                StencilFunction = CompareFunction.Equal,
                StencilFail = StencilOperation.Zero,
                StencilPass = StencilOperation.Zero,
                CounterClockwiseStencilFail = StencilOperation.Zero,
                CounterClockwiseStencilPass = StencilOperation.Zero,
                ReferenceStencil = 0,
                StencilEnable = true,
                StencilMask = 0,

            };

        }

        private void Load(ShaderManager shaderManager, string shaderPath)
        {
            //"Shaders/Deferred/DeferredPointLight"

            _shaderIndex = shaderManager.AddShader(shaderPath);

            _pointLightShader = shaderManager.GetShader(_shaderIndex);

            _shaderManagerReference = shaderManager;
        }


        private void InitializeShader()
        {
            deferredPointLightUnshadowed = _pointLightShader.Techniques["Unshadowed"];
            deferredPointLightUnshadowedVolumetric = _pointLightShader.Techniques["UnshadowedVolume"];
            deferredPointLightShadowed = _pointLightShader.Techniques["Shadowed"];
            deferredPointLightShadowedSDF = _pointLightShader.Techniques["ShadowedSDF"];
            deferredPointLightShadowedVolumetric = _pointLightShader.Techniques["ShadowedVolume"];
            deferredPointLightWriteStencil = _pointLightShader.Techniques["WriteStencilMask"];

            deferredPointLightParameterShadowMap = _pointLightShader.Parameters["ShadowMap"];

            deferredPointLightParameterResolution = _pointLightShader.Parameters["Resolution"];
            deferredPointLightParameter_WorldView = _pointLightShader.Parameters["WorldView"];
            deferredPointLightParameter_WorldViewProjection = _pointLightShader.Parameters["WorldViewProj"];
            deferredPointLightParameter_InverseView = _pointLightShader.Parameters["InverseView"];

            deferredPointLightParameter_LightPosition = _pointLightShader.Parameters["lightPosition"];
            deferredPointLightParameter_LightColor = _pointLightShader.Parameters["lightColor"];
            deferredPointLightParameter_LightRadius = _pointLightShader.Parameters["lightRadius"];
            deferredPointLightParameter_LightIntensity = _pointLightShader.Parameters["lightIntensity"];
            deferredPointLightParameter_ShadowMapSize = _pointLightShader.Parameters["ShadowMapSize"];
            deferredPointLightParameter_ShadowMapRadius = _pointLightShader.Parameters["ShadowMapRadius"];
            deferredPointLightParameter_Inside = _pointLightShader.Parameters["inside"];
            deferredPointLightParameter_Time = _pointLightShader.Parameters["Time"];
            deferredPointLightParameter_FarClip = _pointLightShader.Parameters["FarClip"];
            deferredPointLightParameter_LightVolumeDensity = _pointLightShader.Parameters["lightVolumeDensity"];

            deferredPointLightParameter_VolumeTexParam = _pointLightShader.Parameters["VolumeTex"];
            deferredPointLightParameter_VolumeTexSizeParam = _pointLightShader.Parameters["VolumeTexSize"];
            deferredPointLightParameter_VolumeTexResolution = _pointLightShader.Parameters["VolumeTexResolution"];
            deferredPointLightParameter_InstanceInverseMatrix = _pointLightShader.Parameters["InstanceInverseMatrix"];
            deferredPointLightParameter_InstanceScale = _pointLightShader.Parameters["InstanceScale"];
            deferredPointLightParameter_InstanceSDFIndex = _pointLightShader.Parameters["InstanceSDFIndex"];
            deferredPointLightParameter_InstancesCount = _pointLightShader.Parameters["InstancesCount"];

            deferredPointLightParameter_NoiseMap = _pointLightShader.Parameters["NoiseMap"];
            deferredPointLightParameter_AlbedoMap = _pointLightShader.Parameters["AlbedoMap"];
            deferredPointLightParameter_NormalMap = _pointLightShader.Parameters["NormalMap"];
            deferredPointLightParameter_DepthMap = _pointLightShader.Parameters["DepthMap"];
    }

        /// <summary>
        /// Draw the point lights, set up some stuff first
        /// </summary>
        /// <param name="pointLights"></param>
        /// <param name="cameraOrigin"></param>
        /// <param name="gameTime"></param>
        public void Draw(List<PointLight> pointLights, Vector3 cameraOrigin, GameTime gameTime, Assets assets, BoundingFrustum _boundingFrustum, bool _viewProjectionHasChanged, Matrix _view, Matrix _viewProjection, Matrix _inverseView, GraphicsDevice _graphicsDevice)
        {

            if (pointLights.Count < 1) return;

            ModelMeshPart meshpart = assets.SphereMeshPart;
            _graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
            _graphicsDevice.Indices = (meshpart.IndexBuffer);
            int primitiveCount = meshpart.PrimitiveCount;
            int vertexOffset = meshpart.VertexOffset;
            int startIndex = meshpart.StartIndex;

            if (GameSettings.g_VolumetricLights)
                deferredPointLightParameter_Time.SetValue((float)gameTime.TotalGameTime.TotalSeconds % 1000);

            for (int index = 0; index < pointLights.Count; index++)
            {
                PointLight light = pointLights[index];
                DrawPointLight(light, cameraOrigin, vertexOffset, startIndex, primitiveCount, _boundingFrustum, _viewProjectionHasChanged, _view, _viewProjection, _inverseView, _graphicsDevice);
            }
        }

        /// <summary>
        /// Draw each individual point lights
        /// </summary>
        /// <param name="light"></param>
        /// <param name="cameraOrigin"></param>
        private void DrawPointLight(PointLight light, Vector3 cameraOrigin, int vertexOffset, int startIndex, int primitiveCount, BoundingFrustum _boundingFrustum, bool _viewProjectionHasChanged, Matrix _view, Matrix _viewProjection, Matrix _inverseView, GraphicsDevice _graphicsDevice)
        {
            if (!light.IsEnabled) return;

            //first let's check if the light is even in bounds
            if (_boundingFrustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint ||
                !_boundingFrustum.Intersects(light.BoundingSphere))
                return;

            //For our stats
            GameStats.LightsDrawn++;

            //Send the light parameters to the shader
            if (_viewProjectionHasChanged)
            {
                light.LightViewSpace = light.WorldMatrix * _view;
                light.LightWorldViewProj = light.WorldMatrix * _viewProjection;
            }

            deferredPointLightParameter_WorldView.SetValue(light.LightViewSpace);
            deferredPointLightParameter_WorldViewProjection.SetValue(light.LightWorldViewProj);
            deferredPointLightParameter_LightPosition.SetValue(light.LightViewSpace.Translation);
            deferredPointLightParameter_LightColor.SetValue(light.ColorV3);
            deferredPointLightParameter_LightRadius.SetValue(light.Radius);
            deferredPointLightParameter_LightIntensity.SetValue(light.Intensity);

            //Compute whether we are inside or outside and use 
            float cameraToCenter = Vector3.Distance(cameraOrigin, light.Position);
            int inside = cameraToCenter < light.Radius * 1.2f ? 1 : -1;
            deferredPointLightParameter_Inside.SetValue(inside);

            if (GameSettings.g_UseDepthStencilLightCulling == 2)
            {
                _graphicsDevice.DepthStencilState = _stencilCullPass1;
                //draw front faces
                _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                deferredPointLightWriteStencil.Passes[0].Apply();

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);

                ////////////

                _graphicsDevice.DepthStencilState = _stencilCullPass2;
                //draw backfaces
                _graphicsDevice.RasterizerState = RasterizerState.CullClockwise;

                ApplyShader(light);

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
            }
            else
            {
                //If we are inside compute the backfaces, otherwise frontfaces of the sphere
                _graphicsDevice.RasterizerState = inside > 0 ? RasterizerState.CullClockwise : RasterizerState.CullCounterClockwise;

                ApplyShader(light);

                _graphicsDevice.DepthStencilState = GameSettings.g_UseDepthStencilLightCulling > 0 && !light.IsVolumetric && inside < 0 ? DepthStencilState.DepthRead : DepthStencilState.None;

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
            }

            //Draw the sphere
        }

        private void ApplyShader(PointLight light)
        {
            // Experimental
            if (light.CastSDFShadows)
            {
                deferredPointLightShadowedSDF.Passes[0].Apply();
            }
            else if (light.ShadowMap != null && light.CastShadows)
            {
                deferredPointLightParameterShadowMap.SetValue(light.ShadowMap);
                deferredPointLightParameter_ShadowMapRadius.SetValue((float)light.ShadowMapRadius);
                deferredPointLightParameter_ShadowMapSize.SetValue((float)light.ShadowResolution);

                if (light.IsVolumetric && GameSettings.g_VolumetricLights)
                {
                    deferredPointLightParameter_LightVolumeDensity.SetValue(light.LightVolumeDensity);
                    deferredPointLightShadowedVolumetric.Passes[0].Apply();
                }
                else
                {
                    deferredPointLightShadowed.Passes[0].Apply();
                }
            }
            else
            {
                //todo: remove

                deferredPointLightParameter_ShadowMapRadius.SetValue((float)light.ShadowMapRadius);

                if (light.IsVolumetric && GameSettings.g_VolumetricLights)
                {
                    deferredPointLightParameter_LightVolumeDensity.SetValue(light.LightVolumeDensity);
                    deferredPointLightUnshadowedVolumetric.Passes[0].Apply();
                }
                else
                {
                    deferredPointLightUnshadowed.Passes[0].Apply();
                }
            }
        }

        public void Dispose()
        {
            _pointLightShader?.Dispose();
            _stencilCullPass1?.Dispose();
            _stencilCullPass2?.Dispose();
        }
    }
}
