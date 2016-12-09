using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using EngineTest.Entities;
using EngineTest.Main;
using EngineTest.Recources;
using EngineTest.Recources.Helper;
using EngineTest.Renderer.Helper;
using EngineTest.Renderer.RenderModules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EngineTest.Renderer
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
            Volumetric
        };

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

        //BlendStates
        private BlendState _lightBlendState;

        //Performance Profiler

        private Stopwatch _performanceTimer = new Stopwatch();
        private long _performancePreviousTime = 0;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //  BASE FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initialize variables
        /// </summary>
        /// <param name="content"></param>
        public void Load(ContentManager content)
        {
            _lightBlendState = new BlendState
            {
                AlphaSourceBlend = Blend.One,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                AlphaDestinationBlend = Blend.One
            };

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

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //  RENDER FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
             
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
        public EditorLogic.EditorReceivedData Draw(Camera camera, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities, List<PointLightSource> pointLights, List<DirectionalLightSource> directionalLights, EditorLogic.EditorSendData editorData, GameTime gameTime)
        {
            //Reset the stat counter, so we can count stats/information for this frame only
            ResetStats();
            
            //Update the mesh data for changes in physics etc.
            meshMaterialLibrary.FrustumCullingStartFrame(entities);

            //Check if we changed some drastic stuff for which we need to reload some elements
            CheckRenderChanges(directionalLights);

            //Render ShadowMaps
            DrawShadows(meshMaterialLibrary, entities, pointLights, directionalLights, camera);

            //Render EnvironmentMaps
            //We do this either when pressing C or at the start of the program (_renderTargetCube == null) or when the game settings want us to do it every frame
            if ((Input.WasKeyPressed(Keys.C) && !DebugScreen.ConsoleOpen) || GameSettings.g_EnvironmentMappingEveryFrame || _renderTargetCubeMap == null)
            {
                DrawCubeMap(camera.Position, meshMaterialLibrary, entities, pointLights, directionalLights, 300, gameTime);
                //Our camera has changed we need to reinitialize stuff because we used a different camera in the cubemap render
                camera.HasChanged = true;
            }
            
            //Update our view projection matrices if the camera moved
            UpdateViewProjection(camera, meshMaterialLibrary, entities);

            //Set up our deferred renderer
            SetUpGBuffer();

            //Draw our meshes to the G Buffer
            DrawGBuffer(meshMaterialLibrary, entities);

            //Draw Hologram projections to a different render target
            DrawHolograms(meshMaterialLibrary);
            
            //Draw Screen Space reflections to a different render target
            DrawScreenSpaceReflections(camera, gameTime);

            //SSAO
            DrawScreenSpaceAmbientOcclusion(camera);

            //Screen space shadows for directional lights to an offscreen render target
            //DrawScreenSpaceDirectionalShadow(dirLights);

            //Upsample/blur our SSAO / screen space shadows
            DrawBilateralBlur();

            //Light the scene
            DrawLights(pointLights, directionalLights, camera.Position, gameTime);

            //Draw the environment cube map as a fullscreen effect on all meshes
            DrawEnvironmentMap(camera.Position);

            //Draw emissive materials on an offscreen render target
            DrawEmissiveEffect(entities, camera, meshMaterialLibrary, gameTime);

            //Compose the scene by combining our lighting data with the gbuffer data
            Compose();

            //Compose the image and add information from previous frames to apply temporal super sampling
            CombineTemporalAntialiasing();
                
            //Draw the elements that we are hovering over with outlines
            if(GameSettings.Editor_enable)
                _editorRender.DrawIds(meshMaterialLibrary, pointLights, directionalLights, _staticViewProjection, _view, editorData);

            //Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
            RenderMode();

            //Additional editor elements that overlay our screen
            if (GameSettings.Editor_enable)
            {
                DrawMapToScreenToFullScreen(_editorRender.GetOutlines(), BlendState.Additive);
                _editorRender.DrawEditorElements(meshMaterialLibrary, pointLights, directionalLights, _staticViewProjection, _view, editorData);

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
        private void DrawCubeMap(Vector3 origin, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities,
          List<PointLightSource> pointLights, List<DirectionalLightSource> dirLights, float farPlane, GameTime gameTime)
        {
            //If our cubemap is not yet initialized, create a new one
            if (_renderTargetCubeMap == null)
            {
                //Create a new cube map
                _renderTargetCubeMap = new RenderTargetCube(_graphicsDevice, GameSettings.g_CubeMapResolution, true, SurfaceFormat.Color,
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

                //Pass these values to our shader
                Shaders.ScreenSpaceEffectParameter_InverseViewProjection.SetValue(_inverseView);
                Shaders.deferredPointLightParameter_InverseView.SetValue(_inverseView);

                //yep we changed
                _viewProjectionHasChanged = true;

                if (_boundingFrustum == null) _boundingFrustum = new BoundingFrustum(_viewProjection);
                else _boundingFrustum.Matrix = _viewProjection;
                ComputeFrustumCorners(_boundingFrustum);

                //Base stuff, for description look in Draw()
                meshMaterialLibrary.FrustumCulling(entities, _boundingFrustum, true, origin);
                SetUpGBuffer();
                DrawGBuffer(meshMaterialLibrary, entities);
                DrawLights(pointLights, dirLights, origin, gameTime);

                //We don't use temporal AA obviously for the cubemap
                bool tempAa = GameSettings.g_TemporalAntiAliasing;
                GameSettings.g_TemporalAntiAliasing = false;
                Shaders.DeferredCompose.CurrentTechnique = Shaders.DeferredComposeTechnique_1;
                Compose();
                Shaders.DeferredCompose.CurrentTechnique = GameSettings.g_SSReflection
                    ? Shaders.DeferredComposeTechnique_SSR
                    : Shaders.DeferredComposeTechnique_1;
                GameSettings.g_TemporalAntiAliasing = tempAa;
                DrawMapToScreenToCube(_renderTargetFinal, _renderTargetCubeMap, cubeMapFace);
            }
            Shaders.DeferredComposeEffectParameter_UseSSAO.SetValue(GameSettings.ssao_Active);

            //Change RTs back to normal
            SetUpRenderTargets(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight, false);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawCubeMap = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

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
        private void CombineTemporalAntialiasing()
        {
            if (!GameSettings.g_TemporalAntiAliasing) return;

            //TEST
            RenderTargetBinding[] testAA = new RenderTargetBinding[2];
            testAA[0] = new RenderTargetBinding(_temporalAAOffFrame ? _renderTargetTAA_2 : _renderTargetTAA_1);
            testAA[1] = new RenderTargetBinding(_renderTargetScreenSpaceEffectUpsampleBlurVertical);
            _graphicsDevice.SetRenderTargets(testAA);

            //_graphicsDevice.SetRenderTarget(_temporalAAOffFrame ? _renderTargetTAA_2 : _renderTargetTAA_1);
            _graphicsDevice.BlendState = BlendState.Opaque;
            
            Shaders.TemporalAntiAliasingEffect_AccumulationMap.SetValue(_temporalAAOffFrame ? _renderTargetTAA_1 : _renderTargetTAA_2);
            Shaders.TemporalAntiAliasingEffect_UpdateMap.SetValue(_renderTargetFinal);
            Shaders.TemporalAntiAliasingEffect_CurrentToPrevious.SetValue(_currentViewToPreviousViewProjection);

            _graphicsDevice.Clear(Color.White);

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
                    DrawMapToScreenToFullScreen(_renderTargetScreenSpaceEffectUpsampleBlurVertical);
                    break;
                case RenderModes.Emissive:
                    DrawMapToScreenToFullScreen(_renderTargetEmissive);
                    break;
                case RenderModes.SSR:
                    DrawMapToScreenToFullScreen(_renderTargetScreenSpaceEffectReflection);
                    break;
                default:
                    if (GameSettings.g_TemporalAntiAliasing)
                    {
                        DrawMapToScreenToFullScreen(_temporalAAOffFrame ? _renderTargetTAA_2 : _renderTargetTAA_1);
                    }
                    else
                    {
                        DrawMapToScreenToFullScreen(_renderTargetFinal);
                    }
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
        
        private void DrawBilateralBlur()
        {
            _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectUpsampleBlurVertical);

            _spriteBatch.Begin(0,BlendState.Additive);

            _spriteBatch.Draw(_renderTargetSSAOEffect, new Rectangle(0,0,(int) (GameSettings.g_ScreenWidth * GameSettings.g_supersampling), (int) (GameSettings.g_ScreenHeight * GameSettings.g_supersampling)), Color.Red);

            _spriteBatch.End();


            if (GameSettings.ssao_Blur)
            {
                _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectUpsampleBlurHorizontal);

                Shaders.ScreenSpaceEffectParameter_SSAOMap.SetValue(_renderTargetScreenSpaceEffectUpsampleBlurVertical);
                Shaders.ScreenSpaceEffectTechnique_BlurVertical.Passes[0].Apply();

                _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One*-1, Vector2.One);

                _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectBlurFinal);

                Shaders.ScreenSpaceEffectParameter_SSAOMap.SetValue(_renderTargetScreenSpaceEffectUpsampleBlurHorizontal);
                Shaders.ScreenSpaceEffectTechnique_BlurHorizontal.Passes[0].Apply();

                _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One*-1, Vector2.One);
            }
            else
            {
                _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectBlurFinal);

                _spriteBatch.Begin(0, BlendState.Opaque);

                _spriteBatch.Draw(_renderTargetScreenSpaceEffectUpsampleBlurVertical, new Rectangle(0, 0, GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight), Color.White);

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

            //BLUR

            //need bilateral upsample

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawScreenSpaceEffect = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        private void DrawScreenSpaceReflections(Camera camera, GameTime gameTime)
        {
            //Another way to make SSR, not good yet

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

            
            //Shaders.ScreenSpaceReflectionParameter_InverseProjection.SetValue(_inverseViewProjection);
            //Shaders.ScreenSpaceReflectionParameter_Projection.SetValue(_projection);
            Shaders.ScreenSpaceReflectionParameter_Projection.SetValue(_projection);

            //Shaders.ScreenSpaceEffect.CurrentTechnique = Shaders.ScreenSpaceEffectTechnique_SSAO;
            Shaders.ScreenSpaceReflectionEffect.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
            
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawSSR = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }

        }

        private void DrawPostProcessing()
        {
            if (!GameSettings.g_PostProcessing) return;

            RenderTarget2D baseRenderTarget;

            if (GameSettings.g_TemporalAntiAliasing)
            {
                baseRenderTarget = (_temporalAAOffFrame ? _renderTargetTAA_2 : _renderTargetTAA_1);
            }
            else
            {
                baseRenderTarget = _renderTargetFinal;
            }

            Shaders.PostProcessingParameter_ScreenTexture.SetValue(baseRenderTarget);
            _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectUpsampleBlurVertical);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            
            Shaders.PostProcessing.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

            DrawMapToScreenToFullScreen(_renderTargetScreenSpaceEffectUpsampleBlurVertical);
        }

        private void DrawEmissiveEffect(List<BasicEntity> entities, Camera camera, MeshMaterialLibrary meshMatLib, GameTime gameTime)
        {
            if (!GameSettings.g_EmissiveDraw) return;

            //Make a new _viewProjection

            //This should actually scale dynamically with the position of the object
            //Note: It would be better if the screen extended the same distance in each direction, right now it would probably be wider than tall
            Matrix newProjection = Matrix.CreatePerspectiveFieldOfView(Math.Min((float)Math.PI, camera.FieldOfView*GameSettings.g_EmissiveDrawFOVFactor),
                    GameSettings.g_ScreenWidth / (float)GameSettings.g_ScreenHeight, 1, GameSettings.g_FarPlane);

            Matrix transformedViewProjection = _view*newProjection;

            meshMatLib.DrawEmissive(_graphicsDevice, camera, _viewProjection, transformedViewProjection, _inverseViewProjection, _renderTargetEmissive, _renderTargetDiffuse, _renderTargetSpecular, _lightBlendState, _assets.Sphere.Meshes, gameTime);
            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawEmissive = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }
        
        private void DrawShadows(MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities, List<PointLightSource> pointLights, List<DirectionalLightSource> dirLights, Camera camera)
        {
            //Don't render for the first frame, we need a guideline first
            if (_boundingFrustum == null) UpdateViewProjection(camera, meshMaterialLibrary, entities);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            for (int index = 0; index < pointLights.Count; index++)
            {
                PointLightSource light = pointLights[index];
                if (_boundingFrustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint)
                {
                    continue;
                }

                if (light.DrawShadow)
                {
                    GameStats.shadowMaps += 6;

                    //Check for static
                    if (!light.StaticShadows || light.shadowMapCube == null)
                    {
                        CreateCubeShadowMap(light, light.ShadowResolution, meshMaterialLibrary, entities);
                        camera.HasChanged = true;
                        light.HasChanged = false;
                    }
                }
            }

            int dirLightShadowed = 0;
            foreach (DirectionalLightSource light in dirLights)
            {
                if (light.DrawShadows)
                {
                    GameStats.shadowMaps += 1;

                    CreateShadowMap(light, light.ShadowResolution, meshMaterialLibrary, entities);

                    camera.HasChanged = true;
                    light.HasChanged = false;

                    if(light.ScreenSpaceShadowBlur) dirLightShadowed++;
                }

                if (dirLightShadowed > 1)
                {
                    throw new NotImplementedException("Only one shadowed DirectionalLight with screen space blur is supported right now");
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
                    //Draw our map!
                    _graphicsDevice.SetRenderTarget(_renderTargetScreenSpaceEffectUpsampleBlurVertical);

                    Shaders.deferredDirectionalLightParameter_LightDirection.SetValue(light.Direction);

                    Shaders.deferredDirectionalLightParameterLightViewProjection.SetValue(light.LightViewProjection);
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
        
        private void CreateShadowMap(DirectionalLightSource lightSource, int shadowResolution, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities)
        {
            //Create a renderTarget if we don't have one yet
            if (lightSource.ShadowMap == null)
            {
                if (lightSource.ShadowFiltering != DirectionalLightSource.ShadowFilteringTypes.VSM)
                {
                    lightSource.ShadowMap = new RenderTarget2D(_graphicsDevice, shadowResolution, shadowResolution, false,
                        SurfaceFormat.HalfSingle, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                }
                else //For a VSM shadowMap we need 2 components
                {
                    lightSource.ShadowMap = new RenderTarget2D(_graphicsDevice, shadowResolution, shadowResolution, false,
                       SurfaceFormat.Vector2, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                }
            }

            MeshMaterialLibrary.RenderType renderType = lightSource.ShadowFiltering == DirectionalLightSource.ShadowFilteringTypes.VSM
                ? MeshMaterialLibrary.RenderType.shadowVSM
                : MeshMaterialLibrary.RenderType.shadowDepth;

            if (lightSource.HasChanged)
            {
                Matrix LightProjection = Matrix.CreateOrthographic(lightSource.ShadowSize, lightSource.ShadowSize,
                    -lightSource.ShadowDepth, lightSource.ShadowDepth);
                Matrix LightView = Matrix.CreateLookAt(lightSource.Position, lightSource.Position + lightSource.Direction, Vector3.Down);

                lightSource.LightViewProjection = LightView*LightProjection;

                _boundingFrustumShadow = new BoundingFrustum(lightSource.LightViewProjection);

                _graphicsDevice.SetRenderTarget(lightSource.ShadowMap);
                _graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1, 0);

                meshMaterialLibrary.FrustumCulling(entities, _boundingFrustumShadow, true, lightSource.Position);

                // Rendering!

                meshMaterialLibrary.Draw(renderType, _graphicsDevice,
                    lightSource.LightViewProjection, lightSource.HasChanged, false);
            }
            else
            {
                _boundingFrustumShadow = new BoundingFrustum(lightSource.LightViewProjection);

                bool hasAnyObjectMoved = meshMaterialLibrary.FrustumCulling(entities: entities, boundingFrustrum: _boundingFrustumShadow, hasCameraChanged: false, cameraPosition: lightSource.Position);

                if (!hasAnyObjectMoved) return;

                meshMaterialLibrary.FrustumCulling(entities: entities, boundingFrustrum: _boundingFrustumShadow, hasCameraChanged: true, cameraPosition: lightSource.Position);

                _graphicsDevice.SetRenderTarget(lightSource.ShadowMap);
                _graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1, 0);

                meshMaterialLibrary.Draw(renderType, _graphicsDevice,
                    lightSource.LightViewProjection, false, true);
            }

            //Blur!
            if (lightSource.ShadowFiltering == DirectionalLightSource.ShadowFilteringTypes.VSM)
            {
                lightSource.ShadowMap = _gaussianBlur.DrawGaussianBlur(lightSource.ShadowMap);
            }

        }

        private void CreateCubeShadowMap(PointLightSource light, int size, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities )
        {
            if (light.shadowMapCube == null)
                light.shadowMapCube = new RenderTargetCube(_graphicsDevice, size, false, SurfaceFormat.Vector2, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            Matrix LightViewProjection = new Matrix();
            CubeMapFace cubeMapFace = CubeMapFace.NegativeX;

            if (light.HasChanged)
            {
                Matrix LightProjection = Matrix.CreatePerspectiveFieldOfView((float) (Math.PI/2), 1, 1, light.Radius);
                Matrix LightView = Matrix.Identity;

                for (int i = 0; i < 6; i++)
                {
                    // render the scene to all cubemap faces
                    cubeMapFace = (CubeMapFace) i;

                    switch (cubeMapFace)
                    {
                        case CubeMapFace.NegativeX:
                        {
                            LightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.Left, Vector3.Up);

                            LightViewProjection = LightView*LightProjection;

                            light.LightViewProjectionNegativeX = LightViewProjection;
                            break;
                        }
                        case CubeMapFace.NegativeY:
                        {
                            LightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.Down,
                                Vector3.Forward);

                            LightViewProjection = LightView*LightProjection;

                            light.LightViewProjectionNegativeY = LightViewProjection;
                            break;
                        }
                        case CubeMapFace.NegativeZ:
                        {
                            LightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.Backward,
                                Vector3.Up);

                            LightViewProjection = LightView*LightProjection;

                            light.LightViewProjectionNegativeZ = LightViewProjection;
                            break;
                        }
                        case CubeMapFace.PositiveX:
                        {
                            LightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.Right, Vector3.Up);

                            LightViewProjection = LightView*LightProjection;

                            light.LightViewProjectionPositiveX = LightViewProjection;
                            break;
                        }
                        case CubeMapFace.PositiveY:
                        {
                            LightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.Up,
                                Vector3.Backward);

                            LightViewProjection = LightView*LightProjection;

                            light.LightViewProjectionPositiveY = LightViewProjection;
                            break;
                        }
                        case CubeMapFace.PositiveZ:
                        {
                            LightView = Matrix.CreateLookAt(light.Position, light.Position + Vector3.Forward, Vector3.Up);

                            LightViewProjection = LightView*LightProjection;

                            light.LightViewProjectionPositiveZ = LightViewProjection;

                            break;
                        }
                    }

                    if (_boundingFrustumShadow != null)
                        _boundingFrustumShadow.Matrix = LightViewProjection;
                    else
                        _boundingFrustumShadow = new BoundingFrustum(LightViewProjection);

                    meshMaterialLibrary.FrustumCulling(entities, _boundingFrustumShadow, true, light.Position);

                    // Rendering!

                    _graphicsDevice.SetRenderTarget(light.shadowMapCube, cubeMapFace);
                    _graphicsDevice.Clear(Color.TransparentBlack);

                    meshMaterialLibrary.Draw(MeshMaterialLibrary.RenderType.shadowVSM, _graphicsDevice,
                        LightViewProjection, true, light.HasChanged);
                }
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    // render the scene to all cubemap faces
                    cubeMapFace = (CubeMapFace) i;

                    switch (cubeMapFace)
                    {
                        case CubeMapFace.NegativeX:
                        {
                            LightViewProjection = light.LightViewProjectionNegativeX;
                            break;
                        }
                        case CubeMapFace.NegativeY:
                        {
                            LightViewProjection = light.LightViewProjectionNegativeY;
                            break;
                        }
                        case CubeMapFace.NegativeZ:
                        {
                            LightViewProjection = light.LightViewProjectionNegativeZ;
                            break;
                        }
                        case CubeMapFace.PositiveX:
                        {
                            LightViewProjection = light.LightViewProjectionPositiveX;
                            break;
                        }
                        case CubeMapFace.PositiveY:
                        {
                            LightViewProjection = light.LightViewProjectionPositiveY;
                            break;
                        }
                        case CubeMapFace.PositiveZ:
                        {
                            LightViewProjection = light.LightViewProjectionPositiveZ;

                            break;
                        }
                    }

                    if (_boundingFrustumShadow != null)
                        _boundingFrustumShadow.Matrix = LightViewProjection;
                    else
                        _boundingFrustumShadow = new BoundingFrustum(LightViewProjection);

                    bool hasAnyObjectMoved = meshMaterialLibrary.FrustumCulling(entities, _boundingFrustumShadow, false,
                        light.Position);

                    // Rendering!

                    if (!hasAnyObjectMoved) continue;

                    _graphicsDevice.SetRenderTarget(light.shadowMapCube, cubeMapFace);
                    //_graphicsDevice.Clear(Color.TransparentBlack);
                    //_graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 0, 0);

                    meshMaterialLibrary.Draw(renderType: MeshMaterialLibrary.RenderType.shadowVSM,
                        graphicsDevice: _graphicsDevice,
                        viewProjection: LightViewProjection,
                        lightViewPointChanged: light.HasChanged,
                        hasAnyObjectMoved: true);
                }

                //Culling!


            }

        }
        
        private void DrawHolograms(MeshMaterialLibrary meshMat)
        {
            if (!GameSettings.g_HologramDraw) return;
            _graphicsDevice.SetRenderTarget(_renderTargetHologram);
            _graphicsDevice.Clear(Color.Black);
            meshMat.Draw(MeshMaterialLibrary.RenderType.hologram, _graphicsDevice, _viewProjection);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawHolograms = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        private void DrawEnvironmentMap(Vector3 cameraPosition)
        {
            if (!GameSettings.g_EnvironmentMapping) return;

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
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

        private void DrawLights(List<PointLightSource> pointLights, List<DirectionalLightSource> dirLights, Vector3 cameraOrigin, GameTime gameTime)
        {
            _graphicsDevice.SetRenderTargets(_renderTargetLightBinding);
            _graphicsDevice.Clear(Color.TransparentBlack);
            DrawPointLights(pointLights, cameraOrigin, gameTime);
            DrawDirectionalLights(dirLights, cameraOrigin);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawLights = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

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

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            for (int index = 0; index < dirLights.Count; index++)
            {
                DirectionalLightSource lightSource = dirLights[index];
                DrawDirectionalLight(lightSource);
            }
        }

        private void DrawDirectionalLight(DirectionalLightSource lightSource)
        {
            Shaders.deferredDirectionalLightParameter_LightColor.SetValue(lightSource.Color.ToVector3());
            Shaders.deferredDirectionalLightParameter_LightDirection.SetValue(lightSource.Direction);
            Shaders.deferredDirectionalLightParameter_LightIntensity.SetValue(lightSource.Intensity);
            lightSource.ApplyShader();
            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
        }

        //Draw the pointlights
        private void DrawPointLights( List<PointLightSource> pointLights,Vector3 cameraOrigin, GameTime gameTime)
        {
            _graphicsDevice.BlendState = _lightBlendState;

            if (pointLights.Count < 1) return;

            //If nothing has changed we don't need to update
            //if (viewProjectionHasChanged)
            //{
            //    //Shaders.deferredPointLightParameter_WorldViewProjection.SetValue(_viewProjection);
            //    //Shaders.deferredPointLightParameterCameraPosition.SetValue(cameraOrigin);
            //    //Shaders.deferredPointLightParameterInverseViewProjection.SetValue(_inverseViewProjection);
            //}

            if (GameSettings.g_VolumetricLights)
                Shaders.deferredPointLightParameter_Time.SetValue((float)gameTime.TotalGameTime.TotalSeconds % 1000);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            for (int index = 0; index < pointLights.Count; index++)
            {
                PointLightSource light = pointLights[index];
                DrawPointLight(light, cameraOrigin);
            }
        }

        //Draw each individual Point light
        private void DrawPointLight(PointLightSource light, Vector3 cameraOrigin)
        {
            //first let's check if the light is even in bounds
            if (_boundingFrustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint ||
                !_boundingFrustum.Intersects(light.BoundingSphere))
                return;

            //For our stats
            GameStats.LightsDrawn ++;
            
            //Send the light parameters to the shader
            if (_viewProjectionHasChanged)
            {
                light.LightViewSpace = light.WorldMatrix*_view;
                light.LightWorldViewProj = light.WorldMatrix*_viewProjection;
            }

            Shaders.deferredPointLightParameter_WorldView.SetValue(light.LightViewSpace);
            Shaders.deferredPointLightParameter_WorldViewProjection.SetValue(light.LightWorldViewProj);
            Shaders.deferredPointLightParameter_LightPosition.SetValue(light.LightViewSpace.Translation);
            Shaders.deferredPointLightParameter_LightColor.SetValue(light.ColorV3);
            Shaders.deferredPointLightParameter_LightRadius.SetValue(light.Radius);
            Shaders.deferredPointLightParameter_LightIntensity.SetValue(light.Intensity);
            
            //Compute whether we are inside or outside and use 
            float cameraToCenter = Vector3.Distance(cameraOrigin, light.Position);
            int inside = cameraToCenter < light.Radius*1.2f ? 1 : -1;
            Shaders.deferredPointLightParameter_Inside.SetValue(inside);

            //If we are inside compute the backfaces, otherwise frontfaces of the sphere
            _graphicsDevice.RasterizerState = inside > 0 ? RasterizerState.CullClockwise : RasterizerState.CullCounterClockwise;
            
            //Draw the sphere
            ModelMeshPart meshpart = _assets.SphereMeshPart;
            light.ApplyShader(_inverseView);
            _graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
            _graphicsDevice.Indices = (meshpart.IndexBuffer);
            int primitiveCount = meshpart.PrimitiveCount;
            int vertexOffset = meshpart.VertexOffset;
            int startIndex = meshpart.StartIndex;

            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
                
        }
    
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
            _spriteBatch.Begin(0, blendState, _supersampling>1 ? SamplerState.LinearWrap : SamplerState.PointClamp, null, null);
            _spriteBatch.Draw(map, new Rectangle(0, 0, width, height), Color.White);
            _spriteBatch.End();
        }

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
        
        public void DrawGBuffer(MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities)
        {
            //current state should be
            //blendstate.opaque
            //defaultDepthStencilState
            //renderTarget = albedo/normal/depth_v

            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            meshMaterialLibrary.Draw(renderType: MeshMaterialLibrary.RenderType.opaque, graphicsDevice: _graphicsDevice, viewProjection: _viewProjection, lightViewPointChanged: true, view: _view);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawGBuffer = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }
        
        private void CheckRenderChanges(List<DirectionalLightSource> dirLights)
        {
            if (_g_FarClip != GameSettings.g_FarPlane)
            {
                _g_FarClip = GameSettings.g_FarPlane;
                Shaders.GBufferEffectParameter_FarClip.SetValue(_g_FarClip);
                Shaders.deferredPointLightParameter_FarClip.SetValue(_g_FarClip);
                Shaders.BillboardEffectParameter_FarClip.SetValue(_g_FarClip);
                Shaders.ScreenSpaceReflectionParameter_FarClip.SetValue(_g_FarClip);
            }

            if (_g_SSReflectionNoise != GameSettings.g_SSReflectionNoise)
            {
                _g_SSReflectionNoise = GameSettings.g_SSReflectionNoise;
                if(!_g_SSReflectionNoise) Shaders.ScreenSpaceReflectionParameter_Time.SetValue(0.0f);
            }

            //Check if supersampling has changed
            if (_supersampling != GameSettings.g_supersampling)
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
                    if(light.ShadowMap!=null) light.ShadowMap.Dispose();
                    light.ShadowMap = null;

                    light.ShadowFiltering = (DirectionalLightSource.ShadowFilteringTypes) (_forceShadowFiltering - 1);

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
        
        private void UpdateViewProjection(Camera camera, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities)
        {
            _viewProjectionHasChanged = camera.HasChanged;

            if (GameSettings.g_TemporalAntiAliasing)
            {
                _viewProjectionHasChanged = true;
                _temporalAAOffFrame = !_temporalAAOffFrame;

                //if (_temporalAAFrame >= 4) _temporalAAFrame = 0;
            }

            //If the camera didn't do anything we don't need to update this stuff
            if (_viewProjectionHasChanged)
            {
                camera.HasChanged = false;
                
                camera.HasMoved = false;
                
                _view = Matrix.CreateLookAt(camera.Position, camera.Lookat, camera.Up);

                _inverseView = Matrix.Invert(_view);

                Shaders.deferredPointLightParameter_InverseView.SetValue(_inverseView);

                _projection = Matrix.CreatePerspectiveFieldOfView(camera.FieldOfView,
                    GameSettings.g_ScreenWidth / (float)GameSettings.g_ScreenHeight, 1, GameSettings.g_FarPlane);
                
                //Shaders.GBufferEffectParameter_View.SetValue(_view);
                Shaders.GBufferEffectParameter_Camera.SetValue(camera.Position);
                
                _viewProjection = _view*_projection;
                _staticViewProjection = _viewProjection;
                _currentViewToPreviousViewProjection = Matrix.Invert(_view)*_previousViewProjection;

                //_currentToPrevious = Matrix.Invert(_previousViewProjection) * _viewProjection;

                if (GameSettings.g_TemporalAntiAliasing)
                {
                    if (GameSettings.g_TemporalAntiAliasingJitterMode == 0)
                    {
                        float translation = _temporalAAOffFrame ? 0.5f : -0.5f;
                        _viewProjection = _viewProjection *
                                          Matrix.CreateTranslation(new Vector3(translation / GameSettings.g_ScreenWidth,
                                              translation / GameSettings.g_ScreenHeight, 0));
                    }
                    else if (GameSettings.g_TemporalAntiAliasingJitterMode == 1)
                    {
                        //Create a random direction!
                        float randomAngle = FastRand.NextAngle();
                        Vector3 translation = new Vector3((float)Math.Sin(randomAngle) / GameSettings.g_ScreenWidth, (float)Math.Cos(randomAngle) / GameSettings.g_ScreenHeight, 0) * 0.5f;
                        _viewProjection = _viewProjection *
                                          Matrix.CreateTranslation(translation);

                    }
                    else if (GameSettings.g_TemporalAntiAliasingJitterMode == 2)
                    {
                        Vector3 translation = GetHaltonSequence();
                        _viewProjection = _viewProjection *
                                          Matrix.CreateTranslation(translation);
                    }
                }

                _previousViewProjection = _viewProjection;

                _inverseViewProjection = Matrix.Invert(_viewProjection);
                
                if (_boundingFrustum == null) _boundingFrustum = new BoundingFrustum(_staticViewProjection);
                else _boundingFrustum.Matrix = _staticViewProjection;
                
                ComputeFrustumCorners(_boundingFrustum);
            }

            //We need to update whether or not entities are in our boundingFrustum and then cull them or not!
            meshMaterialLibrary.FrustumCulling(entities, _boundingFrustum, _viewProjectionHasChanged, camera.Position);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileUpdateViewProjection = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }
        
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
                        int i = index+1;

                        while (i > 0)
                        {
                            f = f/baseValue;
                            result = result + f*(i%baseValue);
                            i = i/baseValue; //floor / int()
                        }

                        if (baseValue == 2)
                            _haltonSequence[index].X = (result - 0.5f)*2 * _inverseResolution.X;
                        else
                            _haltonSequence[index].Y = (result - 0.5f)*2 * _inverseResolution.Y;
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
            Shaders.TemporalAntiAliasingEffect_FrustumCorners.SetValue(_currentFrustumCorners);
        }
        
        private void SetUpGBuffer()
        {
            _graphicsDevice.SetRenderTargets(_renderTargetBinding);

            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            //_graphicsDevice.Clear(ClearOptions.Stencil | ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkOrange, 1.0f, 0);

            ////Clear the GBuffer
            if (GameSettings.g_ClearGBuffer)
            {
                Shaders.ClearGBufferEffect.CurrentTechnique.Passes[0].Apply();
                _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One*-1, Vector2.One);
            }

            ////Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileSetupGBuffer = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }
        
        public void UpdateResolution()
        {
            _inverseResolution = new Vector3(1.0f / GameSettings.g_ScreenWidth, 1.0f / GameSettings.g_ScreenHeight, 0);
            _haltonSequence = null;

            SetUpRenderTargets(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight, false);
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


            //DEFAULT

            float ssmultiplier = _supersampling;

            int target_width = (int) (width * ssmultiplier);
            int target_height = (int) (height * ssmultiplier);

            Shaders.BillboardEffectParameter_AspectRatio.SetValue((float)target_width / target_height);

            _renderTargetAlbedo = new RenderTarget2D(_graphicsDevice, target_width,
                target_height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetNormal = new RenderTarget2D(_graphicsDevice, target_width,
                target_height, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            _renderTargetDepth = new RenderTarget2D(_graphicsDevice, target_width,
                target_height, false, SurfaceFormat.Single, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            //Half res!

            _renderTargetBinding[0] = new RenderTargetBinding(_renderTargetAlbedo);
            _renderTargetBinding[1] = new RenderTargetBinding(_renderTargetNormal);
            _renderTargetBinding[2] = new RenderTargetBinding(_renderTargetDepth);

            _renderTargetDiffuse = new RenderTarget2D(_graphicsDevice, target_width,
               target_height, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            _renderTargetSpecular = new RenderTarget2D(_graphicsDevice, target_width,
               target_height, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            _renderTargetVolume = new RenderTarget2D(_graphicsDevice, target_width,
               target_height, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            Shaders.deferredPointLightParameterResolution.SetValue(new Vector2(target_width, target_height));

            _renderTargetLightBinding[0] = new RenderTargetBinding(_renderTargetDiffuse);
            _renderTargetLightBinding[1] = new RenderTargetBinding(_renderTargetSpecular);
            _renderTargetLightBinding[2] = new RenderTargetBinding(_renderTargetVolume);

            _renderTargetFinal = new RenderTarget2D(_graphicsDevice, target_width,
               target_height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            _renderTargetFinalBinding[0] = new RenderTargetBinding(_renderTargetFinal);

            _renderTargetScreenSpaceEffectUpsampleBlurVertical = new RenderTarget2D(_graphicsDevice, target_width,
                target_height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            if (!onlyEssentials)
            {
                _editorRender.SetUpRenderTarget(width, height);

                _renderTargetTAA_1 = new RenderTarget2D(_graphicsDevice, target_width, target_height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                _renderTargetTAA_2 = new RenderTarget2D(_graphicsDevice, target_width, target_height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                
                Shaders.TemporalAntiAliasingEffect_Resolution.SetValue(new Vector2(target_width, target_height));
                // Shaders.SSReflectionEffectParameter_Resolution.SetValue(new Vector2(target_width, target_height));
                Shaders.EmissiveEffectParameter_Resolution.SetValue(new Vector2(target_width, target_height));

                _renderTargetEmissive = new RenderTarget2D(_graphicsDevice, target_width,
                    target_height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

                _renderTargetScreenSpaceEffectUpsampleBlurHorizontal = new RenderTarget2D(_graphicsDevice, target_width,
                    target_height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

                _renderTargetScreenSpaceEffectBlurFinal = new RenderTarget2D(_graphicsDevice, target_width,
                    target_height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

                Shaders.ScreenSpaceEffectParameter_InverseResolution.SetValue(new Vector2(1.0f/target_width,
                    1.0f/target_height));

                Shaders.ScreenSpaceReflectionParameter_Resolution.SetValue(new Vector2(target_width, target_height));
                _renderTargetScreenSpaceEffectReflection = new RenderTarget2D(_graphicsDevice, target_width,
                    target_height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                ///////////////////
                /// HALF RESOLUTION
                
                target_width /= 2;
                target_height /= 2;

                _renderTargetSSAOEffect = new RenderTarget2D(_graphicsDevice, target_width,
                    target_height, false, SurfaceFormat.HalfSingle, DepthFormat.None, 0,
                    RenderTargetUsage.DiscardContents);


                _renderTargetHologram = new RenderTarget2D(_graphicsDevice, target_width,
                    target_height, false, SurfaceFormat.Single, DepthFormat.Depth24, 0,
                    RenderTargetUsage.PreserveContents);



            }

            UpdateRenderMapBindings(onlyEssentials);
        }

        private void UpdateRenderMapBindings(bool onlyEssentials)
        {
            Shaders.BillboardEffectParameter_DepthMap.SetValue(_renderTargetDepth);

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
        }
        
        private void CpuRayMarch(Camera camera)
        {
            if(GameSettings.e_CPURayMarch)

            if (Input.WasKeyPressed(Keys.K))
                _cpuRayMarch.Calculate(_renderTargetDepth, _renderTargetNormal, _projection, _inverseView, _inverseViewProjection,
                    camera, _currentFrustumCorners);

            _cpuRayMarch.Draw();
        }

    }
}

