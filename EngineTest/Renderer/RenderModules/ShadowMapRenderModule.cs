using System;
using System.Collections.Generic;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.RenderModules.Default;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using DirectionalLight = DeferredEngine.Entities.DirectionalLight;

namespace DeferredEngine.Renderer.RenderModules
{
    public class ShadowMapRenderModule : IRenderModule
    {
        private Effect _shader;

        private EffectParameter _WorldViewProj;
        private EffectParameter _WorldView;
        private EffectParameter _World;
        private EffectParameter _LightPositionWS;
        private EffectParameter _FarClip;
        private EffectParameter _SizeBias;
        private EffectParameter _MaskTexture;

        //Linear = VS Depth -> used for directional lights
        private EffectPass _linearPass;

        //Distance = distance(pixel, light) -> used for omnidirectional lights
        private EffectPass _distancePass;
        private EffectPass _distanceAlphaPass;

        private Passes _pass;

        private BoundingFrustum _boundingFrustumShadow;

        private enum Passes
        {
            Directional,
            Omnidirectional,
            OmnidirectionalAlpha
        };

        public ShadowMapRenderModule(ContentManager content, string shaderPath)
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
            _MaskTexture = _shader.Parameters["MaskTexture"];

            _linearPass = _shader.Techniques["DrawLinearDepth"].Passes[0];
            _distancePass = _shader.Techniques["DrawDistanceDepth"].Passes[0];
            _distanceAlphaPass = _shader.Techniques["DrawDistanceDepthAlpha"].Passes[0];
        }

        public void Load(ContentManager content, string shaderPath)
        {
            _shader = content.Load<Effect>(shaderPath);
        }

        public void Draw(GraphicsDevice graphicsDevice, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities, List<PointLight> pointLights, List<DirectionalLight> dirLights, Camera camera)
        {
            _pass = Passes.Omnidirectional;

            //Go through all our point lights
            for (int index = 0; index < pointLights.Count; index++)
            {
                PointLight light = pointLights[index];

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
                DirectionalLight light = dirLights[index];
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
        private void CreateShadowCubeMap(GraphicsDevice graphicsDevice, PointLight light, int size, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities)
        {
            //For VSM we need 2 channels, -> Vector2
            //todo: check if we need preserve contents
            if (light.ShadowMap == null)
                light.ShadowMap = new RenderTarget2D(graphicsDevice, size, size * 6, false, SurfaceFormat.HalfSingle, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

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
                        renderModule: this);
                    
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
                        renderModule: this);
                }
            }
        }

        /// <summary>
        /// Only one shadow map needed for a directional light
        /// </summary>
        /// <param name="light"></param>
        /// <param name="shadowResolution"></param>
        /// <param name="meshMaterialLibrary"></param>
        /// <param name="entities"></param>
        private void CreateShadowMapDirectionalLight(GraphicsDevice graphicsDevice, DirectionalLight light, int shadowResolution, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities)
        {
            //Create a renderTarget if we don't have one yet
            if (light.ShadowMap == null)
            {
                //if (lightSource.ShadowFiltering != DirectionalLightSource.ShadowFilteringTypes.VSM)
                //{
                    light.ShadowMap = new RenderTarget2D(graphicsDevice, shadowResolution, shadowResolution, false,
                        SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                //}
                //else //For a VSM shadowMap we need 2 components
                //{
                //    lightSource.ShadowMap = new RenderTarget2D(_graphicsDevice, shadowResolution, shadowResolution, false,
                //       SurfaceFormat.Vector2, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                //}
            }
            
            if (light.HasChanged)
            {
                Matrix lightProjection = Matrix.CreateOrthographic(light.ShadowSize, light.ShadowSize,
                    -light.ShadowDepth, light.ShadowDepth);
                Matrix lightView = Matrix.CreateLookAt(light.Position, light.Position + light.Direction, Vector3.Down);

                light.LightView = lightView;
                light.LightViewProjection = lightView * lightProjection;

                _boundingFrustumShadow = new BoundingFrustum(light.LightViewProjection);

                graphicsDevice.SetRenderTarget(light.ShadowMap);
                graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1, 0);

                meshMaterialLibrary.FrustumCulling(entities, _boundingFrustumShadow, true, light.Position);

                // Rendering!
                _FarClip.SetValue(light.ShadowDepth);
                _SizeBias.SetValue(GameSettings.ShadowBias * 2048 / light.ShadowResolution);

                meshMaterialLibrary.Draw(MeshMaterialLibrary.RenderType.ShadowLinear,
                    light.LightViewProjection, light.HasChanged, false, false, 0, light.LightView, renderModule: this);
            }
            else
            {
                _boundingFrustumShadow = new BoundingFrustum(light.LightViewProjection);

                bool hasAnyObjectMoved = meshMaterialLibrary.FrustumCulling(entities: entities, boundingFrustrum: _boundingFrustumShadow, hasCameraChanged: false, cameraPosition: light.Position);

                if (!hasAnyObjectMoved) return;

                meshMaterialLibrary.FrustumCulling(entities: entities, boundingFrustrum: _boundingFrustumShadow, hasCameraChanged: true, cameraPosition: light.Position);

                graphicsDevice.SetRenderTarget(light.ShadowMap);
                graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1, 0);

                _FarClip.SetValue(light.ShadowDepth);
                _SizeBias.SetValue(GameSettings.ShadowBias * 2048 / light.ShadowResolution);

                meshMaterialLibrary.Draw(MeshMaterialLibrary.RenderType.ShadowLinear,
                    light.LightViewProjection, false, true, false, 0, light.LightView, renderModule: this);
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
                case Passes.OmnidirectionalAlpha:
                    _World.SetValue(localWorldMatrix);
                    _distanceAlphaPass.Apply();
                    break;
            }
        }

        public void SetMaterialSettings(MaterialEffect material, MeshMaterialLibrary.RenderType renderType)
        {
            if (renderType == MeshMaterialLibrary.RenderType.ShadowOmnidirectional)
            {
                //Check if we have a mask texture
                if (material.HasMask)
                {
                    _pass = Passes.OmnidirectionalAlpha;
                    _MaskTexture.SetValue(material.Mask);

                }
                else
                {
                    _pass = Passes.Omnidirectional;
                }

            }
        }
    }
}
