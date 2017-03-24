using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    public class ShadowMapShader : IShader
    {
        private Effect _shader;

        private EffectParameter _WorldViewProj;
        private EffectParameter _WorldView;
        private EffectParameter _World;
        private EffectParameter _LightPositionWS;
        private EffectParameter _FarClip;
        private EffectParameter _ArraySlice;
        private EffectParameter _SizeBias;

        //Linear = VS Depth -> used for directional lights
        private EffectPass _linearPass;

        //Distance = distance(pixel, light) -> used for omnidirectional lights
        private EffectPass _distancePass;

        private Passes _pass;

        private BoundingFrustum _boundingFrustumShadow;

        private enum Passes
        {
            Directional,
            Omnidirectional
        };

        public ShadowMapShader(ContentManager content, string shaderPath)
        {
            Load(content, shaderPath);
            Initialize();
        }

        public void Initialize()
        {
            _WorldViewProj = _shader.Parameters["WorldViewProj"];
            _WorldView = _shader.Parameters["WorldView"];
            _World = _shader.Parameters["World"];
            _LightPositionWS = _shader.Parameters["LightPositionWS"];
            _FarClip = _shader.Parameters["FarClip"];
            _SizeBias = _shader.Parameters["SizeBias"];

            _linearPass = _shader.Techniques["DrawLinearDepth"].Passes[0];
            _distancePass = _shader.Techniques["DrawDistanceDepth"].Passes[0];
        }

        public void Load(ContentManager content, string shaderPath)
        {
            _shader = content.Load<Effect>(shaderPath);
        }

        public void Draw(GraphicsDevice graphicsDevice, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities, List<PointLightSource> pointLights, List<DirectionalLightSource> dirLights, Camera camera)
        {
            _pass = Passes.Omnidirectional;

            //Go through all our point lights
            for (int index = 0; index < pointLights.Count; index++)
            {
                PointLightSource light = pointLights[index];

                if (!light.IsEnabled) continue;

                //If we don't see the light we shouldn't update. This is actually wrong, can lead to mistakes,
                //if we implement it like this we should rerender once we enter visible space again.
                //if (_boundingFrustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint)
                //{
                //    continue;
                //}

                if (light.CastShadows)
                {
                    //A poing light has 6 shadow maps, add that to our stat counter. These are total shadow maps, not updated ones
                    GameStats.shadowMaps += 6;

                    //Update if we didn't initialize yet or if we are dynamic
                    if (!light.StaticShadows || light.ShadowMap == null)
                    {
                        CreateShadowCubeMap(graphicsDevice, light, light.ShadowResolution, meshMaterialLibrary, entities);

                        light.HasChanged = false;
                        camera.HasChanged = true;
                    }
                }
            }

            _pass = Passes.Directional;

            int dirLightShadowedWithSSBlur = 0;
            for (int index = 0; index < dirLights.Count; index++)
            {
                DirectionalLightSource light = dirLights[index];
                if (!light.IsEnabled) continue;

                if (light.CastShadows)
                {
                    GameStats.shadowMaps += 1;

                    CreateShadowMapDirectionalLight(graphicsDevice, light, light.ShadowResolution, meshMaterialLibrary, entities);

                    camera.HasChanged = true;
                    light.HasChanged = false;

                    if (light.ScreenSpaceShadowBlur) dirLightShadowedWithSSBlur++;
                }

                if (dirLightShadowedWithSSBlur > 1)
                {
                    throw new NotImplementedException(
                        "Only one shadowed DirectionalLight with screen space blur is supported right now");
                }
            }
        }

        /// <summary>
        /// Create the shadow map for each cubemapside, then combine into one cubemap
        /// </summary>
        /// <param name="light"></param>
        /// <param name="size"></param>
        /// <param name="meshMaterialLibrary"></param>
        /// <param name="entities"></param>
        private void CreateShadowCubeMap(GraphicsDevice graphicsDevice, PointLightSource light, int size, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities)
        {
            //For VSM we need 2 channels, -> Vector2
            //todo: check if we need preserve contents
            if (light.ShadowMap == null)
                light.ShadowMap = new RenderTarget2D(graphicsDevice, size, size * 6, false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            Matrix lightViewProjection = new Matrix();
            CubeMapFace cubeMapFace; // = CubeMapFace.NegativeX;

            if (light.HasChanged)
            {
                graphicsDevice.SetRenderTarget(light.ShadowMap);

                Matrix lightProjection = Matrix.CreatePerspectiveFieldOfView((float)(Math.PI / 2), 1, 1, light.Radius);
                Matrix lightView; // = identity

                //Reset the blur array
                light.faceBlurCount = new int[6];
                
                graphicsDevice.SetRenderTarget(light.ShadowMap);
                graphicsDevice.Clear(Color.Black);

                for (int i = 0; i < 6; i++)
                {
                    // render the scene to all cubemap faces
                    cubeMapFace = (CubeMapFace)i;
                    switch (cubeMapFace)
                    {
                        case CubeMapFace.PositiveX:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.UnitX, Vector3.UnitZ);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionPositiveX = lightViewProjection;
                                break;
                            }
                        case CubeMapFace.NegativeX:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position - Vector3.UnitX, Vector3.UnitZ);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionNegativeX = lightViewProjection;
                                break;
                            }
                        case CubeMapFace.PositiveY:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.UnitY, Vector3.UnitZ);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionPositiveY = lightViewProjection;
                                break;
                            }
                        case CubeMapFace.NegativeY:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position - Vector3.UnitY, Vector3.UnitZ);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionNegativeY = lightViewProjection;
                                break;
                            }
                        case CubeMapFace.PositiveZ:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.UnitZ, Vector3.UnitX);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionPositiveZ = lightViewProjection;
                                break;
                            }
                        case CubeMapFace.NegativeZ:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position - Vector3.UnitZ, Vector3.UnitX);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionNegativeZ = lightViewProjection;
                                break;
                            }
                    }

                    if (_boundingFrustumShadow != null) _boundingFrustumShadow.Matrix = lightViewProjection;
                    else _boundingFrustumShadow = new BoundingFrustum(lightViewProjection);

                    meshMaterialLibrary.FrustumCulling(entities, _boundingFrustumShadow, true, light.Position);

                    // Rendering!
                    
                    graphicsDevice.Viewport = new Viewport(0, light.ShadowResolution * (int)cubeMapFace, light.ShadowResolution, light.ShadowResolution);
                    //_graphicsDevice.ScissorRectangle = new Rectangle(0, light.ShadowResolution* (int) cubeMapFace,  light.ShadowResolution, light.ShadowResolution);
                    
                    _FarClip.SetValue(light.Radius);
                    _LightPositionWS.SetValue(light.Position);

                    graphicsDevice.ScissorRectangle = new Rectangle(0, light.ShadowResolution * (int)cubeMapFace, light.ShadowResolution, light.ShadowResolution);

                    meshMaterialLibrary.Draw(renderType: MeshMaterialLibrary.RenderType.ShadowOmnidirectional, 
                        viewProjection: lightViewProjection, 
                        lightViewPointChanged: true, 
                        hasAnyObjectMoved: light.HasChanged, 
                        shader: this);
                    
                }
            }
            else
            {
                bool draw = false;

                for (int i = 0; i < 6; i++)
                {
                    // render the scene to all cubemap faces
                    cubeMapFace = (CubeMapFace)i;

                    switch (cubeMapFace)
                    {
                        case CubeMapFace.NegativeX:
                            lightViewProjection = light.LightViewProjectionNegativeX;
                            break;
                        case CubeMapFace.NegativeY:
                            lightViewProjection = light.LightViewProjectionNegativeY;
                            break;
                        case CubeMapFace.NegativeZ:
                            lightViewProjection = light.LightViewProjectionNegativeZ;
                            break;
                        case CubeMapFace.PositiveX:
                            lightViewProjection = light.LightViewProjectionPositiveX;
                            break;
                        case CubeMapFace.PositiveY:
                            lightViewProjection = light.LightViewProjectionPositiveY;
                            break;
                        case CubeMapFace.PositiveZ:
                            lightViewProjection = light.LightViewProjectionPositiveZ;
                            break;
                    }

                    if (_boundingFrustumShadow != null) _boundingFrustumShadow.Matrix = lightViewProjection;
                    else _boundingFrustumShadow = new BoundingFrustum(lightViewProjection);

                    bool hasAnyObjectMoved = meshMaterialLibrary.FrustumCulling(entities, _boundingFrustumShadow, false, light.Position);
                    
                    if (!hasAnyObjectMoved) continue;

                    if (!draw)
                    {

                        graphicsDevice.SetRenderTarget(light.ShadowMap);
                        draw = true;
                    }

                    graphicsDevice.Viewport = new Viewport(0, light.ShadowResolution * (int)cubeMapFace, light.ShadowResolution, light.ShadowResolution);
                    
                    //_graphicsDevice.Clear(Color.TransparentBlack);
                    //_graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 0, 0);
                    graphicsDevice.ScissorRectangle = new Rectangle(0, light.ShadowResolution * (int)cubeMapFace, light.ShadowResolution, light.ShadowResolution);

                    meshMaterialLibrary.Draw(renderType: MeshMaterialLibrary.RenderType.ShadowOmnidirectional,
                        viewProjection: lightViewProjection,
                        lightViewPointChanged: light.HasChanged,
                        hasAnyObjectMoved: true,
                        shader: this);
                }
            }
        }

        /// <summary>
        /// Only one shadow map needed for a directional light
        /// </summary>
        /// <param name="lightSource"></param>
        /// <param name="shadowResolution"></param>
        /// <param name="meshMaterialLibrary"></param>
        /// <param name="entities"></param>
        private void CreateShadowMapDirectionalLight(GraphicsDevice graphicsDevice, DirectionalLightSource lightSource, int shadowResolution, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities)
        {
            //Create a renderTarget if we don't have one yet
            if (lightSource.ShadowMap == null)
            {
                //if (lightSource.ShadowFiltering != DirectionalLightSource.ShadowFilteringTypes.VSM)
                //{
                    lightSource.ShadowMap = new RenderTarget2D(graphicsDevice, shadowResolution, shadowResolution, false,
                        SurfaceFormat.HalfSingle, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                //}
                //else //For a VSM shadowMap we need 2 components
                //{
                //    lightSource.ShadowMap = new RenderTarget2D(_graphicsDevice, shadowResolution, shadowResolution, false,
                //       SurfaceFormat.Vector2, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                //}
            }
            
            if (lightSource.HasChanged)
            {
                Matrix lightProjection = Matrix.CreateOrthographic(lightSource.ShadowSize, lightSource.ShadowSize,
                    -lightSource.ShadowDepth, lightSource.ShadowDepth);
                Matrix lightView = Matrix.CreateLookAt(lightSource.Position, lightSource.Position + lightSource.Direction, Vector3.Down);

                lightSource.LightView = lightView;
                lightSource.LightViewProjection = lightView * lightProjection;

                _boundingFrustumShadow = new BoundingFrustum(lightSource.LightViewProjection);

                graphicsDevice.SetRenderTarget(lightSource.ShadowMap);
                graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1, 0);

                meshMaterialLibrary.FrustumCulling(entities, _boundingFrustumShadow, true, lightSource.Position);

                // Rendering!
                _FarClip.SetValue(lightSource.ShadowDepth);
                _SizeBias.SetValue(GameSettings.ShadowBias * 2048 / lightSource.ShadowResolution);

                meshMaterialLibrary.Draw(MeshMaterialLibrary.RenderType.ShadowLinear,
                    lightSource.LightViewProjection, lightSource.HasChanged, false, false, 0, lightSource.LightView, shader: this);
            }
            else
            {
                _boundingFrustumShadow = new BoundingFrustum(lightSource.LightViewProjection);

                bool hasAnyObjectMoved = meshMaterialLibrary.FrustumCulling(entities: entities, boundingFrustrum: _boundingFrustumShadow, hasCameraChanged: false, cameraPosition: lightSource.Position);

                if (!hasAnyObjectMoved) return;

                meshMaterialLibrary.FrustumCulling(entities: entities, boundingFrustrum: _boundingFrustumShadow, hasCameraChanged: true, cameraPosition: lightSource.Position);

                graphicsDevice.SetRenderTarget(lightSource.ShadowMap);
                graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1, 0);

                _FarClip.SetValue(lightSource.ShadowDepth);
                _SizeBias.SetValue(GameSettings.ShadowBias * 2048 / lightSource.ShadowResolution);

                meshMaterialLibrary.Draw(MeshMaterialLibrary.RenderType.ShadowLinear,
                    lightSource.LightViewProjection, false, true, false, 0, lightSource.LightView, shader: this);
            }

            //Blur!
            //if (lightSource.ShadowFiltering == DirectionalLightSource.ShadowFilteringTypes.VSM)
            //{
            //    lightSource.ShadowMap = _gaussianBlur.DrawGaussianBlur(lightSource.ShadowMap);
            //}

        }

        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            _WorldViewProj.SetValue(localWorldMatrix * viewProjection);

            switch (_pass)
            {
                case Passes.Directional:
                    _WorldView.SetValue(localWorldMatrix * (Matrix)view);
                    _linearPass.Apply();
                    break;
                case Passes.Omnidirectional:
                    _World.SetValue(localWorldMatrix);
                    _distancePass.Apply();
                    break;
            }
        }
    }
}
