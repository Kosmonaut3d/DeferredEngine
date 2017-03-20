using System;
using System.Collections.Generic;
using System.Diagnostics;
using DeferredEngine.Entities;
using DeferredEngine.Main;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.RenderModules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Renderer
{
    public class Renderer
    {
        #region VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Graphics & Helpers
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private QuadRenderer _quadRenderer;
        private GaussianBlur _gaussianBlur;
        private EditorRender _editorRender;
        private CPURayMarch _cpuRayMarch;
        private BloomFilter _bloomFilter;
        private LightRenderer _lightRenderer;

        //Assets
        private Assets _assets;

        //View Projection
        private bool _viewProjectionHasChanged;
        private Vector3 _inverseResolution;

        //Temporal Anti Aliasing
        private bool _temporalAAOffFrame = true;
        private Vector3[] _haltonSequence;
        private int _haltonSequenceIndex = -1;
        private const int HaltonSequenceLength = 16;

        private int frameCounter = 0;
        
        //Projection Matrices and derivates used in shaders
        private Matrix _view;
        private Matrix _inverseView;
        private Matrix _viewIT;
        private Matrix _projection;
        private Matrix _viewProjection;
        private Matrix _staticViewProjection;
        private Matrix _inverseViewProjection;
        private Matrix _previousViewProjection;
        private Matrix _currentViewToPreviousViewProjection;

        //Bounding Frusta of our view projection, to calculate which objects are inside the view
        private BoundingFrustum _boundingFrustum;
        private BoundingFrustum _boundingFrustumShadow;

        //Used for the view space directions in our shaders. Far edges of our view frustum
        private readonly Vector3[] _cornersWorldSpace = new Vector3[8];
        private readonly Vector3[] _cornersViewSpace = new Vector3[8];
        private readonly Vector3[] _currentFrustumCorners = new Vector3[4];

        //Checkvariables to see which console variables have changed from the frame before
        private float _g_FarClip;
        private float _supersampling = 1;
        private bool _hologramDraw;
        private int _forceShadowFiltering;
        private bool _forceShadowSS;
        private bool _ssr = true;
        private bool _g_SSReflectionNoise;

        //Render modes
        public enum RenderModes { Albedo, Normal, Depth, Deferred, Diffuse, Specular, Hologram,
            SSAO,
            Emissive,
            DirectionalShadow,
            SSR,
            Volumetric,
            HDR,
        }

        //Render targets
        private RenderTarget2D _renderTargetAlbedo;
        private readonly RenderTargetBinding[] _renderTargetBinding = new RenderTargetBinding[3];

        private RenderTarget2D _renderTargetDepth;
        private RenderTarget2D _renderTargetNormal;
        private RenderTarget2D _renderTargetDiffuse;
        private RenderTarget2D _renderTargetSpecular;
        private RenderTarget2D _renderTargetVolume;
        private readonly RenderTargetBinding[] _renderTargetLightBinding = new RenderTargetBinding[3];

        private RenderTarget2D _renderTargetFinal;
        private readonly RenderTargetBinding[] _renderTargetFinalBinding = new RenderTargetBinding[1];
        private RenderTarget2D _renderTargetFinal8Bit;
        private readonly RenderTargetBinding[] _renderTargetFinal8BitBinding = new RenderTargetBinding[1];

        //TAA
        private RenderTarget2D _renderTargetTAA_1;
        private RenderTarget2D _renderTargetTAA_2;
        
        private RenderTarget2D _renderTargetScreenSpaceEffectReflection;

        private RenderTarget2D _renderTargetHologram;

        private RenderTarget2D _renderTargetSSAOEffect;

        private RenderTarget2D _renderTargetScreenSpaceEffectUpsampleBlurVertical;

        private RenderTarget2D _renderTargetScreenSpaceEffectUpsampleBlurHorizontal;
        private RenderTarget2D _renderTargetScreenSpaceEffectBlurFinal;

        private RenderTarget2D _renderTargetEmissive;

        //Cubemap
        private RenderTargetCube _renderTargetCubeMap;
        

        //Performance Profiler

        private readonly Stopwatch _performanceTimer = new Stopwatch();
        private long _performancePreviousTime;

        //TEST
        private TestShadow testShadow;

        #endregion

        #region FUNCTIONS

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            #region BASE FUNCTIONS

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            //  BASE FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initialize variables
        /// </summary>
        /// <param name="content"></param>
        public void Load(ContentManager content)
        {
            _bloomFilter = new BloomFilter();
            _bloomFilter.Load(content, _quadRenderer);

            _inverseResolution = new Vector3(1.0f / GameSettings.g_ScreenWidth, 1.0f / GameSettings.g_ScreenHeight, 0);
        }

        /// <summary>
        /// Initialize all our rendermodules and helpers. Done after the Load() function
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="assets"></param>
        public void Initialize(GraphicsDevice graphicsDevice, Assets assets)
        {
            _graphicsDevice = graphicsDevice;
            _quadRenderer = new QuadRenderer();
            _spriteBatch = new SpriteBatch(graphicsDevice);
            _gaussianBlur = new GaussianBlur();
            _gaussianBlur.Initialize(graphicsDevice);

            _editorRender = new EditorRender();
            _editorRender.Initialize(graphicsDevice, assets);

            _cpuRayMarch = new CPURayMarch();
            _cpuRayMarch.Initialize(_graphicsDevice);

            _bloomFilter.Initialize(_graphicsDevice, GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight);

            _lightRenderer = new LightRenderer();
            _lightRenderer.Initialize(graphicsDevice, _quadRenderer, assets);

            testShadow = new TestShadow();
            testShadow.Initialize(graphicsDevice);
            
            _assets = assets;

            //Apply some base settings to overwrite shader defaults with game settings defaults
            GameSettings.ApplySettings();

            Shaders.ScreenSpaceReflectionParameter_NoiseMap.SetValue(_assets.NoiseMap);
            SetUpRenderTargets(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight, false);
            
        }

        /// <summary>
        /// Update our function
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="isActive"></param>
        public void Update(GameTime gameTime, bool isActive)
        {
            if (!isActive) return;
            _editorRender.Update(gameTime);
        }

        #endregion

            #region RENDER FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            //  RENDER FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////////

                #region MAIN DRAW FUNCTIONS
                ////////////////////////////////////////////////////////////////////////////////////////////////////
                //  MAIN DRAW FUNCTIONS
                ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Main Draw function of the game
        /// </summary>
        /// <param name="camera">view point of the renderer</param>
        /// <param name="meshMaterialLibrary">a class that has stored all our mesh data</param>
        /// <param name="entities">entities and their properties</param>
        /// <param name="pointLights"></param>
        /// <param name="directionalLights"></param>
        /// <param name="editorData">The data passed from our editor logic</param>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public EditorLogic.EditorReceivedData Draw(Camera camera, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities, List<PointLightSource> pointLights, List<DirectionalLightSource> directionalLights, EnvironmentSample envSample, EditorLogic.EditorSendData editorData, GameTime gameTime)
        {
            frameCounter++;

            //Reset the stat counter, so we can count stats/information for this frame only
            ResetStats();
            
            //Update the mesh data for changes in physics etc.
            meshMaterialLibrary.FrustumCullingStartFrame(entities);

            //Check if we changed some drastic stuff for which we need to reload some elements
            CheckRenderChanges(directionalLights);

            //Render ShadowMaps
            DrawShadowMaps(meshMaterialLibrary, entities, pointLights, directionalLights, camera);

            //Render EnvironmentMaps
            //We do this either when pressing C or at the start of the program (_renderTargetCube == null) or when the game settings want us to do it every frame
            if (/*(Input.WasKeyPressed(Keys.C) && !DebugScreen.ConsoleOpen) ||*/ envSample.NeedsUpdate || GameSettings.g_EnvironmentMappingEveryFrame /*|| _renderTargetCubeMap == null*/)
            {
                DrawCubeMap(envSample.Position, meshMaterialLibrary, entities, pointLights, directionalLights, 300, gameTime, camera);
                envSample.NeedsUpdate = false;
            }
            
            //Update our view projection matrices if the camera moved
            UpdateViewProjection(camera, meshMaterialLibrary, entities);

            //Set up our deferred renderer
            SetUpGBuffer();

            //Draw our meshes to the G Buffer
            DrawGBuffer(meshMaterialLibrary);

            //Draw Hologram projections to a different render target
            DrawHolograms(meshMaterialLibrary);
            
            //Draw Screen Space reflections to a different render target
            DrawScreenSpaceReflections(gameTime);

            //SSAO
            DrawScreenSpaceAmbientOcclusion(camera);

            //Screen space shadows for directional lights to an offscreen render target
            DrawScreenSpaceDirectionalShadow(directionalLights);

            //Upsample/blur our SSAO / screen space shadows
            DrawBilateralBlur();

            //Light the scene
            _lightRenderer.DrawLights(pointLights, directionalLights, camera.Position, gameTime, _renderTargetLightBinding, _renderTargetDiffuse);

            //Draw the environment cube map as a fullscreen effect on all meshes
            DrawEnvironmentMap();

            //Draw emissive materials on an offscreen render target
            DrawEmissiveEffect(camera, meshMaterialLibrary, gameTime);

            //Compose the scene by combining our lighting data with the gbuffer data
            Compose();

            //Do Bloom
            DrawBloom();

            //Compose the image and add information from previous frames to apply temporal super sampling
            CombineTemporalAntialiasing();

            DrawTestShadow(camera);
                
            //Draw the elements that we are hovering over with outlines
            if(GameSettings.Editor_enable && GameStats.e_EnableSelection)
                _editorRender.DrawIds(meshMaterialLibrary, pointLights, directionalLights, envSample, _staticViewProjection, _view, editorData);

            //Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
            RenderMode();

            //Additional editor elements that overlay our screen
            if (GameSettings.Editor_enable && GameStats.e_EnableSelection)
            {
                DrawMapToScreenToFullScreen(_editorRender.GetOutlines(), BlendState.Additive);
                _editorRender.DrawEditorElements(meshMaterialLibrary, pointLights, directionalLights, envSample, _staticViewProjection, _view, editorData);
                
                if (editorData.SelectedObject != null)
                {
                    if (editorData.SelectedObject is DirectionalLightSource)
                    {
                        int size = 512;
                        DirectionalLightSource light = (DirectionalLightSource)editorData.SelectedObject;
                        if (light.DrawShadows)
                        {
                            _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp);
                            _spriteBatch.Draw(light.ShadowMap, new Rectangle(0, GameSettings.g_ScreenHeight - size, size, size), Color.White);
                            _spriteBatch.End();
                        }
                    }
                }
            }

            //Debug ray marching
            CpuRayMarch(camera);

            //Draw (debug) lines
            LineHelperManager.Draw(_graphicsDevice, _staticViewProjection);

            //Set up the frustum culling for the next frame
            meshMaterialLibrary.FrustumCullingFinalizeFrame(entities);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileTotalRender = performanceCurrentTime;
            }

            //return data we have recovered from the editor id, so we know what entity gets hovered/clicked on and can manipulate in the update function
            return new EditorLogic.EditorReceivedData
            {
                HoveredId =  _editorRender.GetHoveredId(),
                ViewMatrix =  _view,
                ProjectionMatrix =  _projection
            };
        }

        /// <summary>
        /// Another draw function, but this time for cubemaps. Doesn't need all the stuff we have in the main draw function
        /// </summary>
        /// <param name="origin">from where do we render the cubemap</param>
        /// <param name="meshMaterialLibrary"></param>
        /// <param name="entities"></param>
        /// <param name="pointLights"></param>
        /// <param name="dirLights"></param>
        /// <param name="farPlane"></param>
        /// <param name="gameTime"></param>
        /// <param name="camera"></param>
        private void DrawCubeMap(Vector3 origin, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities, List<PointLightSource> pointLights, List<DirectionalLightSource> dirLights, float farPlane, GameTime gameTime, Camera camera)
        {
            //If our cubemap is not yet initialized, create a new one
            if (_renderTargetCubeMap == null)
            {
                //Create a new cube map
                _renderTargetCubeMap = new RenderTargetCube(_graphicsDevice, GameSettings.g_CubeMapResolution, true, SurfaceFormat.HalfVector4,
                    DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

                //Set this cubemap in the shader of the environment map
                Shaders.deferredEnvironmentParameter_ReflectionCubeMap.SetValue(_renderTargetCubeMap);
            }

            //Set up all the base rendertargets with the resolution of our cubemap
            SetUpRenderTargets(GameSettings.g_CubeMapResolution, GameSettings.g_CubeMapResolution, false);

            //We don't want to use SSAO in this cubemap
            Shaders.DeferredComposeEffectParameter_UseSSAO.SetValue(false);
            
            //Create our projection, which is a basic pyramid
            _projection = Matrix.CreatePerspectiveFieldOfView((float)(Math.PI / 2), 1, 1, farPlane);

            //Now we need to actually render for each cubemapface (6 direcetions)
            for (int i = 0; i < 6; i++)
            {
                // render the scene to all cubemap faces
                CubeMapFace cubeMapFace = (CubeMapFace)i;
                switch (cubeMapFace)
                {
                    case CubeMapFace.NegativeX:
                        {
                            _view = Matrix.CreateLookAt(origin, origin + Vector3.Left, Vector3.Up);
                            break;
                        }
                    case CubeMapFace.NegativeY:
                        {
                            _view = Matrix.CreateLookAt(origin, origin + Vector3.Down, Vector3.Forward);
                            break;
                        }
                    case CubeMapFace.NegativeZ:
                        {
                            _view = Matrix.CreateLookAt(origin, origin + Vector3.Backward, Vector3.Up);
                            break;
                        }
                    case CubeMapFace.PositiveX:
                        {
                            _view = Matrix.CreateLookAt(origin, origin + Vector3.Right, Vector3.Up);
                            break;
                        }
                    case CubeMapFace.PositiveY:
                        {
                            _view = Matrix.CreateLookAt(origin, origin + Vector3.Up, Vector3.Backward);
                            break;
                        }
                    case CubeMapFace.PositiveZ:
                        {
                            _view = Matrix.CreateLookAt(origin, origin + Vector3.Forward, Vector3.Up);
                            break;
                        }
                }

                //Create our projection matrices
                _inverseView = Matrix.Invert(_view);
                _viewProjection = _view * _projection;
                _inverseViewProjection = Matrix.Invert(_viewProjection);
                _viewIT = Matrix.Transpose(_inverseView);

                //Pass these values to our shader
                Shaders.ScreenSpaceEffectParameter_InverseViewProjection.SetValue(_inverseView);
                Shaders.deferredPointLightParameter_InverseView.SetValue(_inverseView);

                //yep we changed
                _viewProjectionHasChanged = true;

                if (_boundingFrustum == null) _boundingFrustum = new BoundingFrustum(_viewProjection);
                else _boundingFrustum.Matrix = _viewProjection;
                ComputeFrustumCorners(_boundingFrustum);

                _lightRenderer.UpdateViewProjection(_boundingFrustum, _viewProjectionHasChanged, _view, _inverseView, _viewIT, _projection, _viewProjection, _inverseViewProjection);

                //Base stuff, for description look in Draw()
                meshMaterialLibrary.FrustumCulling(entities, _boundingFrustum, true, origin);
                SetUpGBuffer();
                DrawGBuffer(meshMaterialLibrary);

                bool volumeEnabled = GameSettings.g_VolumetricLights;
                GameSettings.g_VolumetricLights = false;
                _lightRenderer.DrawLights(pointLights, dirLights, origin, gameTime, _renderTargetLightBinding, _renderTargetDiffuse);

                GameSettings.g_VolumetricLights = volumeEnabled;

                //We don't use temporal AA obviously for the cubemap
                bool tempAa = GameSettings.g_TemporalAntiAliasing;
                GameSettings.g_TemporalAntiAliasing = false;
                
                //Shaders.DeferredCompose.CurrentTechnique = Shaders.DeferredComposeTechnique_NonLinear;
                Compose();
                //Shaders.DeferredCompose.CurrentTechnique = GameSettings.g_SSReflection
                //    ? Shaders.DeferredComposeTechnique_Linear
                //    : Shaders.DeferredComposeTechnique_NonLinear;
                GameSettings.g_TemporalAntiAliasing = tempAa;
                DrawMapToScreenToCube(_renderTargetFinal, _renderTargetCubeMap, cubeMapFace);
            }
            Shaders.DeferredComposeEffectParameter_UseSSAO.SetValue(GameSettings.ssao_Active);

            //Change RTs back to normal
            SetUpRenderTargets(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight, false);

            _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectReflection);
            _graphicsDevice.Clear(Color.TransparentBlack);

            //Our camera has changed we need to reinitialize stuff because we used a different camera in the cubemap render
            camera.HasChanged = true;

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawCubeMap = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

                #endregion

                #region DEFERRED RENDERING FUNCTIONS
                /////////////////////////////////////////////////////////////////////////////////////////////////////
                //  DEFERRED RENDERING FUNCTIONS, IN ORDER OF USAGE
                ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset our stat counting for this frame
        /// </summary>
        private void ResetStats()
        {
            GameStats.MaterialDraws = 0;
            GameStats.MeshDraws = 0;
            GameStats.LightsDrawn = 0;
            GameStats.shadowMaps = 0;
            GameStats.activeShadowMaps = 0;
            GameStats.EmissiveMeshDraws = 0;

            //Profiler
            if (GameSettings.d_profiler)
            {
                _performanceTimer.Restart();
                _performancePreviousTime = 0;
            }
            else if (_performanceTimer.IsRunning)
            {
                _performanceTimer.Stop();
            }
        }

        /// <summary>
        /// Check whether any GameSettings have changed that need setup
        /// </summary>
        /// <param name="dirLights"></param>
        private void CheckRenderChanges(List<DirectionalLightSource> dirLights)
        {
            if (Math.Abs(_g_FarClip - GameSettings.g_FarPlane) > 0.0001f)
            {
                _g_FarClip = GameSettings.g_FarPlane;
                Shaders.GBufferEffectParameter_FarClip.SetValue(_g_FarClip);
                Shaders.deferredPointLightParameter_FarClip.SetValue(_g_FarClip);
                Shaders.BillboardEffectParameter_FarClip.SetValue(_g_FarClip);
                Shaders.ScreenSpaceReflectionParameter_FarClip.SetValue(_g_FarClip);
                Shaders.ReconstructDepthParameter_FarClip.SetValue(_g_FarClip);
            }

            if (_g_SSReflectionNoise != GameSettings.g_SSReflectionNoise)
            {
                _g_SSReflectionNoise = GameSettings.g_SSReflectionNoise;
                if (!_g_SSReflectionNoise) Shaders.ScreenSpaceReflectionParameter_Time.SetValue(0.0f);
            }

            //Check if supersampling has changed
            if (Math.Abs(_supersampling - GameSettings.g_supersampling) > 0.0001f)
            {
                _supersampling = GameSettings.g_supersampling;
                SetUpRenderTargets(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight, false);
            }

            if (_hologramDraw != GameSettings.g_HologramDraw)
            {
                _hologramDraw = GameSettings.g_HologramDraw;

                if (!_hologramDraw)
                {
                    _graphicsDevice.SetRenderTarget(_renderTargetHologram);
                    _graphicsDevice.Clear(Color.Black);
                }
            }

            if (_forceShadowFiltering != GameSettings.g_ShadowForceFiltering)
            {
                _forceShadowFiltering = GameSettings.g_ShadowForceFiltering;

                foreach (DirectionalLightSource light in dirLights)
                {
                    if (light.ShadowMap != null) light.ShadowMap.Dispose();
                    light.ShadowMap = null;

                    light.ShadowFiltering = (DirectionalLightSource.ShadowFilteringTypes)(_forceShadowFiltering - 1);

                    light.HasChanged = true;
                }
            }

            if (_forceShadowSS != GameSettings.g_ShadowForceScreenSpace)
            {
                _forceShadowSS = GameSettings.g_ShadowForceScreenSpace;

                foreach (DirectionalLightSource light in dirLights)
                {

                    light.ScreenSpaceShadowBlur = _forceShadowSS;

                    light.HasChanged = true;
                }
            }

            if (_ssr != GameSettings.g_SSReflection)
            {
                _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectReflection);
                _graphicsDevice.Clear(Color.TransparentBlack);

                _ssr = GameSettings.g_SSReflection;
            }

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileRenderChanges = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Draw our shadow maps from the individual lights. Check if something has changed first, otherwise leave as it is
        /// </summary>
        /// <param name="meshMaterialLibrary"></param>
        /// <param name="entities"></param>
        /// <param name="pointLights"></param>
        /// <param name="dirLights"></param>
        /// <param name="camera"></param>
        private void DrawShadowMaps(MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities, List<PointLightSource> pointLights, List<DirectionalLightSource> dirLights, Camera camera)
        {
            //Don't render for the first frame, we need a guideline first
            if (_boundingFrustum == null) UpdateViewProjection(camera, meshMaterialLibrary, entities);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            GameStats.ShadowsBlurred = 0;

            //Go through all our point lights
            for (int index = 0; index < pointLights.Count; index++)
            {
                //DISABLED
                
                PointLightSource light = pointLights[index];

                if (!light.IsEnabled) continue;

                //If we don't see the light we shouldn't update. This is actually wrong, can lead to mistakes,
                //if we implement it like this we should rerender once we enter visible space again.
                //if (_boundingFrustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint)
                //{
                //    continue;
                //}

                if (light.DrawShadow)
                {
                    //A poing light has 6 shadow maps, add that to our stat counter. These are total shadow maps, not updated ones
                    GameStats.shadowMaps += 6;
                    
                    //Update if we didn't initialize yet or if we are dynamic
                    if (!light.StaticShadows || light.shadowMapCube == null)
                    {
                        CreateShadowCubeMap(light, light.ShadowResolution, meshMaterialLibrary, entities);
                        
                        light.HasChanged = false;
                        camera.HasChanged = true;
                    }


                }

                //Soft VSM Shadow blur

            }

            int dirLightShadowed = 0;
            for (int index = 0; index < dirLights.Count; index++)
            {
                DirectionalLightSource light = dirLights[index];
                if (!light.IsEnabled) continue;

                if (light.DrawShadows)
                {
                    GameStats.shadowMaps += 1;

                    CreateShadowMapDirectionalLight(light, light.ShadowResolution, meshMaterialLibrary, entities);

                    camera.HasChanged = true;
                    light.HasChanged = false;

                    if (light.ScreenSpaceShadowBlur) dirLightShadowed++;
                }

                if (dirLightShadowed > 1)
                {
                    throw new NotImplementedException(
                        "Only one shadowed DirectionalLight with screen space blur is supported right now");
                }
            }

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawShadows = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }

        }

        /// <summary>
        /// Create the shadow map for each cubemapside, then combine into one cubemap
        /// </summary>
        /// <param name="light"></param>
        /// <param name="size"></param>
        /// <param name="meshMaterialLibrary"></param>
        /// <param name="entities"></param>
        private void CreateShadowCubeMap(PointLightSource light, int size, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities)
        {
            //For VSM we need 2 channels, -> Vector2
            if (light.shadowMapCube == null)
                light.shadowMapCube = new RenderTargetCube(_graphicsDevice, size, false, SurfaceFormat.Vector2, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            Matrix lightViewProjection = new Matrix();
            CubeMapFace cubeMapFace; // = CubeMapFace.NegativeX;

            if (light.HasChanged)
            {
                Matrix lightProjection = Matrix.CreatePerspectiveFieldOfView((float)(Math.PI / 2), 1, 1, light.Radius);
                Matrix lightView; // = identity

                //Reset the blur array
                light.faceBlurCount = new int[6];

                for (int i = 0; i < 6; i++)
                {
                    // render the scene to all cubemap faces
                    cubeMapFace = (CubeMapFace)i;
                    switch (cubeMapFace)
                    {
                        case CubeMapFace.NegativeX:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.Left, Vector3.Up);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionNegativeX = lightViewProjection;
                                break;
                            }
                        case CubeMapFace.NegativeY:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.Down,
                                    Vector3.Forward);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionNegativeY = lightViewProjection;
                                break;
                            }
                        case CubeMapFace.NegativeZ:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.Backward,
                                    Vector3.Up);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionNegativeZ = lightViewProjection;
                                break;
                            }
                        case CubeMapFace.PositiveX:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.Right, Vector3.Up);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionPositiveX = lightViewProjection;
                                break;
                            }
                        case CubeMapFace.PositiveY:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.Up,
                                    Vector3.Backward);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionPositiveY = lightViewProjection;
                                break;
                            }
                        case CubeMapFace.PositiveZ:
                            {
                                lightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.Forward, Vector3.Up);
                                lightViewProjection = lightView * lightProjection;
                                light.LightViewProjectionPositiveZ = lightViewProjection;
                                break;
                            }
                    }

                    if (_boundingFrustumShadow != null) _boundingFrustumShadow.Matrix = lightViewProjection;
                    else _boundingFrustumShadow = new BoundingFrustum(lightViewProjection);

                    meshMaterialLibrary.FrustumCulling(entities, _boundingFrustumShadow, true, light.Position);

                    // Rendering!

                    _graphicsDevice.SetRenderTarget(light.shadowMapCube, cubeMapFace);
                    _graphicsDevice.Clear(Color.TransparentBlack);
                    meshMaterialLibrary.Draw(renderType: MeshMaterialLibrary.RenderType.ShadowZW, 
                        graphicsDevice: _graphicsDevice,
                        viewProjection: lightViewProjection, 
                        lightViewPointChanged: true, 
                        hasAnyObjectMoved: light.HasChanged);

                    if (GameStats.ShadowsBlurred < GameSettings.g_ShadowBlurBudget && light.SoftShadowBlurAmount>0)
                    {
                        //i is cubeface

                        throw new NotImplementedException();

                        if (light.faceBlurCount[i] < light.SoftShadowBlurAmount)
                        {
                            GameStats.ShadowsBlurred++;
                            light.faceBlurCount[i]++;

                            _gaussianBlur.DrawGaussianBlur(light.shadowMapCube, cubeMapFace);
                        }
                    }
                }
            }
            else
            {
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

                    _graphicsDevice.SetRenderTarget(light.shadowMapCube, cubeMapFace);
                    //_graphicsDevice.Clear(Color.TransparentBlack);
                    //_graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 0, 0);

                    meshMaterialLibrary.Draw(renderType: MeshMaterialLibrary.RenderType.ShadowZW,
                        graphicsDevice: _graphicsDevice,
                        viewProjection: lightViewProjection,
                        lightViewPointChanged: light.HasChanged,
                        hasAnyObjectMoved: true);
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
        private void CreateShadowMapDirectionalLight(DirectionalLightSource lightSource, int shadowResolution, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities)
        {
            //Create a renderTarget if we don't have one yet
            if (lightSource.ShadowMap == null)
            {
                //if (lightSource.ShadowFiltering != DirectionalLightSource.ShadowFilteringTypes.VSM)
                //{
                    lightSource.ShadowMap = new RenderTarget2D(_graphicsDevice, shadowResolution, shadowResolution, false,
                        SurfaceFormat.HalfSingle, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                //}
                //else //For a VSM shadowMap we need 2 components
                //{
                //    lightSource.ShadowMap = new RenderTarget2D(_graphicsDevice, shadowResolution, shadowResolution, false,
                //       SurfaceFormat.Vector2, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                //}
            }

            //MeshMaterialLibrary.RenderType renderType = lightSource.ShadowFiltering == DirectionalLightSource.ShadowFilteringTypes.VSM
            //    ? MeshMaterialLibrary.RenderType.ShadowZW
            //    : MeshMaterialLibrary.RenderType.ShadowLinear;
            MeshMaterialLibrary.RenderType renderType = MeshMaterialLibrary.RenderType.ShadowLinear;

            if (lightSource.HasChanged)
            {
                Matrix lightProjection = Matrix.CreateOrthographic(lightSource.ShadowSize, lightSource.ShadowSize,
                    -lightSource.ShadowDepth, lightSource.ShadowDepth);
                Matrix lightView = Matrix.CreateLookAt(lightSource.Position, lightSource.Position + lightSource.Direction, Vector3.Down);

                lightSource.LightView = lightView;
                lightSource.LightViewProjection = lightView * lightProjection;

                _boundingFrustumShadow = new BoundingFrustum(lightSource.LightViewProjection);

                _graphicsDevice.SetRenderTarget(lightSource.ShadowMap);
                _graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1, 0);

                meshMaterialLibrary.FrustumCulling(entities, _boundingFrustumShadow, true, lightSource.Position);

                // Rendering!
                Shaders.virtualShadowMappingEffectParameter_FarClip.SetValue(lightSource.ShadowDepth);
                Shaders.virtualShadowMappingEffectParameter_SizeBias.SetValue(GameSettings.ShadowBias * 2048 / lightSource.ShadowResolution);

                meshMaterialLibrary.Draw(renderType, _graphicsDevice,
                    lightSource.LightViewProjection, lightSource.HasChanged, false, false, 0, lightSource.LightView);
            }
            else
            {
                _boundingFrustumShadow = new BoundingFrustum(lightSource.LightViewProjection);

                bool hasAnyObjectMoved = meshMaterialLibrary.FrustumCulling(entities: entities, boundingFrustrum: _boundingFrustumShadow, hasCameraChanged: false, cameraPosition: lightSource.Position);

                if (!hasAnyObjectMoved) return;

                meshMaterialLibrary.FrustumCulling(entities: entities, boundingFrustrum: _boundingFrustumShadow, hasCameraChanged: true, cameraPosition: lightSource.Position);

                _graphicsDevice.SetRenderTarget(lightSource.ShadowMap);
                _graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1, 0);

                Shaders.virtualShadowMappingEffectParameter_FarClip.SetValue(lightSource.ShadowDepth);
                Shaders.virtualShadowMappingEffectParameter_SizeBias.SetValue(GameSettings.ShadowBias * 2048 / lightSource.ShadowResolution);

                meshMaterialLibrary.Draw(renderType, _graphicsDevice,
                    lightSource.LightViewProjection, false, true, false, 0, lightSource.LightView);
            }

            //Blur!
            //if (lightSource.ShadowFiltering == DirectionalLightSource.ShadowFilteringTypes.VSM)
            //{
            //    lightSource.ShadowMap = _gaussianBlur.DrawGaussianBlur(lightSource.ShadowMap);
            //}

        }

        /// <summary>
        /// Create the projection matrices
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="meshMaterialLibrary"></param>
        /// <param name="entities"></param>
        private void UpdateViewProjection(Camera camera, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities)
        {
            _viewProjectionHasChanged = camera.HasChanged;

            //alternate frames with temporal aa
            if (GameSettings.g_TemporalAntiAliasing)
            {
                _viewProjectionHasChanged = true;
                _temporalAAOffFrame = !_temporalAAOffFrame;
            }

            //If the camera didn't do anything we don't need to update this stuff
            if (_viewProjectionHasChanged)
            {
                //We have processed the change, now setup for next frame as false
                camera.HasChanged = false;
                camera.HasMoved = false;

                //View matrix
                _view = Matrix.CreateLookAt(camera.Position, camera.Lookat, camera.Up);
                _inverseView = Matrix.Invert(_view);

                _viewIT = Matrix.Transpose(_inverseView);

                Shaders.deferredPointLightParameter_InverseView.SetValue(_inverseView);

                _projection = Matrix.CreatePerspectiveFieldOfView(camera.FieldOfView,
                    GameSettings.g_ScreenWidth / (float)GameSettings.g_ScreenHeight, 1, GameSettings.g_FarPlane);
                
                Shaders.GBufferEffectParameter_Camera.SetValue(camera.Position);

                _viewProjection = _view * _projection;

                //this is the unjittered viewProjection. For some effects we don't want the jittered one
                _staticViewProjection = _viewProjection;

                //Transformation for TAA - from current view back to the old view projection
                _currentViewToPreviousViewProjection = Matrix.Invert(_view) * _previousViewProjection;
                
                //Temporal AA
                if (GameSettings.g_TemporalAntiAliasing)
                {
                    switch (GameSettings.g_TemporalAntiAliasingJitterMode)
                    {
                        case 0: //2 frames, just basic translation. Worst taa implementation. Not good with the continous integration used
                        {
                            float translation = _temporalAAOffFrame ? 0.5f : -0.5f;
                            _viewProjection = _viewProjection *
                                              Matrix.CreateTranslation(new Vector3(translation / GameSettings.g_ScreenWidth,
                                                  translation / GameSettings.g_ScreenHeight, 0));
                        }
                            break;
                        case 1: // Just random translation
                        {
                            float randomAngle = FastRand.NextAngle();
                            Vector3 translation = new Vector3((float)Math.Sin(randomAngle) / GameSettings.g_ScreenWidth, (float)Math.Cos(randomAngle) / GameSettings.g_ScreenHeight, 0) * 0.5f;
                            _viewProjection = _viewProjection *
                                              Matrix.CreateTranslation(translation);

                        }
                            break;
                        case 2: // Halton sequence, default
                        {
                            Vector3 translation = GetHaltonSequence();
                            _viewProjection = _viewProjection *
                                              Matrix.CreateTranslation(translation);
                        }
                            break;
                    }
                }

                _previousViewProjection = _viewProjection;
                _inverseViewProjection = Matrix.Invert(_viewProjection);

                if (_boundingFrustum == null) _boundingFrustum = new BoundingFrustum(_staticViewProjection);
                else _boundingFrustum.Matrix = _staticViewProjection;

                // Compute the frustum corners for cheap view direction computation in shaders
                ComputeFrustumCorners(_boundingFrustum);
            }

            //We need to update whether or not entities are in our boundingFrustum and then cull them or not!
            meshMaterialLibrary.FrustumCulling(entities, _boundingFrustum, _viewProjectionHasChanged, camera.Position);

            _lightRenderer.UpdateViewProjection(_boundingFrustum, _viewProjectionHasChanged, _view, _inverseView, _viewIT, _projection, _viewProjection, _inverseViewProjection);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileUpdateViewProjection = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// The halton sequence is a good way to create good distribution
        /// I use a 2,3 sequence
        /// https://en.wikipedia.org/wiki/Halton_sequence
        /// </summary>
        /// <returns></returns>
        private Vector3 GetHaltonSequence()
        {
            //First time? Create the sequence
            if (_haltonSequence == null)
            {
                _haltonSequence = new Vector3[HaltonSequenceLength];
                for (int index = 0; index < HaltonSequenceLength; index++)
                {
                    for (int baseValue = 2; baseValue <= 3; baseValue++)
                    {
                        float result = 0;
                        float f = 1;
                        int i = index + 1;

                        while (i > 0)
                        {
                            f = f / baseValue;
                            result = result + f * (i % baseValue);
                            i = i / baseValue; //floor / int()
                        }

                        if (baseValue == 2)
                            _haltonSequence[index].X = (result - 0.5f) * 2 * _inverseResolution.X;
                        else
                            _haltonSequence[index].Y = (result - 0.5f) * 2 * _inverseResolution.Y;
                    }
                }
            }
            _haltonSequenceIndex++;
            if (_haltonSequenceIndex >= HaltonSequenceLength) _haltonSequenceIndex = 0;
            return _haltonSequence[_haltonSequenceIndex];
        }

        /// <summary>
        /// From https://jcoluna.wordpress.com/2011/01/18/xna-4-0-light-pre-pass/
        /// Compute the frustum corners for a camera.
        /// Its used to reconstruct the pixel position using only the depth value.
        /// Read here for more information
        /// http://mynameismjp.wordpress.com/2009/03/10/reconstructing-position-from-depth/
        /// </summary>
        /// <param name="cameraFrustum"></param>
        private void ComputeFrustumCorners(BoundingFrustum cameraFrustum)
        {
            cameraFrustum.GetCorners(_cornersWorldSpace);
            //this is the inverse of our camera transform
            Vector3.Transform(_cornersWorldSpace, ref _view, _cornersViewSpace); //put the frustum into view space
            for (int i = 0; i < 4; i++) //take only the 4 farthest points
            {
                _currentFrustumCorners[i] = _cornersViewSpace[i + 4];
            }
            Vector3 temp = _currentFrustumCorners[3];
            _currentFrustumCorners[3] = _currentFrustumCorners[2];
            _currentFrustumCorners[2] = temp;

            Shaders.deferredEnvironmentParameter_FrustumCorners.SetValue(_currentFrustumCorners);
            Shaders.ScreenSpaceReflectionParameter_FrustumCorners.SetValue(_currentFrustumCorners);
            Shaders.ScreenSpaceEffectParameter_FrustumCorners.SetValue(_currentFrustumCorners);
            Shaders.TemporalAntiAliasingEffect_FrustumCorners.SetValue(_currentFrustumCorners);
            Shaders.ReconstructDepthParameter_FrustumCorners.SetValue(_currentFrustumCorners);
            Shaders.deferredDirectionalLightParameterFrustumCorners.SetValue(_currentFrustumCorners);
            Shaders.TestShadowEffect_FrustumCorners.SetValue(_currentFrustumCorners);
        }

        /// <summary>
        /// Clear the GBuffer and prepare it for drawing the meshes
        /// </summary>
        private void SetUpGBuffer()
        {
            _graphicsDevice.SetRenderTargets(_renderTargetBinding);

            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            //Clear the GBuffer
            if (GameSettings.g_ClearGBuffer)
            {
                Shaders.ClearGBufferEffect.CurrentTechnique.Passes[0].Apply();
                _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
            }

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileSetupGBuffer = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Draw all our meshes to the GBuffer - albedo, normal, depth - for further computation
        /// </summary>
        /// <param name="meshMaterialLibrary"></param>
        private void DrawGBuffer(MeshMaterialLibrary meshMaterialLibrary)
        {
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            meshMaterialLibrary.Draw(renderType: MeshMaterialLibrary.RenderType.Opaque, graphicsDevice: _graphicsDevice, viewProjection: _viewProjection, lightViewPointChanged: true, view: _view);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawGBuffer = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// "Hologram" projection effect
        /// </summary>
        /// <param name="meshMat"></param>
        private void DrawHolograms(MeshMaterialLibrary meshMat)
        {
            if (!GameSettings.g_HologramDraw) return;
            _graphicsDevice.SetRenderTarget(_renderTargetHologram);
            _graphicsDevice.Clear(Color.Black);
            meshMat.Draw(MeshMaterialLibrary.RenderType.Hologram, _graphicsDevice, _viewProjection);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawHolograms = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Draw Screen Space Reflections
        /// </summary>
        /// <param name="gameTime"></param>
        private void DrawScreenSpaceReflections(GameTime gameTime)
        {
            if (!GameSettings.g_SSReflection) return;

            _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectReflection);
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            if (GameSettings.g_TemporalAntiAliasing)
            {
                Shaders.ScreenSpaceReflectionParameter_TargetMap.SetValue(_temporalAAOffFrame ? _renderTargetTAA_1 : _renderTargetTAA_2);
            }
            else
            {
                Shaders.ScreenSpaceReflectionParameter_TargetMap.SetValue(_renderTargetFinal);
            }

            if (GameSettings.g_SSReflectionNoise)
                Shaders.ScreenSpaceReflectionParameter_Time.SetValue((float)gameTime.TotalGameTime.TotalSeconds % 1000);

            Shaders.ScreenSpaceReflectionParameter_Projection.SetValue(_projection);
            
            Shaders.ScreenSpaceReflectionEffect.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawSSR = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }

        }

        /// <summary>
        /// Draw SSAO to a different rendertarget
        /// </summary>
        /// <param name="camera"></param>
        private void DrawScreenSpaceAmbientOcclusion(Camera camera)
        {
            if (!GameSettings.ssao_Active) return;

            _graphicsDevice.SetRenderTarget(_renderTargetSSAOEffect);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Shaders.ScreenSpaceEffectParameter_InverseViewProjection.SetValue(_inverseViewProjection);
            Shaders.ScreenSpaceEffectParameter_Projection.SetValue(_projection);
            Shaders.ScreenSpaceEffectParameter_ViewProjection.SetValue(_viewProjection);
            Shaders.ScreenSpaceEffectParameter_CameraPosition.SetValue(camera.Position);

            Shaders.ScreenSpaceEffect.CurrentTechnique = Shaders.ScreenSpaceEffectTechnique_SSAO;
            Shaders.ScreenSpaceEffect.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
            
            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawScreenSpaceEffect = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Screen space blur for directional lights
        /// </summary>
        /// <param name="dirLights"></param>
        private void DrawScreenSpaceDirectionalShadow(List<DirectionalLightSource> dirLights)
        {
            if (_viewProjectionHasChanged)
            {
                Shaders.deferredDirectionalLightParameterViewProjection.SetValue(_viewProjection);
                Shaders.deferredDirectionalLightParameterInverseViewProjection.SetValue(_inverseViewProjection);

            }
            foreach (DirectionalLightSource light in dirLights)
            {
                if (light.DrawShadows && light.ScreenSpaceShadowBlur)
                {
                    throw new NotImplementedException();

                    //Draw our map!
                    _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectUpsampleBlurVertical);

                    Shaders.deferredDirectionalLightParameter_LightDirection.SetValue(light.Direction);

                    if (_viewProjectionHasChanged)
                    {
                        light.DirectionViewSpace = Vector3.Transform(light.Direction, _viewIT);
                        light.LightViewProjection_ViewSpace = _inverseView* light.LightViewProjection;
                        light.LightView_ViewSpace = _inverseView * light.LightView;

                    }

                    Shaders.deferredDirectionalLightParameterLightViewProjection.SetValue(light.LightViewProjection_ViewSpace);
                    Shaders.deferredDirectionalLightParameterLightView.SetValue(light.LightViewProjection_ViewSpace);
                    Shaders.deferredDirectionalLightParameter_ShadowMap.SetValue(light.ShadowMap);
                    Shaders.deferredDirectionalLightParameter_ShadowFiltering.SetValue((int)light.ShadowFiltering);
                    Shaders.deferredDirectionalLightParameter_ShadowMapSize.SetValue((float)light.ShadowResolution);

                    Shaders.deferredDirectionalLightShadowOnly.Passes[0].Apply();

                    _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
                }
            }

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawScreenSpaceDirectionalShadow = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Bilateral blur, to upsample our undersampled SSAO
        /// </summary>
        private void DrawBilateralBlur()
        {
            _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectUpsampleBlurVertical);

            _spriteBatch.Begin(0, BlendState.Additive);

            _spriteBatch.Draw(_renderTargetSSAOEffect, new Rectangle(0, 0, (int)(GameSettings.g_ScreenWidth * GameSettings.g_supersampling), (int)(GameSettings.g_ScreenHeight * GameSettings.g_supersampling)), Color.Red);

            _spriteBatch.End();

            if (GameSettings.ssao_Blur &&  GameSettings.ssao_Active)
            {
                _graphicsDevice.RasterizerState = RasterizerState.CullNone;

                _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectUpsampleBlurHorizontal);

                Shaders.ScreenSpaceEffectParameter_InverseResolution.SetValue(new Vector2(1.0f / _renderTargetScreenSpaceEffectUpsampleBlurVertical.Width,
                    1.0f / _renderTargetScreenSpaceEffectUpsampleBlurVertical.Height) * 2);
                Shaders.ScreenSpaceEffectParameter_SSAOMap.SetValue(_renderTargetScreenSpaceEffectUpsampleBlurVertical);
                Shaders.ScreenSpaceEffectTechnique_BlurVertical.Passes[0].Apply();

                _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

                _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectBlurFinal);

                Shaders.ScreenSpaceEffectParameter_InverseResolution.SetValue(new Vector2(1.0f / _renderTargetScreenSpaceEffectUpsampleBlurHorizontal.Width,
                    1.0f / _renderTargetScreenSpaceEffectUpsampleBlurHorizontal.Height)*0.5f);
                Shaders.ScreenSpaceEffectParameter_SSAOMap.SetValue(_renderTargetScreenSpaceEffectUpsampleBlurHorizontal);
                Shaders.ScreenSpaceEffectTechnique_BlurHorizontal.Passes[0].Apply();

                _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
                
            }
            else
            {
                _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectBlurFinal);

                _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.LinearClamp);

                _spriteBatch.Draw(_renderTargetScreenSpaceEffectUpsampleBlurVertical, new Rectangle(0, 0, _renderTargetScreenSpaceEffectBlurFinal.Width, _renderTargetScreenSpaceEffectBlurFinal.Height), Color.White);

                _spriteBatch.End();
            }

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawBilateralBlur = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }
        
        /// <summary>
        /// Apply our environment cubemap to the renderer
        /// </summary>
        private void DrawEnvironmentMap()
        {
            if (!GameSettings.g_EnvironmentMapping) return;

            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            Shaders.deferredEnvironmentParameterTransposeView.SetValue(Matrix.Transpose(_view));

            //Shaders.deferredEnvironment.CurrentTechnique = GameSettings.g_SSReflection
            //    ? Shaders.deferredEnvironment.Techniques["g_SSR"]
            //    : Shaders.deferredEnvironment.Techniques["Classic"];
            Shaders.deferredEnvironment.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawEnvironmentMap = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Emissive materials have some screen space lighting properties
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="meshMatLib"></param>
        /// <param name="gameTime"></param>
        private void DrawEmissiveEffect(Camera camera, MeshMaterialLibrary meshMatLib, GameTime gameTime)
        {
            if (!GameSettings.g_EmissiveDraw) return;

            throw new NotImplementedException("Check an older build, emissives are currently not implemented because I switched from World Space to View Space but did not update all the effects yet");

            //Make a new _viewProjection
            //This should actually scale dynamically with the position of the object
            //Note: It would be better if the screen extended the same distance in each direction, right now it would probably be wider than tall
            Matrix newProjection = Matrix.CreatePerspectiveFieldOfView(Math.Min((float)Math.PI, camera.FieldOfView * GameSettings.g_EmissiveDrawFOVFactor),
                    GameSettings.g_ScreenWidth / (float)GameSettings.g_ScreenHeight, 1, GameSettings.g_FarPlane);

            Matrix transformedViewProjection = _view * newProjection;

            //meshMatLib.DrawEmissive(_graphicsDevice, camera, _viewProjection, transformedViewProjection, _inverseViewProjection, _renderTargetEmissive, _renderTargetDiffuse, _renderTargetSpecular, _lightBlendState, _assets.Sphere.Meshes, gameTime);
            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawEmissive = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Compose the render by combining the albedo channel with the light channels
        /// </summary>
        private void Compose()
        {
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            _graphicsDevice.SetRenderTargets(_renderTargetFinalBinding);
            _graphicsDevice.BlendState = BlendState.Opaque;

            //combine!
            Shaders.DeferredCompose.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileCompose = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        private void DrawBloom()
        {

            if (GameSettings.g_BloomEnable)
            {
                Texture2D bloom = _bloomFilter.Draw(_renderTargetFinal, GameSettings.g_ScreenWidth,
                    GameSettings.g_ScreenHeight);

                _graphicsDevice.SetRenderTargets(_renderTargetFinal8BitBinding);

                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

                _spriteBatch.Draw(_renderTargetFinal,
                    new Rectangle(0, 0, GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight), Color.White);
                _spriteBatch.Draw(bloom, new Rectangle(0, 0, GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight),
                    Color.White);

                _spriteBatch.End();
                
            }
            else
            {

                _graphicsDevice.SetRenderTarget(_renderTargetFinal8Bit);
                _spriteBatch.Begin(0, BlendState.Opaque, _supersampling > 1 ? SamplerState.LinearWrap : SamplerState.PointClamp);
                _spriteBatch.Draw(_renderTargetFinal, new Rectangle(0, 0, GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight), Color.White);
                _spriteBatch.End();
            }
        }

        /// <summary>
        /// Combine the render with previous frames to get more information per sample and make the image anti-aliased / super sampled
        /// </summary>
        private void CombineTemporalAntialiasing()
        {
            if (!GameSettings.g_TemporalAntiAliasing) return;

            //TEST
            //if (GameSettings.d_debugTAA)
            //{
            //    RenderTargetBinding[] testAA = new RenderTargetBinding[2];
            //    testAA[0] = new RenderTargetBinding(_temporalAAOffFrame ? _renderTargetTAA_2 : _renderTargetTAA_1);
            //    testAA[1] = new RenderTargetBinding(_renderTargetScreenSpaceEffectUpsampleBlurVertical);
            //    _graphicsDevice.SetRenderTargets(testAA);
            //}
            //else
            //{
                _graphicsDevice.SetRenderTarget(_temporalAAOffFrame ? _renderTargetTAA_2 : _renderTargetTAA_1);
            //}

            _graphicsDevice.BlendState = BlendState.Opaque;
            
            Shaders.TemporalAntiAliasingEffect_AccumulationMap.SetValue(_temporalAAOffFrame ? _renderTargetTAA_1 : _renderTargetTAA_2);
            Shaders.TemporalAntiAliasingEffect_UpdateMap.SetValue(_renderTargetFinal8Bit);
            Shaders.TemporalAntiAliasingEffect_CurrentToPrevious.SetValue(_currentViewToPreviousViewProjection);
            
            Shaders.TemporalAntiAliasingEffect.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
            
            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileCombineTemporalAntialiasing = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }
        
        private void DrawTestShadow(Camera camera)
        {
            //testShadow.Draw(_renderTargetDepth, _view, camera, _projection);
        }


        /// <summary>
        /// Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
        /// </summary>
        /// <param name="editorData"></param>
        private void RenderMode()
        {
            switch (GameSettings.g_RenderMode)
            {
               case RenderModes.Albedo:
                    DrawMapToScreenToFullScreen(_renderTargetAlbedo);
                    break;
               case RenderModes.Normal:
                    DrawMapToScreenToFullScreen(_renderTargetNormal);
                    break;
               case RenderModes.Depth:
                    DrawMapToScreenToFullScreen(_renderTargetDepth);
                    break;
               case RenderModes.Diffuse:
                    DrawMapToScreenToFullScreen(_renderTargetDiffuse);
                    break;
               case RenderModes.Specular:
                    DrawMapToScreenToFullScreen(_renderTargetSpecular);
                    break;
                case RenderModes.Volumetric:
                    DrawMapToScreenToFullScreen(_renderTargetVolume);
                    break;
                case RenderModes.SSAO:
                    DrawMapToScreenToFullScreen(_renderTargetSSAOEffect);
                    break;
                case RenderModes.Hologram:
                    DrawMapToScreenToFullScreen(_renderTargetHologram);
                    break;
                case RenderModes.DirectionalShadow:
                    DrawMapToScreenToFullScreen(_renderTargetScreenSpaceEffectBlurFinal);
                    break;
                case RenderModes.Emissive:
                    DrawMapToScreenToFullScreen(_renderTargetEmissive);
                    break;
                case RenderModes.SSR:
                    DrawMapToScreenToFullScreen(_renderTargetScreenSpaceEffectReflection);
                    break;
                case RenderModes.HDR:
                    if (GameSettings.g_TemporalAntiAliasing)
                    {
                        DrawMapToScreenToFullScreen(_temporalAAOffFrame ? _renderTargetTAA_2 : _renderTargetTAA_1);
                    }
                    else
                    {
                        DrawMapToScreenToFullScreen(_renderTargetFinal8Bit);
                    }
                    break;
                default:
                    DrawPostProcessing();
                    break;
            }


            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawFinalRender = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }
        
        /// <summary>
        /// Add some post processing to the image
        /// </summary>
        private void DrawPostProcessing()
        {
            if (!GameSettings.g_PostProcessing) return;

            RenderTarget2D baseRenderTarget;
            RenderTarget2D destinationRenderTarget;

            if (GameSettings.g_TemporalAntiAliasing)
            {
                baseRenderTarget = (_temporalAAOffFrame ? _renderTargetTAA_2 : _renderTargetTAA_1);
            }
            else
            {
                baseRenderTarget = _renderTargetFinal8Bit;
            }

            destinationRenderTarget = _renderTargetScreenSpaceEffectUpsampleBlurVertical;
            
            Shaders.PostProcessingParameter_ScreenTexture.SetValue(baseRenderTarget);
            _graphicsDevice.SetRenderTarget(destinationRenderTarget);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            
            Shaders.PostProcessing.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

            DrawMapToScreenToFullScreen(destinationRenderTarget);
        }
        #endregion

        #endregion

            #region RENDERTARGET SETUP FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            //  RENDERTARGET SETUP FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update the resolution of our rendertargets
        /// </summary>
        public void UpdateResolution()
        {
            _inverseResolution = new Vector3(1.0f / GameSettings.g_ScreenWidth, 1.0f / GameSettings.g_ScreenHeight, 0);
            _haltonSequence = null;

            SetUpRenderTargets(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight, false);

            testShadow.UpdateResolution();
        }

        private void SetUpRenderTargets(int width, int height, bool onlyEssentials)
        {
            //Discard first
            if (_renderTargetAlbedo != null)
            {
                _renderTargetAlbedo.Dispose();
                _renderTargetDepth.Dispose();
                _renderTargetNormal.Dispose();
                _renderTargetFinal.Dispose();
                _renderTargetFinal8Bit.Dispose();
                _renderTargetDiffuse.Dispose();
                _renderTargetSpecular.Dispose();
                _renderTargetVolume.Dispose();

                _renderTargetScreenSpaceEffectUpsampleBlurVertical.Dispose();

                if (!onlyEssentials)
                {
                    _renderTargetHologram.Dispose();
                    _renderTargetTAA_1.Dispose();
                    _renderTargetTAA_2.Dispose();
                    _renderTargetSSAOEffect.Dispose();
                    _renderTargetScreenSpaceEffectReflection.Dispose();

                    _renderTargetScreenSpaceEffectUpsampleBlurHorizontal.Dispose();
                    _renderTargetScreenSpaceEffectBlurFinal.Dispose();

                    _renderTargetEmissive.Dispose();
                }
            }

            float ssmultiplier = _supersampling;

            int targetWidth = (int)(width * ssmultiplier);
            int targetHeight = (int)(height * ssmultiplier);

            Shaders.BillboardEffectParameter_AspectRatio.SetValue((float)targetWidth / targetHeight);

            _renderTargetAlbedo = new RenderTarget2D(_graphicsDevice, targetWidth,
                targetHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetNormal = new RenderTarget2D(_graphicsDevice, targetWidth,
                targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            _renderTargetDepth = new RenderTarget2D(_graphicsDevice, targetWidth,
                targetHeight, false, SurfaceFormat.Single, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            
            _renderTargetBinding[0] = new RenderTargetBinding(_renderTargetAlbedo);
            _renderTargetBinding[1] = new RenderTargetBinding(_renderTargetNormal);
            _renderTargetBinding[2] = new RenderTargetBinding(_renderTargetDepth);

            _renderTargetDiffuse = new RenderTarget2D(_graphicsDevice, targetWidth,
               targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);

            _renderTargetSpecular = new RenderTarget2D(_graphicsDevice, targetWidth,
               targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            _renderTargetVolume = new RenderTarget2D(_graphicsDevice, targetWidth,
               targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            Shaders.deferredPointLightParameterResolution.SetValue(new Vector2(targetWidth, targetHeight));

            _renderTargetLightBinding[0] = new RenderTargetBinding(_renderTargetDiffuse);
            _renderTargetLightBinding[1] = new RenderTargetBinding(_renderTargetSpecular);
            _renderTargetLightBinding[2] = new RenderTargetBinding(_renderTargetVolume);

            _renderTargetFinal = new RenderTarget2D(_graphicsDevice, targetWidth,
               targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            _renderTargetFinalBinding[0] = new RenderTargetBinding(_renderTargetFinal);

            _renderTargetFinal8Bit = new RenderTarget2D(_graphicsDevice, targetWidth,
               targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            _renderTargetFinal8BitBinding[0] = new RenderTargetBinding(_renderTargetFinal8Bit);

            _renderTargetScreenSpaceEffectUpsampleBlurVertical = new RenderTarget2D(_graphicsDevice, targetWidth,
                targetHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            if (!onlyEssentials)
            {
                _editorRender.SetUpRenderTarget(width, height);

                _renderTargetTAA_1 = new RenderTarget2D(_graphicsDevice, targetWidth, targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                _renderTargetTAA_2 = new RenderTarget2D(_graphicsDevice, targetWidth, targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

                Shaders.TemporalAntiAliasingEffect_Resolution.SetValue(new Vector2(targetWidth, targetHeight));
                // Shaders.SSReflectionEffectParameter_Resolution.SetValue(new Vector2(target_width, target_height));
                Shaders.EmissiveEffectParameter_Resolution.SetValue(new Vector2(targetWidth, targetHeight));

                _renderTargetEmissive = new RenderTarget2D(_graphicsDevice, targetWidth,
                    targetHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

                Shaders.ScreenSpaceReflectionParameter_Resolution.SetValue(new Vector2(targetWidth, targetHeight));
                Shaders.deferredEnvironmentParameter_Resolution.SetValue(new Vector2(targetWidth, targetHeight));
                _renderTargetScreenSpaceEffectReflection = new RenderTarget2D(_graphicsDevice, targetWidth,
                    targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);


                ///////////////////
                // HALF RESOLUTION

                targetWidth /= 2;
                targetHeight /= 2;

                _renderTargetScreenSpaceEffectUpsampleBlurHorizontal = new RenderTarget2D(_graphicsDevice, targetWidth,
                    targetHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

                _renderTargetScreenSpaceEffectBlurFinal = new RenderTarget2D(_graphicsDevice, targetWidth,
                    targetHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

                _renderTargetSSAOEffect = new RenderTarget2D(_graphicsDevice, targetWidth,
                    targetHeight, false, SurfaceFormat.HalfSingle, DepthFormat.None, 0,
                    RenderTargetUsage.DiscardContents);



                Shaders.ScreenSpaceEffectParameter_InverseResolution.SetValue(new Vector2(1.0f / targetWidth,
                    1.0f / targetHeight));

                _renderTargetHologram = new RenderTarget2D(_graphicsDevice, targetWidth,
                    targetHeight, false, SurfaceFormat.Single, DepthFormat.Depth24, 0,
                    RenderTargetUsage.PreserveContents);
            }

            UpdateRenderMapBindings(onlyEssentials);
        }

        private void UpdateRenderMapBindings(bool onlyEssentials)
        {
            Shaders.BillboardEffectParameter_DepthMap.SetValue(_renderTargetDepth);

            Shaders.ReconstructDepthParameter_DepthMap.SetValue(_renderTargetDepth);

            Shaders.deferredPointLightParameter_AlbedoMap.SetValue(_renderTargetAlbedo);
            Shaders.deferredPointLightParameter_DepthMap.SetValue(_renderTargetDepth);
            Shaders.deferredPointLightParameter_NormalMap.SetValue(_renderTargetNormal);

            Shaders.deferredDirectionalLightParameter_AlbedoMap.SetValue(_renderTargetAlbedo);
            Shaders.deferredDirectionalLightParameter_DepthMap.SetValue(_renderTargetDepth);
            Shaders.deferredDirectionalLightParameter_NormalMap.SetValue(_renderTargetNormal);

            Shaders.deferredDirectionalLightParameter_SSShadowMap.SetValue(onlyEssentials ? _renderTargetScreenSpaceEffectUpsampleBlurVertical : _renderTargetScreenSpaceEffectBlurFinal);

            Shaders.deferredEnvironmentParameter_AlbedoMap.SetValue(_renderTargetAlbedo);
            //Shaders.deferredEnvironmentParameter_DepthMap.SetValue(_renderTargetDepth);
            Shaders.deferredEnvironmentParameter_NormalMap.SetValue(_renderTargetNormal);
            Shaders.deferredEnvironmentParameter_SSRMap.SetValue(_renderTargetScreenSpaceEffectReflection);

            Shaders.DeferredComposeEffectParameter_ColorMap.SetValue(_renderTargetAlbedo);
            Shaders.DeferredComposeEffectParameter_diffuseLightMap.SetValue(_renderTargetDiffuse);
            Shaders.DeferredComposeEffectParameter_specularLightMap.SetValue(_renderTargetSpecular);
            Shaders.DeferredComposeEffectParameter_volumeLightMap.SetValue(_renderTargetVolume);
            Shaders.DeferredComposeEffectParameter_SSAOMap.SetValue(_renderTargetScreenSpaceEffectBlurFinal);
            Shaders.DeferredComposeEffectParameter_HologramMap.SetValue(_renderTargetHologram);
            // Shaders.DeferredComposeEffectParameter_SSRMap.SetValue(_renderTargetScreenSpaceEffectReflection);

            Shaders.ScreenSpaceEffectParameter_NormalMap.SetValue(_renderTargetNormal);

            Shaders.ScreenSpaceEffectParameter_DepthMap.SetValue(_renderTargetDepth);
            Shaders.ScreenSpaceEffectParameter_SSAOMap.SetValue(_renderTargetSSAOEffect);

            Shaders.ScreenSpaceReflectionParameter_DepthMap.SetValue(_renderTargetDepth);
            Shaders.ScreenSpaceReflectionParameter_NormalMap.SetValue(_renderTargetNormal);
            //Shaders.ScreenSpaceReflectionParameter_TargetMap.SetValue(_renderTargetFinal);

            Shaders.EmissiveEffectParameter_DepthMap.SetValue(_renderTargetDepth);
            Shaders.EmissiveEffectParameter_EmissiveMap.SetValue(_renderTargetEmissive);
            Shaders.EmissiveEffectParameter_NormalMap.SetValue(_renderTargetNormal);

            Shaders.TemporalAntiAliasingEffect_DepthMap.SetValue(_renderTargetDepth);

            Shaders.TestShadowEffect_DepthMap.SetValue(_renderTargetDepth);
            Shaders.TestShadowEffect_Resolution.SetValue(new Vector2(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight));
        }

            #endregion

            #region HELPER FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            //  HELPER FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void DrawMapToScreenToCube(RenderTarget2D map, RenderTargetCube target, CubeMapFace? face)
        {

            if (face != null) _graphicsDevice.SetRenderTarget(target, (CubeMapFace)face);
           // _graphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp);
            _spriteBatch.Draw(map, new Rectangle(0, 0, map.Width, map.Height), Color.White);
            _spriteBatch.End();
        }

        private void DrawMapToScreenToFullScreen(Texture2D map, BlendState blendState = null)
        {
            if(blendState == null) blendState = BlendState.Opaque;

            int height;
            int width;
            if (Math.Abs(map.Width / (float)map.Height - GameSettings.g_ScreenWidth / (float)GameSettings.g_ScreenHeight) < 0.001)
            //If same aspectratio
            {
                height = GameSettings.g_ScreenHeight;
                width = GameSettings.g_ScreenWidth;
            }
            else
            {
                if (GameSettings.g_ScreenHeight < GameSettings.g_ScreenWidth)
                {
                    height = GameSettings.g_ScreenHeight;
                    width = GameSettings.g_ScreenHeight;
                }
                else
                {
                    height = GameSettings.g_ScreenWidth;
                    width = GameSettings.g_ScreenWidth;
                }
            }
            _graphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin(0, blendState, _supersampling>1 ? SamplerState.LinearWrap : SamplerState.PointClamp);
            _spriteBatch.Draw(map, new Rectangle(0, 0, width, height), Color.White);
            _spriteBatch.End();
        }
        
        private void CpuRayMarch(Camera camera)
        {
            if(GameSettings.e_CPURayMarch)

            if (Input.WasKeyPressed(Keys.K))
                _cpuRayMarch.Calculate(_renderTargetDepth, _renderTargetNormal, _projection, _inverseView, _inverseViewProjection,
                    camera, _currentFrustumCorners);

            _cpuRayMarch.Draw();
        }
        
        #endregion

        #endregion
    }

}

