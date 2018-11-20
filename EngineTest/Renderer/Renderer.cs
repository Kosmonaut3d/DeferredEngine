using System;
using System.Collections.Generic;
using System.Diagnostics;
using DeferredEngine.Entities;
using DeferredEngine.Logic;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.Helper.HelperGeometry;
using DeferredEngine.Renderer.RenderModules;
using DeferredEngine.Renderer.RenderModules.Default;
using DeferredEngine.Renderer.RenderModules.PostProcessingFilters;
using DeferredEngine.Renderer.RenderModules.Signed_Distance_Fields;
using DeferredEngine.Renderer.RenderModules.Signed_Distance_Fields.SDF_Generator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DirectionalLight = DeferredEngine.Entities.DirectionalLight;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Renderer
{
    public class Renderer : IDisposable
    {
        #region VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Graphics & Helpers
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        //private QuadRenderer _quadRenderer;
        private FullScreenTriangle _fullScreenTriangle;
        private GaussianBlur _gaussianBlur;
        private EditorRender _editorRender;
        private CPURayMarch _cpuRayMarch;
        private LightAccumulationModule _lightAccumulationModule;

        private ShadowMapRenderModule _shadowMapRenderModule;
        private GBufferRenderModule _gBufferRenderModule;
        private TemporalAntialiasingRenderModule _temporalAntialiasingRenderModule;
        private DeferredEnvironmentMapRenderModule _deferredEnvironmentMapRenderModule;
        private DecalRenderModule _decalRenderModule;
        private SubsurfaceScatterRenderModule _subsurfaceScatterRenderModule;
        private ForwardRenderModule _forwardRenderModule;
        private HelperGeometryRenderModule _helperGeometryRenderModule;
        private DistanceFieldRenderModule _distanceFieldRenderModule;

        private BloomFilter _bloomFilter;
        private ColorGradingFilter _colorGradingFilter;

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

        //Used for the view space directions in our shaders. Far edges of our view frustum
        private readonly Vector3[] _cornersWorldSpace = new Vector3[8];
        private readonly Vector3[] _cornersViewSpace = new Vector3[8];
        private readonly Vector3[] _currentFrustumCorners = new Vector3[4];

        //Checkvariables to see which console variables have changed from the frame before
        private float _g_FarClip;
        private float _supersampling = 1;
        //private bool _hologramDraw;
        private int _forceShadowFiltering;
        private bool _forceShadowSS;
        private bool _ssr = true;
        private bool _g_SSReflectionNoise;

        //SDF
        private List<SignedDistanceField> _sdfDefinitions;

        //Render modes
        public enum RenderModes {
            Deferred,
            Albedo,
            Normal,
            Depth,
            Diffuse,
            Specular,
            Volumetric,
            //Hologram,
            SSAO,
            SSBlur,
            //Emissive,
            SSR,
            HDR
        }

        //Render targets
        private RenderTarget2D _renderTargetAlbedo;
        private readonly RenderTargetBinding[] _renderTargetBinding = new RenderTargetBinding[3];

        private RenderTarget2D _renderTargetDepth;
        private RenderTarget2D _renderTargetNormal;

        private RenderTarget2D _renderTargetDecalOffTarget;

        //Subsurface Scattering
        private RenderTarget2D _renderTargetSSS;

        private RenderTarget2D _renderTargetDiffuse;
        private RenderTarget2D _renderTargetSpecular;
        private RenderTarget2D _renderTargetVolume;
        private readonly RenderTargetBinding[] _renderTargetLightBinding = new RenderTargetBinding[3];

        private RenderTarget2D _renderTargetComposed;
        private RenderTarget2D _renderTargetBloom;

        //TAA
        private RenderTarget2D _renderTargetTAA_1;
        private RenderTarget2D _renderTargetTAA_2;
        
        private RenderTarget2D _renderTargetScreenSpaceEffectReflection;

        //private RenderTarget2D _renderTargetHologram;

        private RenderTarget2D _renderTargetSSAOEffect;

        private RenderTarget2D _renderTargetScreenSpaceEffectUpsampleBlurVertical;

        private RenderTarget2D _renderTargetScreenSpaceEffectUpsampleBlurHorizontal;
        private RenderTarget2D _renderTargetScreenSpaceEffectBlurFinal;

        //private RenderTarget2D _renderTargetEmissive;

        private RenderTarget2D _renderTargetOutput;

        private RenderTarget2D _currentOutput;
        
        //Cubemap
        private RenderTargetCube _renderTargetCubeMap;

        //Color Correction LUT

        private Texture2D _lut;
        

        //Performance Profiler

        private readonly Stopwatch _performanceTimer = new Stopwatch();
        private long _performancePreviousTime;

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
        public void Load(ContentManager content, ShaderManager shaderManager)
        {
            _bloomFilter = new BloomFilter();
            _bloomFilter.Load(content);


            _lightAccumulationModule = new LightAccumulationModule(shaderManager, "Shaders/Deferred/DeferredPointLight");
            _shadowMapRenderModule = new ShadowMapRenderModule(content, "Shaders/Shadow/ShadowMap");
            _gBufferRenderModule = new GBufferRenderModule(content, "Shaders/GbufferSetup/ClearGBuffer", "Shaders/GbufferSetup/Gbuffer");
            _temporalAntialiasingRenderModule = new TemporalAntialiasingRenderModule(content, "Shaders/TemporalAntiAliasing/TemporalAntiAliasing");
            _deferredEnvironmentMapRenderModule = new DeferredEnvironmentMapRenderModule(content, "Shaders/Deferred/DeferredEnvironmentMap");
            _decalRenderModule = new DecalRenderModule(shaderManager, "Shaders/Deferred/DeferredDecal");
            _subsurfaceScatterRenderModule = new SubsurfaceScatterRenderModule(content, "Shaders/SubsurfaceScattering/SubsurfaceScattering");
            _forwardRenderModule = new ForwardRenderModule(content, "Shaders/forward/forward");
            _helperGeometryRenderModule = new HelperGeometryRenderModule(content, "Shaders/Editor/LineEffect");
            _distanceFieldRenderModule = new DistanceFieldRenderModule(shaderManager, "Shaders/SignedDistanceFields/volumeProjection");

            _inverseResolution = new Vector3(1.0f / GameSettings.g_screenwidth, 1.0f / GameSettings.g_screenheight, 0);
            
            _colorGradingFilter = new ColorGradingFilter(content, "Shaders/PostProcessing/ColorGrading");

            _lut = content.Load<Texture2D>("Shaders/PostProcessing/lut");
        }

        /// <summary>
        /// Initialize all our rendermodules and helpers. Done after the Load() function
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="assets"></param>
        public void Initialize(GraphicsDevice graphicsDevice, Assets assets)
        {
            _graphicsDevice = graphicsDevice;
            //_quadRenderer = new QuadRenderer();
            _fullScreenTriangle = new FullScreenTriangle(graphicsDevice);
            _spriteBatch = new SpriteBatch(graphicsDevice);
            _gaussianBlur = new GaussianBlur();
            _gaussianBlur.Initialize(graphicsDevice);

            _editorRender = new EditorRender();
            _editorRender.Initialize(graphicsDevice, assets);

            _cpuRayMarch = new CPURayMarch();
            _cpuRayMarch.Initialize(_graphicsDevice);

            _bloomFilter.Initialize(_graphicsDevice, GameSettings.g_screenwidth, GameSettings.g_screenheight, _fullScreenTriangle);

            _colorGradingFilter.Initialize(graphicsDevice);

            _lightAccumulationModule.Initialize(graphicsDevice, _fullScreenTriangle, assets);

            _gBufferRenderModule.Initialize(_graphicsDevice);

            _decalRenderModule.Initialize(graphicsDevice);

            _subsurfaceScatterRenderModule.Initialize();

            _forwardRenderModule.Initialize();

            _helperGeometryRenderModule.Initialize();
            
            _assets = assets;
            //Apply some base settings to overwrite shader defaults with game settings defaults
            GameSettings.ApplySettings();

            Shaders.ScreenSpaceReflectionParameter_NoiseMap.SetValue(_assets.NoiseMap);
            SetUpRenderTargets(GameSettings.g_screenwidth, GameSettings.g_screenheight, false);
            
        }

        /// <summary>
        /// Update our function
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="isActive"></param>
        public void Update(GameTime gameTime, bool isActive, SdfGenerator sdfGenerator, List<BasicEntity> entities)
        {
            if (!isActive) return;
            _editorRender.Update(gameTime);

            //SDF Updating
            sdfGenerator.Update(entities, _graphicsDevice, _distanceFieldRenderModule, _fullScreenTriangle, ref _sdfDefinitions);

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
        public EditorLogic.EditorReceivedData Draw(Camera camera, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities, List<Decal> decals, List<PointLight> pointLights, List<DirectionalLight> directionalLights, EnvironmentSample envSample, List<DebugEntity> debugEntities, EditorLogic.EditorSendData editorData, GameTime gameTime)
        {
            //Reset the stat counter, so we can count stats/information for this frame only
            ResetStats();

            if (GameSettings.d_drawnothing)
            {
                _graphicsDevice.Clear(Color.Black);
                return new EditorLogic.EditorReceivedData
                {
                    HoveredId = 0,
                    ViewMatrix = _view,
                    ProjectionMatrix = _projection
                };
            } 

            //Update the mesh data for changes in physics etc.
            meshMaterialLibrary.FlagMovedObjects(entities);

            //Check if we changed some drastic stuff for which we need to reload some elements
            CheckRenderChanges(directionalLights);

            //Render ShadowMaps
            DrawShadowMaps(meshMaterialLibrary, entities, pointLights, directionalLights, camera);

            //Update SDFs
            if (IsSDFUsed(pointLights))
            {
                _distanceFieldRenderModule.UpdateDistanceFieldTransformations(entities, _sdfDefinitions, _deferredEnvironmentMapRenderModule, _graphicsDevice, _spriteBatch, _lightAccumulationModule);
            }
            //Render EnvironmentMaps
            //We do this either when pressing C or at the start of the program (_renderTargetCube == null) or when the game settings want us to do it every frame
            if (envSample.NeedsUpdate || GameSettings.g_envmapupdateeveryframe)
            {
                DrawCubeMap(envSample.Position, meshMaterialLibrary, entities, pointLights, directionalLights, envSample, 300, gameTime, camera);
                envSample.NeedsUpdate = false;
            }

            //Update our view projection matrices if the camera moved
            UpdateViewProjection(camera, meshMaterialLibrary, entities);
            
            //Draw our meshes to the G Buffer
            DrawGBuffer(meshMaterialLibrary);

            //Deferred Decals
            DrawDecals(decals);
            
            //Draw Screen Space reflections to a different render target
            DrawScreenSpaceReflections(gameTime);

            //SSAO
            DrawScreenSpaceAmbientOcclusion(camera);

            //Screen space shadows for directional lights to an offscreen render target
            DrawScreenSpaceDirectionalShadow(directionalLights);

            //Upsample/blur our SSAO / screen space shadows
            DrawBilateralBlur();

            //Light the scene
            _lightAccumulationModule.DrawLights(pointLights, directionalLights, camera.Position, gameTime, _renderTargetLightBinding, _renderTargetDiffuse);

            //Draw the environment cube map as a fullscreen effect on all meshes
            DrawEnvironmentMap(envSample, camera, gameTime);

            //Draw emissive materials on an offscreen render target
            //DrawEmissiveEffect(camera, meshMaterialLibrary, gameTime);

            //Compose the scene by combining our lighting data with the gbuffer data
            _currentOutput = Compose(); //-> output _renderTargetComposed

            ///*_currentOutput =*/ DrawSubsurfaceScattering(_renderTargetSSS, meshMaterialLibrary);

            //Forward
            _currentOutput = DrawForward(_currentOutput, meshMaterialLibrary, camera, pointLights);
            
            //Compose the image and add information from previous frames to apply temporal super sampling
            _currentOutput = TonemapAndCombineTemporalAntialiasing(_currentOutput); // -> output: _temporalAAOffFrame ? _renderTargetTAA_2 : _renderTargetTAA_1

            //Do Bloom
            _currentOutput = DrawBloom(_currentOutput); // -> output: _renderTargetBloom
            
            //Draw the elements that we are hovering over with outlines
            if(GameSettings.e_enableeditor && GameStats.e_EnableSelection)
                _editorRender.DrawIds(meshMaterialLibrary, decals, pointLights, directionalLights, envSample, debugEntities, _staticViewProjection, _view, editorData);

            //Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
            RenderMode(_currentOutput);

            //Draw signed distance field functions
            DrawSignedDistanceFieldFunctions(camera);
            
            //Additional editor elements that overlay our screen
            
            RenderEditorOverlays(editorData, meshMaterialLibrary, decals, pointLights, directionalLights, envSample, debugEntities);
            

            //Debug ray marching
                CpuRayMarch(camera);

            //Draw debug geometry
            _helperGeometryRenderModule.Draw(_graphicsDevice, _staticViewProjection);
            
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

        private bool IsSDFUsed(List<PointLight> pointLights)
        {
            for (var index = 0; index < pointLights.Count; index++)
            {
                if (pointLights[index].CastSDFShadows)
                {
                    return true;
                }
            }
            return false;
        }

        private void RenderEditorOverlays(EditorLogic.EditorSendData editorData, MeshMaterialLibrary meshMaterialLibrary, List<Decal> decals, List<PointLight> pointLights, List<DirectionalLight> directionalLights, EnvironmentSample envSample, List<DebugEntity> debugEntities)
        {

            if (GameSettings.e_enableeditor && GameStats.e_EnableSelection)
            {
                if (GameSettings.e_drawoutlines)
                    DrawMapToScreenToFullScreen(_editorRender.GetOutlines(), BlendState.Additive);

                _editorRender.DrawEditorElements(meshMaterialLibrary, decals, pointLights, directionalLights, envSample,
                    debugEntities,
                    _staticViewProjection, _view, editorData);


                if (editorData.SelectedObject != null)
                {
                    if (editorData.SelectedObject is Decal)
                    {
                        _decalRenderModule.DrawOutlines(_graphicsDevice, editorData.SelectedObject as Decal,
                            _staticViewProjection, _view);
                    }

                    if (GameSettings.e_drawboundingbox)
                        if (editorData.SelectedObject is BasicEntity)
                        {
                            HelperGeometryManager.GetInstance()
                                .AddBoundingBox(editorData.SelectedObject as BasicEntity);
                        }
                }
            }

            if(GameSettings.sdf_debug && _distanceFieldRenderModule.GetAtlas()!= null)
            {
                _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp);
                _spriteBatch.Draw(_distanceFieldRenderModule.GetAtlas() , new Rectangle(0, GameSettings.g_screenheight - 200, GameSettings.g_screenwidth, 200), Color.White);
                _spriteBatch.End();
            }

            // //Show shadow maps
            //if (editorData.SelectedObject != null)
            //{
            //    if (editorData.SelectedObject is PointLight)
            //    {
            //        int size = 128;
            //        PointLight light = pointLights[2]; /*(PointLightSource)editorData.SelectedObject*/
            //        ;
            //        if (light.CastShadows)
            //        {
            //            _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp);
            //            _spriteBatch.Draw(light.ShadowMap,
            //                new Rectangle(0, GameSettings.g_screenheight - size*6, size, size*6), Color.White);
            //            _spriteBatch.End();
            //        }
            //    }

            //}
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
        private void DrawCubeMap(Vector3 origin, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities, List<PointLight> pointLights, List<DirectionalLight> dirLights, EnvironmentSample envSample, float farPlane, GameTime gameTime, Camera camera)
        {
            //If our cubemap is not yet initialized, create a new one
            if (_renderTargetCubeMap == null)
            {
                //Create a new cube map
                _renderTargetCubeMap = new RenderTargetCube(_graphicsDevice, GameSettings.g_envmapresolution, true, SurfaceFormat.HalfVector4,
                    DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

                //Set this cubemap in the shader of the environment map
                _deferredEnvironmentMapRenderModule.Cubemap = _renderTargetCubeMap;
            }

            //Set up all the base rendertargets with the resolution of our cubemap
            SetUpRenderTargets(GameSettings.g_envmapresolution, GameSettings.g_envmapresolution, true);

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
                _lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_InverseView.SetValue(_inverseView);

                //yep we changed
                _viewProjectionHasChanged = true;

                if (_boundingFrustum == null) _boundingFrustum = new BoundingFrustum(_viewProjection);
                else _boundingFrustum.Matrix = _viewProjection;
                ComputeFrustumCorners(_boundingFrustum, camera);

                _lightAccumulationModule.UpdateViewProjection(_boundingFrustum, _viewProjectionHasChanged, _view, _inverseView, _viewIT, _projection, _viewProjection, _inverseViewProjection);
                
                //Base stuff, for description look in Draw()
                meshMaterialLibrary.FrustumCulling(entities, _boundingFrustum, true, origin);

                DrawGBuffer(meshMaterialLibrary);

                bool volumeEnabled = GameSettings.g_VolumetricLights;
                GameSettings.g_VolumetricLights = false;
                _lightAccumulationModule.DrawLights(pointLights, dirLights, origin, gameTime, _renderTargetLightBinding, _renderTargetDiffuse);

                _deferredEnvironmentMapRenderModule.DrawSky(_graphicsDevice, _fullScreenTriangle);

                GameSettings.g_VolumetricLights = volumeEnabled;

                //We don't use temporal AA obviously for the cubemap
                bool tempAa = GameSettings.g_taa;
                GameSettings.g_taa = false;
                
                //Shaders.DeferredCompose.CurrentTechnique = Shaders.DeferredComposeTechnique_NonLinear;
                Compose();
                //Shaders.DeferredCompose.CurrentTechnique = GameSettings.g_SSReflection
                //    ? Shaders.DeferredComposeTechnique_Linear
                //    : Shaders.DeferredComposeTechnique_NonLinear;
                GameSettings.g_taa = tempAa;
                DrawMapToScreenToCube(_renderTargetComposed, _renderTargetCubeMap, cubeMapFace);
            }
            Shaders.DeferredComposeEffectParameter_UseSSAO.SetValue(GameSettings.g_ssao_draw);

            //Change RTs back to normal
            SetUpRenderTargets(GameSettings.g_screenwidth, GameSettings.g_screenheight, true);
            
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
        private void CheckRenderChanges(List<DirectionalLight> dirLights)
        {
            if (Math.Abs(_g_FarClip - GameSettings.g_farplane) > 0.0001f)
            {
                _g_FarClip = GameSettings.g_farplane;
                _gBufferRenderModule.FarClip = _g_FarClip;
                _decalRenderModule.FarClip = _g_FarClip;
                _lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_FarClip.SetValue(_g_FarClip);
                Shaders.BillboardEffectParameter_FarClip.SetValue(_g_FarClip);
                Shaders.ScreenSpaceReflectionParameter_FarClip.SetValue(_g_FarClip);
                Shaders.ReconstructDepthParameter_FarClip.SetValue(_g_FarClip);
            }

            if (_g_SSReflectionNoise != GameSettings.g_SSReflectionNoise)
            {
                _g_SSReflectionNoise = GameSettings.g_SSReflectionNoise;
                if (!_g_SSReflectionNoise) Shaders.ScreenSpaceReflectionParameter_Time.SetValue(0.0f);
            }
            
            //if (_hologramDraw != GameSettings.g_HologramDraw)
            //{
            //    _hologramDraw = GameSettings.g_HologramDraw;

            //    if (!_hologramDraw)
            //    {
            //        _graphicsDevice.SetRenderTarget(_renderTargetHologram);
            //        _graphicsDevice.Clear(Color.Black);
            //    }
            //}

            if (_forceShadowFiltering != GameSettings.g_shadowforcefiltering)
            {
                _forceShadowFiltering = GameSettings.g_shadowforcefiltering;

                for (var index = 0; index < dirLights.Count; index++)
                {
                    DirectionalLight light = dirLights[index];
                    if (light.ShadowMap != null) light.ShadowMap.Dispose();
                    light.ShadowMap = null;

                    light.ShadowFiltering = (DirectionalLight.ShadowFilteringTypes) (_forceShadowFiltering - 1);

                    light.HasChanged = true;
                }
            }

            if (_forceShadowSS != GameSettings.g_shadowforcescreenspace)
            {
                _forceShadowSS = GameSettings.g_shadowforcescreenspace;

                for (var index = 0; index < dirLights.Count; index++)
                {
                    DirectionalLight light = dirLights[index];
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
        private void DrawShadowMaps(MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities, List<PointLight> pointLights, List<DirectionalLight> dirLights, Camera camera)
        {
            //Don't render for the first frame, we need a guideline first
            if (_boundingFrustum == null) UpdateViewProjection(camera, meshMaterialLibrary, entities);

            _shadowMapRenderModule.Draw(_graphicsDevice, meshMaterialLibrary, entities, pointLights, dirLights, camera);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawShadows = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
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
            if (GameSettings.g_taa)
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

                _lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_InverseView.SetValue(_inverseView);

                _projection = Matrix.CreatePerspectiveFieldOfView(camera.FieldOfView,
                    GameSettings.g_screenwidth / (float)GameSettings.g_screenheight, 1, GameSettings.g_farplane);
                
                _gBufferRenderModule.Camera = camera.Position;

                _viewProjection = _view * _projection;

                //this is the unjittered viewProjection. For some effects we don't want the jittered one
                _staticViewProjection = _viewProjection;

                //Transformation for TAA - from current view back to the old view projection
                _currentViewToPreviousViewProjection = Matrix.Invert(_view) * _previousViewProjection;
                
                //Temporal AA
                if (GameSettings.g_taa)
                {
                    switch (GameSettings.g_taa_jittermode)
                    {
                        case 0: //2 frames, just basic translation. Worst taa implementation. Not good with the continous integration used
                        {
                            float translation = _temporalAAOffFrame ? 0.5f : -0.5f;
                            _viewProjection = _viewProjection *
                                              Matrix.CreateTranslation(new Vector3(translation / GameSettings.g_screenwidth,
                                                  translation / GameSettings.g_screenheight, 0));
                        }
                            break;
                        case 1: // Just random translation
                        {
                            float randomAngle = FastRand.NextAngle();
                            Vector3 translation = new Vector3((float)Math.Sin(randomAngle) / GameSettings.g_screenwidth, (float)Math.Cos(randomAngle) / GameSettings.g_screenheight, 0) * 0.5f;
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
                ComputeFrustumCorners(_boundingFrustum, camera);
            }

            //We need to update whether or not entities are in our boundingFrustum and then cull them or not!
            meshMaterialLibrary.FrustumCulling(entities, _boundingFrustum, _viewProjectionHasChanged, camera.Position);

            _lightAccumulationModule.UpdateViewProjection(_boundingFrustum, _viewProjectionHasChanged, _view, _inverseView, _viewIT, _projection, _viewProjection, _inverseViewProjection);

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
        private void ComputeFrustumCorners(BoundingFrustum cameraFrustum, Camera camera)
        {
            cameraFrustum.GetCorners(_cornersWorldSpace);

            /*this part is used for volume projection*/
            //World Space Corners - Camera Position
            for (int i = 0; i < 4; i++) //take only the 4 farthest points
            {
                _currentFrustumCorners[i] = _cornersWorldSpace[i + 4] - camera.Position;
            }
            Vector3 temp = _currentFrustumCorners[3];
            _currentFrustumCorners[3] = _currentFrustumCorners[2];
            _currentFrustumCorners[2] = temp;

            _distanceFieldRenderModule.FrustumCornersWorldSpace = _currentFrustumCorners;
            _deferredEnvironmentMapRenderModule.FrustumCornersWS = _currentFrustumCorners;

            //View Space Corners
            //this is the inverse of our camera transform
            Vector3.Transform(_cornersWorldSpace, ref _view, _cornersViewSpace); //put the frustum into view space
            for (int i = 0; i < 4; i++) //take only the 4 farthest points
            {
                _currentFrustumCorners[i] = _cornersViewSpace[i + 4];
            }
            temp = _currentFrustumCorners[3];
            _currentFrustumCorners[3] = _currentFrustumCorners[2];
            _currentFrustumCorners[2] = temp;

            Shaders.ScreenSpaceReflectionParameter_FrustumCorners.SetValue(_currentFrustumCorners);
            Shaders.ScreenSpaceEffectParameter_FrustumCorners.SetValue(_currentFrustumCorners);
            _temporalAntialiasingRenderModule.FrustumCorners = _currentFrustumCorners;
            Shaders.ReconstructDepthParameter_FrustumCorners.SetValue(_currentFrustumCorners);
            Shaders.deferredDirectionalLightParameterFrustumCorners.SetValue(_currentFrustumCorners);
            _subsurfaceScatterRenderModule.FrustumCorners = _currentFrustumCorners;
        }
        
        /// <summary>
        /// Draw all our meshes to the GBuffer - albedo, normal, depth - for further computation
        /// </summary>
        /// <param name="meshMaterialLibrary"></param>
        private void DrawGBuffer(MeshMaterialLibrary meshMaterialLibrary)
        {
            _gBufferRenderModule.Draw(_graphicsDevice, _renderTargetBinding, meshMaterialLibrary, _viewProjection, _view);
            
            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawGBuffer = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Draw deferred Decals
        /// </summary>
        /// <param name="decals"></param>
        private void DrawDecals(List<Decal> decals)
        {
            if (!GameSettings.g_drawdecals) return;
            
            //First copy albedo to decal offtarget
            DrawMapToScreenToFullScreen(_renderTargetAlbedo, BlendState.Opaque, _renderTargetDecalOffTarget);

            DrawMapToScreenToFullScreen(_renderTargetDecalOffTarget, BlendState.Opaque, _renderTargetAlbedo);
            
            _decalRenderModule.Draw(_graphicsDevice, decals, _view, _viewProjection, _inverseView);
        }
        
        /// <summary>
        /// Draw Screen Space Reflections
        /// </summary>
        /// <param name="gameTime"></param>
        private void DrawScreenSpaceReflections(GameTime gameTime)
        {
            if (!GameSettings.g_SSReflection) return;


            //todo: more samples for more reflective materials!
            _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectReflection);
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            if (GameSettings.g_taa)
            {
                    Shaders.ScreenSpaceReflectionParameter_TargetMap.SetValue(_temporalAAOffFrame ? _renderTargetTAA_1 : _renderTargetTAA_2);
            }
            else
            {
            Shaders.ScreenSpaceReflectionParameter_TargetMap.SetValue(_renderTargetComposed);
            }

            if (GameSettings.g_SSReflectionNoise)
                Shaders.ScreenSpaceReflectionParameter_Time.SetValue((float)gameTime.TotalGameTime.TotalSeconds % 1000);

            Shaders.ScreenSpaceReflectionParameter_Projection.SetValue(_projection);
            
            Shaders.ScreenSpaceReflectionEffect.CurrentTechnique.Passes[0].Apply();
            _fullScreenTriangle.Draw(_graphicsDevice);

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
            if (!GameSettings.g_ssao_draw) return;

            _graphicsDevice.SetRenderTarget(_renderTargetSSAOEffect);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Shaders.ScreenSpaceEffectParameter_InverseViewProjection.SetValue(_inverseViewProjection);
            Shaders.ScreenSpaceEffectParameter_Projection.SetValue(_projection);
            Shaders.ScreenSpaceEffectParameter_ViewProjection.SetValue(_viewProjection);
            Shaders.ScreenSpaceEffectParameter_CameraPosition.SetValue(camera.Position);

            Shaders.ScreenSpaceEffect.CurrentTechnique = Shaders.ScreenSpaceEffectTechnique_SSAO;
            Shaders.ScreenSpaceEffect.CurrentTechnique.Passes[0].Apply();
            _fullScreenTriangle.Draw(_graphicsDevice);
            
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
        private void DrawScreenSpaceDirectionalShadow(List<DirectionalLight> dirLights)
        {
            if (_viewProjectionHasChanged)
            {
                Shaders.deferredDirectionalLightParameterViewProjection.SetValue(_viewProjection);
                Shaders.deferredDirectionalLightParameterInverseViewProjection.SetValue(_inverseViewProjection);

            }
            for (var index = 0; index < dirLights.Count; index++)
            {
                DirectionalLight light = dirLights[index];
                if (light.CastShadows && light.ScreenSpaceShadowBlur)
                {
                    throw new NotImplementedException();

                    /*
                    //Draw our map!
                    _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectUpsampleBlurVertical);

                    Shaders.deferredDirectionalLightParameter_LightDirection.SetValue(light.Direction);

                    if (_viewProjectionHasChanged)
                    {
                        light.DirectionViewSpace = Vector3.Transform(light.Direction, _viewIT);
                        light.LightViewProjection_ViewSpace = _inverseView * light.LightViewProjection;
                        light.LightView_ViewSpace = _inverseView * light.LightView;
                    }

                    Shaders.deferredDirectionalLightParameterLightViewProjection.SetValue(light
                        .LightViewProjection_ViewSpace);
                    Shaders.deferredDirectionalLightParameterLightView.SetValue(light.LightViewProjection_ViewSpace);
                    Shaders.deferredDirectionalLightParameter_ShadowMap.SetValue(light.ShadowMap);
                    Shaders.deferredDirectionalLightParameter_ShadowFiltering.SetValue((int) light.ShadowFiltering);
                    Shaders.deferredDirectionalLightParameter_ShadowMapSize.SetValue((float) light.ShadowResolution);

                    Shaders.deferredDirectionalLightShadowOnly.Passes[0].Apply();

                    _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
                    */
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

            _spriteBatch.Draw(_renderTargetSSAOEffect, new Rectangle(0, 0, GameSettings.g_screenwidth, GameSettings.g_screenheight), Color.Red);

            _spriteBatch.End();

            if (GameSettings.g_ssao_blur &&  GameSettings.g_ssao_draw)
            {
                _graphicsDevice.RasterizerState = RasterizerState.CullNone;

                _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectUpsampleBlurHorizontal);

                Shaders.ScreenSpaceEffectParameter_InverseResolution.SetValue(new Vector2(1.0f / _renderTargetScreenSpaceEffectUpsampleBlurVertical.Width,
                    1.0f / _renderTargetScreenSpaceEffectUpsampleBlurVertical.Height) * 2);
                Shaders.ScreenSpaceEffectParameter_SSAOMap.SetValue(_renderTargetScreenSpaceEffectUpsampleBlurVertical);
                Shaders.ScreenSpaceEffectTechnique_BlurVertical.Passes[0].Apply();

                _fullScreenTriangle.Draw(_graphicsDevice);

                _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectBlurFinal);

                Shaders.ScreenSpaceEffectParameter_InverseResolution.SetValue(new Vector2(1.0f / _renderTargetScreenSpaceEffectUpsampleBlurHorizontal.Width,
                    1.0f / _renderTargetScreenSpaceEffectUpsampleBlurHorizontal.Height)*0.5f);
                Shaders.ScreenSpaceEffectParameter_SSAOMap.SetValue(_renderTargetScreenSpaceEffectUpsampleBlurHorizontal);
                Shaders.ScreenSpaceEffectTechnique_BlurHorizontal.Passes[0].Apply();

                _fullScreenTriangle.Draw(_graphicsDevice);

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
        private void DrawEnvironmentMap(EnvironmentSample envSample, Camera camera, GameTime gameTime)
        {
            if (!GameSettings.g_environmentmapping) return;

            _deferredEnvironmentMapRenderModule.DrawEnvironmentMap(_graphicsDevice, camera, _view, _fullScreenTriangle, envSample, gameTime, GameSettings.g_SSReflection_FireflyReduction, GameSettings.g_SSReflection_FireflyThreshold);

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
            //if (!GameSettings.g_EmissiveDraw) return;

            throw new NotImplementedException("Check an older build, emissives are currently not implemented because I switched from World Space to View Space but did not update all the effects yet");

            //Make a new _viewProjection
            //This should actually scale dynamically with the position of the object
            //Note: It would be better if the screen extended the same distance in each direction, right now it would probably be wider than tall

            //Matrix newProjection = Matrix.CreatePerspectiveFieldOfView(Math.Min((float)Math.PI, camera.FieldOfView * GameSettings.g_EmissiveDrawFOVFactor),
            //        GameSettings.g_screenwidth / (float)GameSettings.g_screenheight, 1, GameSettings.g_farplane);

            //Matrix transformedViewProjection = _view * newProjection;

            ////meshMatLib.DrawEmissive(_graphicsDevice, camera, _viewProjection, transformedViewProjection, _inverseViewProjection, _renderTargetEmissive, _renderTargetDiffuse, _renderTargetSpecular, _lightBlendState, _assets.Sphere.Meshes, gameTime);
            ////Performance Profiler
            //if (GameSettings.d_profiler)
            //{
            //    long performanceCurrentTime = _performanceTimer.ElapsedTicks;
            //    GameStats.d_profileDrawEmissive = performanceCurrentTime - _performancePreviousTime;

            //    _performancePreviousTime = performanceCurrentTime;
            //}
        }

        /// <summary>
        /// Compose the render by combining the albedo channel with the light channels
        /// </summary>
        private RenderTarget2D Compose()
        {
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            _graphicsDevice.SetRenderTarget(_renderTargetComposed);
            _graphicsDevice.BlendState = BlendState.Opaque;

            //combine!
            Shaders.DeferredCompose.CurrentTechnique.Passes[0].Apply();
            _fullScreenTriangle.Draw(_graphicsDevice);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileCompose = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }

            return _renderTargetComposed;
        }

        /// <summary>
        /// Not working right now
        /// </summary>
        /// <param name="input"></param>
        /// <param name="meshMaterialLibrary"></param>
        /// <returns></returns>
        private RenderTarget2D DrawSubsurfaceScattering(RenderTarget2D input, MeshMaterialLibrary meshMaterialLibrary)
        {
            return _subsurfaceScatterRenderModule.Draw(_graphicsDevice, input, input, meshMaterialLibrary, _viewProjection );
        }
        
        private void ReconstructDepth()
        {
            if (_viewProjectionHasChanged)
                Shaders.ReconstructDepthParameter_Projection.SetValue(_projection);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            Shaders.ReconstructDepth.CurrentTechnique.Passes[0].Apply();
            _fullScreenTriangle.Draw(_graphicsDevice);
        }

        private RenderTarget2D DrawForward(RenderTarget2D input, MeshMaterialLibrary meshMaterialLibrary, Camera camera, List<PointLight> pointLights)
        {
            if (!GameSettings.g_forwardenable) return input;

            _graphicsDevice.SetRenderTarget(input);
            ReconstructDepth();
            
            return _forwardRenderModule.Draw(_graphicsDevice, input, meshMaterialLibrary, _viewProjection, camera, pointLights, _boundingFrustum);
        }

        private RenderTarget2D DrawBloom(RenderTarget2D input)
        {
            if (GameSettings.g_BloomEnable)
            {
                Texture2D bloom = _bloomFilter.Draw(input, GameSettings.g_screenwidth,
                    GameSettings.g_screenheight);
                
                _graphicsDevice.SetRenderTargets(_renderTargetBloom);

                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

                _spriteBatch.Draw(input,
                    new Rectangle(0, 0, GameSettings.g_screenwidth, GameSettings.g_screenheight), Color.White);
                _spriteBatch.Draw(bloom, new Rectangle(0, 0, GameSettings.g_screenwidth, GameSettings.g_screenheight),
                    Color.White);

                _spriteBatch.End();

                return _renderTargetBloom;
            }
            else
            {

                //_graphicsDevice.SetRenderTarget(_renderTargetBloom);
                //_spriteBatch.Begin(0, BlendState.Opaque, _supersampling > 1 ? SamplerState.LinearWrap : SamplerState.PointClamp);
                //_spriteBatch.Draw(_renderTargetComposed, new Rectangle(0, 0, GameSettings.g_screenwidth, GameSettings.g_screenheight), Color.White);
                //_spriteBatch.End();
                return input;
            }
        }

        /// <summary>
        /// Combine the render with previous frames to get more information per sample and make the image anti-aliased / super sampled
        /// </summary>
        private RenderTarget2D TonemapAndCombineTemporalAntialiasing(RenderTarget2D input)
        {
            if (!GameSettings.g_taa) return input;

            RenderTarget2D output = _temporalAAOffFrame ? _renderTargetTAA_2 : _renderTargetTAA_1;

            _temporalAntialiasingRenderModule.Draw(_graphicsDevice, 
                useTonemap: GameSettings.g_taa_tonemapped,
                currentFrame: input,
                previousFrames: _temporalAAOffFrame? _renderTargetTAA_1 : _renderTargetTAA_2,
                output: output,
                fullScreenTriangle: _fullScreenTriangle, 
                currentViewToPreviousViewProjection: _currentViewToPreviousViewProjection);
            
            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileCombineTemporalAntialiasing = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
            
            return GameSettings.g_taa_tonemapped ? input : output;
        }
        
        private void DrawSignedDistanceFieldFunctions(Camera camera)
        {
            if (!GameSettings.sdf_drawdistance) return;
            
            _distanceFieldRenderModule.Draw(_graphicsDevice, camera, _fullScreenTriangle);

            //_spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp);

            //int height = Math.Max(volumeTexture.Texture.Height / volumeTexture.Texture.Width * GameSettings.g_screenheight, 40);
            //_spriteBatch.Draw(volumeTexture.Texture,
            //    new Rectangle(0, GameSettings.g_screenheight - height, GameSettings.g_screenwidth, height), Color.White);
            //_spriteBatch.End();
            
        }

        /// <summary>
        /// Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
        /// </summary>
        /// <param name="currentOutput"></param>
        /// <param name="editorData"></param>
        private void RenderMode(RenderTarget2D currentInput)
        {
            switch (GameSettings.g_rendermode)
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
                //case RenderModes.Hologram:
                //    DrawMapToScreenToFullScreen(_renderTargetHologram);
                //    break;
                case RenderModes.SSBlur:
                    DrawMapToScreenToFullScreen(_renderTargetScreenSpaceEffectBlurFinal);
                    break;
                //case RenderModes.Emissive:
                //    DrawMapToScreenToFullScreen(_renderTargetEmissive);
                //    break;
                case RenderModes.SSR:
                    DrawMapToScreenToFullScreen(_renderTargetScreenSpaceEffectReflection);
                    break;
                //case RenderModes.SubsurfaceScattering:
                //    DrawMapToScreenToFullScreen(_renderTargetSSS);
                //    break;
                case RenderModes.HDR:
                        DrawMapToScreenToFullScreen(currentInput);
                    break;
                default:
                    DrawPostProcessing(currentInput);
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
        /// <param name="currentInput"></param>
        private void DrawPostProcessing(RenderTarget2D currentInput)
        {
            if (!GameSettings.g_PostProcessing) return;
            
            RenderTarget2D destinationRenderTarget;
            
            destinationRenderTarget = _renderTargetOutput;
            
            Shaders.PostProcessingParameter_ScreenTexture.SetValue(currentInput);
            _graphicsDevice.SetRenderTarget(destinationRenderTarget);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            
            Shaders.PostProcessing.CurrentTechnique.Passes[0].Apply();
            _fullScreenTriangle.Draw(_graphicsDevice);

            if(GameSettings.g_ColorGrading)
            destinationRenderTarget = _colorGradingFilter.Draw(_graphicsDevice, destinationRenderTarget, _lut);

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
            _inverseResolution = new Vector3(1.0f / GameSettings.g_screenwidth, 1.0f / GameSettings.g_screenheight, 0);
            _haltonSequence = null;

            SetUpRenderTargets(GameSettings.g_screenwidth, GameSettings.g_screenheight, false);
        }

        private void SetUpRenderTargets(int width, int height, bool onlyEssentials)
        {
            //Discard first
            if (_renderTargetAlbedo != null)
            {
                _renderTargetAlbedo.Dispose();
                _renderTargetDecalOffTarget.Dispose();
                _renderTargetDepth.Dispose();
                _renderTargetNormal.Dispose();
                _renderTargetComposed.Dispose();
                _renderTargetBloom.Dispose();
                _renderTargetDiffuse.Dispose();
                _renderTargetSpecular.Dispose();
                _renderTargetVolume.Dispose();
                _renderTargetOutput.Dispose();
                //_renderTargetSSS.Dispose();

                _renderTargetScreenSpaceEffectUpsampleBlurVertical.Dispose();

                if (!onlyEssentials)
                {
                    //_renderTargetHologram.Dispose();
                    _renderTargetTAA_1.Dispose();
                    _renderTargetTAA_2.Dispose();
                    _renderTargetSSAOEffect.Dispose();
                    _renderTargetScreenSpaceEffectReflection.Dispose();

                    _renderTargetScreenSpaceEffectUpsampleBlurHorizontal.Dispose();
                    _renderTargetScreenSpaceEffectBlurFinal.Dispose();
                }
            }

            float ssmultiplier = _supersampling;

            int targetWidth = (int)(width * ssmultiplier);
            int targetHeight = (int)(height * ssmultiplier);

            Shaders.BillboardEffectParameter_AspectRatio.SetValue((float)targetWidth / targetHeight);

            _renderTargetAlbedo = new RenderTarget2D(_graphicsDevice, targetWidth,
                targetHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetDecalOffTarget = new RenderTarget2D(_graphicsDevice, targetWidth,
                targetHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetNormal = new RenderTarget2D(_graphicsDevice, targetWidth,
                targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            _renderTargetDepth = new RenderTarget2D(_graphicsDevice, targetWidth,
                targetHeight, false, SurfaceFormat.Single, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            //_renderTargetSSS = new RenderTarget2D(_graphicsDevice, targetWidth,
            //    targetHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetBinding[0] = new RenderTargetBinding(_renderTargetAlbedo);
            _renderTargetBinding[1] = new RenderTargetBinding(_renderTargetNormal);
            _renderTargetBinding[2] = new RenderTargetBinding(_renderTargetDepth);

            _renderTargetDiffuse = new RenderTarget2D(_graphicsDevice, targetWidth,
               targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);

            _renderTargetSpecular = new RenderTarget2D(_graphicsDevice, targetWidth,
               targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            _renderTargetVolume = new RenderTarget2D(_graphicsDevice, targetWidth,
               targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            _lightAccumulationModule.PointLightRenderModule.deferredPointLightParameterResolution.SetValue(new Vector2(targetWidth, targetHeight));

            _renderTargetLightBinding[0] = new RenderTargetBinding(_renderTargetDiffuse);
            _renderTargetLightBinding[1] = new RenderTargetBinding(_renderTargetSpecular);
            _renderTargetLightBinding[2] = new RenderTargetBinding(_renderTargetVolume);

            _renderTargetComposed = new RenderTarget2D(_graphicsDevice, targetWidth,
               targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            
            _renderTargetBloom = new RenderTarget2D(_graphicsDevice, targetWidth,
               targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            
            _renderTargetScreenSpaceEffectUpsampleBlurVertical = new RenderTarget2D(_graphicsDevice, targetWidth,
                targetHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            _renderTargetOutput = new RenderTarget2D(_graphicsDevice, targetWidth,
                targetHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            if (!onlyEssentials)
            {
                _editorRender.SetUpRenderTarget(width, height);

                _renderTargetTAA_1 = new RenderTarget2D(_graphicsDevice, targetWidth, targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                _renderTargetTAA_2 = new RenderTarget2D(_graphicsDevice, targetWidth, targetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

                _temporalAntialiasingRenderModule.Resolution = new Vector2(targetWidth, targetHeight);
                // Shaders.SSReflectionEffectParameter_Resolution.SetValue(new Vector2(target_width, target_height));
                Shaders.EmissiveEffectParameter_Resolution.SetValue(new Vector2(targetWidth, targetHeight));
                
                Shaders.ScreenSpaceReflectionParameter_Resolution.SetValue(new Vector2(targetWidth, targetHeight));
                _deferredEnvironmentMapRenderModule.Resolution = new Vector2(targetWidth, targetHeight);
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


                Vector2 aspectRatio = new Vector2(Math.Min(1.0f, targetWidth / (float)targetHeight), Math.Min(1.0f, targetHeight / (float)targetWidth));
                
                Shaders.ScreenSpaceEffectParameter_AspectRatio.SetValue(aspectRatio);


                //_renderTargetHologram = new RenderTarget2D(_graphicsDevice, targetWidth,
                //    targetHeight, false, SurfaceFormat.Single, DepthFormat.Depth24, 0,
                //    RenderTargetUsage.PreserveContents);
            }

            UpdateRenderMapBindings(onlyEssentials);
        }

        private void UpdateRenderMapBindings(bool onlyEssentials)
        {
            Shaders.BillboardEffectParameter_DepthMap.SetValue(_renderTargetDepth);

            Shaders.ReconstructDepthParameter_DepthMap.SetValue(_renderTargetDepth);

            _lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_AlbedoMap.SetValue(_renderTargetAlbedo);
            _lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_DepthMap.SetValue(_renderTargetDepth);
            _lightAccumulationModule.PointLightRenderModule.deferredPointLightParameter_NormalMap.SetValue(_renderTargetNormal);

            Shaders.deferredDirectionalLightParameter_AlbedoMap.SetValue(_renderTargetAlbedo);
            Shaders.deferredDirectionalLightParameter_DepthMap.SetValue(_renderTargetDepth);
            Shaders.deferredDirectionalLightParameter_NormalMap.SetValue(_renderTargetNormal);

            Shaders.deferredDirectionalLightParameter_SSShadowMap.SetValue(onlyEssentials ? _renderTargetScreenSpaceEffectUpsampleBlurVertical : _renderTargetScreenSpaceEffectBlurFinal);

            _deferredEnvironmentMapRenderModule.AlbedoMap = _renderTargetAlbedo;
            _deferredEnvironmentMapRenderModule.NormalMap = _renderTargetNormal;
            _deferredEnvironmentMapRenderModule.SSRMap = _renderTargetScreenSpaceEffectReflection;
            _deferredEnvironmentMapRenderModule.DepthMap = _renderTargetDepth;

            _decalRenderModule.DepthMap = _renderTargetDepth;

            _distanceFieldRenderModule.DepthMap = _renderTargetDepth;
            
            //_subsurfaceScatterRenderModule.NormalMap = _renderTargetNormal;
            //_subsurfaceScatterRenderModule.AlbedoMap = _renderTargetAlbedo;

            Shaders.DeferredComposeEffectParameter_ColorMap.SetValue(_renderTargetAlbedo);
            Shaders.DeferredComposeEffectParameter_NormalMap.SetValue(_renderTargetNormal);
            Shaders.DeferredComposeEffectParameter_diffuseLightMap.SetValue(_renderTargetDiffuse);
            Shaders.DeferredComposeEffectParameter_specularLightMap.SetValue(_renderTargetSpecular);
            Shaders.DeferredComposeEffectParameter_volumeLightMap.SetValue(_renderTargetVolume);
            Shaders.DeferredComposeEffectParameter_SSAOMap.SetValue(_renderTargetScreenSpaceEffectBlurFinal);
            //Shaders.DeferredComposeEffectParameter_HologramMap.SetValue(_renderTargetHologram);
            // Shaders.DeferredComposeEffectParameter_SSRMap.SetValue(_renderTargetScreenSpaceEffectReflection);

            Shaders.ScreenSpaceEffectParameter_NormalMap.SetValue(_renderTargetNormal);

            Shaders.ScreenSpaceEffectParameter_DepthMap.SetValue(_renderTargetDepth);
            Shaders.ScreenSpaceEffectParameter_SSAOMap.SetValue(_renderTargetSSAOEffect);

            Shaders.ScreenSpaceReflectionParameter_DepthMap.SetValue(_renderTargetDepth);
            Shaders.ScreenSpaceReflectionParameter_NormalMap.SetValue(_renderTargetNormal);
            //Shaders.ScreenSpaceReflectionParameter_TargetMap.SetValue(_renderTargetFinal);

            Shaders.EmissiveEffectParameter_DepthMap.SetValue(_renderTargetDepth);
            Shaders.EmissiveEffectParameter_NormalMap.SetValue(_renderTargetNormal);

            _temporalAntialiasingRenderModule.DepthMap = _renderTargetDepth;  
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

        private void DrawMapToScreenToFullScreen(Texture2D map, BlendState blendState = null, RenderTarget2D output = null)
        {
            if(blendState == null) blendState = BlendState.Opaque;

            int height;
            int width;
            if (Math.Abs(map.Width / (float)map.Height - GameSettings.g_screenwidth / (float)GameSettings.g_screenheight) < 0.001)
            //If same aspectratio
            {
                height = GameSettings.g_screenheight;
                width = GameSettings.g_screenwidth;
            }
            else
            {
                if (GameSettings.g_screenheight < GameSettings.g_screenwidth)
                {
                    //Should be squared!
                    height = GameSettings.g_screenheight;
                    width = GameSettings.g_screenheight;
                }
                else
                {
                    height = GameSettings.g_screenwidth;
                    width = GameSettings.g_screenwidth;
                }
            }
            _graphicsDevice.SetRenderTarget(output);
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

        public void Dispose()
        {
            _graphicsDevice?.Dispose();
            _spriteBatch?.Dispose();
            _gaussianBlur?.Dispose();
            _bloomFilter?.Dispose();
            _lightAccumulationModule?.Dispose();
            _gBufferRenderModule?.Dispose();
            _temporalAntialiasingRenderModule?.Dispose();
            _deferredEnvironmentMapRenderModule?.Dispose();
            _decalRenderModule?.Dispose();
            _assets?.Dispose();
            _renderTargetAlbedo?.Dispose();
            _renderTargetDepth?.Dispose();
            _renderTargetNormal?.Dispose();
            _renderTargetDecalOffTarget?.Dispose();
            _renderTargetDiffuse?.Dispose();
            _renderTargetSpecular?.Dispose();
            _renderTargetVolume?.Dispose();
            _renderTargetComposed?.Dispose();
            _renderTargetBloom?.Dispose();
            _renderTargetTAA_1?.Dispose();
            _renderTargetTAA_2?.Dispose();
            _renderTargetScreenSpaceEffectReflection?.Dispose();
            _renderTargetSSAOEffect?.Dispose();
            _renderTargetScreenSpaceEffectUpsampleBlurVertical?.Dispose();
            _renderTargetScreenSpaceEffectUpsampleBlurHorizontal?.Dispose();
            _renderTargetScreenSpaceEffectBlurFinal?.Dispose();
            _renderTargetOutput?.Dispose();
            _currentOutput?.Dispose();
            _renderTargetCubeMap?.Dispose();
        }
    }

}

