using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    public class LightRenderer
    {
        private GraphicsDevice _graphicsDevice;
        private QuadRenderer _quadRenderer;
        private Assets _assets;
        private bool _g_UseDepthStencilLightCulling;
        private BlendState _lightBlendState;
        private BoundingFrustum _boundingFrustum;

        private bool _viewProjectionHasChanged;

        private Matrix _view;
        private Matrix _inverseView;
        private Matrix _viewIT;
        private Matrix _projection;
        private Matrix _viewProjection;
        private Matrix _inverseViewProjection;
        private DepthStencilState _stencilCullPass1;
        private DepthStencilState _stencilCullPass2;

        public void Initialize(GraphicsDevice graphicsDevice, QuadRenderer quadRenderer, Assets assets)
        {
            _graphicsDevice = graphicsDevice;
            _quadRenderer = quadRenderer;
            _assets = assets;

            _lightBlendState = new BlendState
            {
                AlphaSourceBlend = Blend.One,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                AlphaDestinationBlend = Blend.One
            };
            
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

        /// <summary>
        /// Needs to be called before draw
        /// </summary>
        /// <param name="boundingFrustum"></param>
        /// <param name="viewProjHasChanged"></param>
        /// <param name="view"></param>
        /// <param name="inverseView"></param>
        /// <param name="viewIT"></param>
        /// <param name="projection"></param>
        /// <param name="viewProjection"></param>
        /// <param name="staticViewProjection"></param>
        /// <param name="inverseViewProjection"></param>
        public void UpdateViewProjection( BoundingFrustum boundingFrustum,
                                         bool viewProjHasChanged,
                                         Matrix view,
                                         Matrix inverseView,
                                         Matrix viewIT,
                                         Matrix projection,
                                         Matrix viewProjection,
                                         Matrix inverseViewProjection)
        {
            _boundingFrustum = boundingFrustum;
            _viewProjectionHasChanged = viewProjHasChanged;
            _view = view;
            _inverseView = inverseView;
            _viewIT = viewIT;
            _projection = projection;
            _viewProjection = viewProjection;
            _inverseViewProjection = inverseViewProjection;
        }

        /// <summary>
        /// Draw our lights to the diffuse/specular/volume buffer
        /// </summary>
        /// <param name="pointLights"></param>
        /// <param name="dirLights"></param>
        /// <param name="cameraOrigin"></param>
        /// <param name="gameTime"></param>
        /// <param name="renderTargetLightBinding"></param>
        /// <param name="renderTargetDiffuse"></param>
        public void DrawLights(List<PointLightSource> pointLights, List<DirectionalLightSource> dirLights,
            Vector3 cameraOrigin, GameTime gameTime, RenderTargetBinding[] renderTargetLightBinding, RenderTarget2D  renderTargetDiffuse)
        {
            //Reconstruct Depth
            if (GameSettings.g_UseDepthStencilLightCulling > 0)
            {
                _graphicsDevice.SetRenderTarget(renderTargetDiffuse);
                _graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.TransparentBlack, 1, 0);
                _graphicsDevice.Clear(ClearOptions.Stencil, Color.TransparentBlack, 1, 0);
                ReconstructDepth();

                _g_UseDepthStencilLightCulling = true;
            }
            else
            {
                if (_g_UseDepthStencilLightCulling)
                {
                    _g_UseDepthStencilLightCulling = false;
                    _graphicsDevice.SetRenderTarget(renderTargetDiffuse);
                    _graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.TransparentBlack, 1, 0);
                }
            }

            _graphicsDevice.SetRenderTargets(renderTargetLightBinding);
            _graphicsDevice.Clear(ClearOptions.Target, Color.TransparentBlack, 1, 0);
            _graphicsDevice.BlendState = _lightBlendState;

            DrawPointLights(pointLights, cameraOrigin, gameTime);
            DrawDirectionalLights(dirLights, cameraOrigin);

            ////Performance Profiler
            //if (GameSettings.d_profiler)
            //{
            //    long performanceCurrentTime = _performanceTimer.ElapsedTicks;
            //    GameStats.d_profileDrawLights = performanceCurrentTime - _performancePreviousTime;

            //    _performancePreviousTime = performanceCurrentTime;
            //}

        }
        private void ReconstructDepth()
        {
            if (_viewProjectionHasChanged)
                Shaders.ReconstructDepthParameter_Projection.SetValue(_projection);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            Shaders.ReconstructDepth.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
        }

        /// <summary>
        /// Draw the point lights, set up some stuff first
        /// </summary>
        /// <param name="pointLights"></param>
        /// <param name="cameraOrigin"></param>
        /// <param name="gameTime"></param>
        private void DrawPointLights(List<PointLightSource> pointLights, Vector3 cameraOrigin, GameTime gameTime)
        {
            
            if (pointLights.Count < 1) return;

            ModelMeshPart meshpart = _assets.SphereMeshPart;
            _graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
            _graphicsDevice.Indices = (meshpart.IndexBuffer);
            int primitiveCount = meshpart.PrimitiveCount;
            int vertexOffset = meshpart.VertexOffset;
            int startIndex = meshpart.StartIndex;

            if (GameSettings.g_VolumetricLights)
                Shaders.deferredPointLightParameter_Time.SetValue((float)gameTime.TotalGameTime.TotalSeconds % 1000);

            for (int index = 0; index < pointLights.Count; index++)
            {
                PointLightSource light = pointLights[index];
                DrawPointLight(light, cameraOrigin, vertexOffset, startIndex, primitiveCount);
            }
        }

        /// <summary>
        /// Draw each individual point lights
        /// </summary>
        /// <param name="light"></param>
        /// <param name="cameraOrigin"></param>
        private void DrawPointLight(PointLightSource light, Vector3 cameraOrigin, int vertexOffset, int startIndex, int primitiveCount)
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

            Shaders.deferredPointLightParameter_WorldView.SetValue(light.LightViewSpace);
            Shaders.deferredPointLightParameter_WorldViewProjection.SetValue(light.LightWorldViewProj);
            Shaders.deferredPointLightParameter_LightPosition.SetValue(light.LightViewSpace.Translation);
            Shaders.deferredPointLightParameter_LightColor.SetValue(light.ColorV3);
            Shaders.deferredPointLightParameter_LightRadius.SetValue(light.Radius);
            Shaders.deferredPointLightParameter_LightIntensity.SetValue(light.Intensity);

            //Compute whether we are inside or outside and use 
            float cameraToCenter = Vector3.Distance(cameraOrigin, light.Position);
            int inside = cameraToCenter < light.Radius * 1.2f ? 1 : -1;
            Shaders.deferredPointLightParameter_Inside.SetValue(inside);

            if (GameSettings.g_UseDepthStencilLightCulling == 2)
            {
                _graphicsDevice.DepthStencilState = _stencilCullPass1;
                //draw front faces
                _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                Shaders.deferredPointLightWriteStencil.Passes[0].Apply();

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);

                ////////////

                _graphicsDevice.DepthStencilState = _stencilCullPass2;
                //draw backfaces
                _graphicsDevice.RasterizerState = RasterizerState.CullClockwise;

                light.ApplyShader(_inverseView);

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
            }
            else
            {
                //If we are inside compute the backfaces, otherwise frontfaces of the sphere
                _graphicsDevice.RasterizerState = inside > 0 ? RasterizerState.CullClockwise : RasterizerState.CullCounterClockwise;

                light.ApplyShader(_inverseView);

                _graphicsDevice.DepthStencilState = GameSettings.g_UseDepthStencilLightCulling > 0 && !light.IsVolumetric && inside < 0 ? DepthStencilState.DepthRead : DepthStencilState.None;

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
            }

            //Draw the sphere
        }

        /// <summary>
        /// Draw all directional lights, set up some shader variables first
        /// </summary>
        /// <param name="dirLights"></param>
        /// <param name="cameraOrigin"></param>
        private void DrawDirectionalLights(List<DirectionalLightSource> dirLights, Vector3 cameraOrigin)
        {
            if (dirLights.Count < 1) return;

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            //If nothing has changed we don't need to update
            if (_viewProjectionHasChanged)
            {
                Shaders.deferredDirectionalLightParameterViewProjection.SetValue(_viewProjection);
                Shaders.deferredDirectionalLightParameterCameraPosition.SetValue(cameraOrigin);
                Shaders.deferredDirectionalLightParameterInverseViewProjection.SetValue(_inverseViewProjection);
            }

            _graphicsDevice.DepthStencilState = DepthStencilState.None;

            for (int index = 0; index < dirLights.Count; index++)
            {
                DirectionalLightSource lightSource = dirLights[index];
                DrawDirectionalLight(lightSource);
            }
        }

        /// <summary>
        /// Draw the individual light, full screen effect
        /// </summary>
        /// <param name="lightSource"></param>
        private void DrawDirectionalLight(DirectionalLightSource lightSource)
        {
            if (!lightSource.IsEnabled) return;

            if (_viewProjectionHasChanged)
            {
                lightSource.DirectionViewSpace = Vector3.Transform(lightSource.Direction, _viewIT);
                lightSource.LightViewProjection_ViewSpace = _inverseView * lightSource.LightViewProjection;
                lightSource.LightView_ViewSpace = _inverseView*lightSource.LightView;
            }

            Shaders.deferredDirectionalLightParameter_LightColor.SetValue(lightSource.Color.ToVector3());
            Shaders.deferredDirectionalLightParameter_LightDirection.SetValue(lightSource.DirectionViewSpace);
            Shaders.deferredDirectionalLightParameter_LightIntensity.SetValue(lightSource.Intensity);
            lightSource.ApplyShader();
            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
        }

    }
}
