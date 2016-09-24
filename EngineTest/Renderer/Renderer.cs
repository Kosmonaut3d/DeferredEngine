using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineTest.Entities;
using EngineTest.Main;
using EngineTest.Recources;
using EngineTest.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EngineTest.Renderer
{
    public class Renderer
    {
        ///////////////////////////////////////////////////// FIELDS ////////////////////////////////////


        //Graphics
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private QuadRenderer _quadRenderer;

        private float _supersampling = 1;

        private Assets _assets;

        //View Projection
        private bool viewProjectionHasChanged;

        private Matrix _view;
        private Matrix _projection;
        private Matrix _viewProjection;
        private Matrix _inverseViewProjection;

        private BoundingFrustum _boundingFrustum;
        private BoundingFrustum _boundingFrustumShadow;

        //RenderTargets
        public enum RenderModes { Skull, Albedo, Normal, Depth, Deferred, Diffuse, Specular, SSR };

        private RenderTarget2D _renderTargetAlbedo;
        private RenderTargetBinding[] _renderTargetBinding = new RenderTargetBinding[3];

        private RenderTarget2D _renderTargetDepth;
        private RenderTarget2D _renderTargetNormal;
        private RenderTarget2D _renderTargetAccumulation;
        private RenderTarget2D _renderTargetLightReflection;
        private RenderTarget2D _renderTargetLightDepth;
        private RenderTarget2D _renderTargetDiffuse;
        private RenderTarget2D _renderTargetSpecular;
        private RenderTargetBinding[] _renderTargetLightBinding = new RenderTargetBinding[2];

        private RenderTarget2D _renderTargetFinal;
        private RenderTargetBinding[] _renderTargetFinalBinding = new RenderTargetBinding[1];

        private RenderTarget2D _renderTargetSkull;
        private RenderTargetBinding[] _renderTargetSkullBinding = new RenderTargetBinding[1];

        private RenderTarget2D _renderTargetVSMBlur;
        private RenderTargetBinding[] _renderTargetVSMBlurBinding = new RenderTargetBinding[1];

        private RenderTarget2D _renderTargetSSReflection;

        //Cubemap
        private RenderTargetCube _renderTargetCubeMap;

        //BlendStates
        
        private BlendState _lightBlendState;


        /////////////////////////////////////////////////////// FUNCTIONS ////////////////////////////////

        #region initialize
        //Done after Load
        public void Initialize(GraphicsDevice graphicsDevice, Assets assets)
        {
            _graphicsDevice = graphicsDevice;
            _quadRenderer = new QuadRenderer();
            _spriteBatch = new SpriteBatch(graphicsDevice);

            //SetUpRenderTargets(_graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight);

            _assets = assets;
        }

        //Update per frame
        public void Update(GameTime gameTime)
        {

        }

        //Load content
        public void Load(ContentManager content)
        {
            _lightBlendState = new BlendState
            {
                AlphaSourceBlend = Blend.One,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                AlphaDestinationBlend = Blend.One
            };
        }
#endregion

        
        //Main Draw!
        public void Draw(Camera camera, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities, List<PointLight> pointLights)
        {
            //Reset the stat counter
            ResetStats(); 

            //Check if we changed some drastic stuff for which we need to reload some elements
            CheckRenderChanges();

            //Render ShadowMaps
            DrawShadows(meshMaterialLibrary, entities, pointLights, camera);

            //Render EnvironmentMaps
            if ((Input.WasKeyPressed(Keys.C)&&!DebugScreen.ConsoleOpen) || GameSettings.g_EnvironmentMappingEveryFrame || _renderTargetCubeMap == null)
            {
                DrawCubeMap(camera.Position, meshMaterialLibrary, entities, pointLights);
                camera.HasChanged = true;
            }

            //Update our view projection matrices if the camera moved
            UpdateViewProjection(camera, meshMaterialLibrary, entities);

            //Set up our deferred renderer
            SetUpGBuffer();

            DrawGBuffer(meshMaterialLibrary, entities);

            //Light the scene
            DrawLights(pointLights, camera.Position);

            DrawEnvironmentMap(camera);

            //Combine the buffers
            Compose();

            //Show certain buffer stages depending on user input
            RenderMode();

            //Just some object culling, setting up for the next frame
            meshMaterialLibrary.FrustumCullingFinalizeFrame(entities);

        }



        private void DrawCubeMap(Vector3 origin, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities,
            List<PointLight> pointLights)
        {
            if (_renderTargetCubeMap == null) // _renderTargetCubeMap.Dispose();
            {
                _renderTargetCubeMap = new RenderTargetCube(_graphicsDevice, 512, true, SurfaceFormat.Color,
                    DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

                Shaders.deferredEnvironment.Parameters["ReflectionCubeMap"].SetValue(_renderTargetCubeMap);
            }
            SetUpRenderTargets(512, 512);

            _projection = Matrix.CreatePerspectiveFieldOfView((float) (Math.PI/2), 1, 1, 200);

            for (int i = 0; i < 6; i++)
            {
                // render the scene to all cubemap faces
                CubeMapFace cubeMapFace = (CubeMapFace) i;

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

                _viewProjection = _view*_projection;
                _inverseViewProjection = Matrix.Invert(_viewProjection);

                viewProjectionHasChanged = true;

                if (_boundingFrustum != null)
                    _boundingFrustum.Matrix = _viewProjection;
                else
                    _boundingFrustum = new BoundingFrustum(_viewProjection);
                //cull
                meshMaterialLibrary.FrustumCulling(entities, _boundingFrustum, true, origin);

                SetUpGBuffer();

                DrawGBuffer(meshMaterialLibrary, entities);

                DrawLights(pointLights, origin);

                Compose();

                DrawMapToScreenToCube(_renderTargetFinal, _renderTargetCubeMap, cubeMapFace);

            }

            SetUpRenderTargets(1280,800);

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
                default:
                    DrawMapToScreenToFullScreen(_renderTargetFinal);
                    break;
            }
        }

        #region Shadow Mapping

        //todo: don't draw shadows for lights that are not visible
        private void DrawShadows(MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities, List<PointLight> pointLights, Camera camera)
        {
            //Don't render for the first frame, we need a guideline first
            if (_boundingFrustum == null) return;

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (PointLight light in pointLights)
            {
                if (_boundingFrustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint)
                {
                    continue;
                }

                GameStats.shadowMaps += 6;

                if (light.DrawShadow)
                {
                    //Check for static
                    if (!light.StaticShadows || light.shadowMapCube == null)
                    {
                        CreateCubeShadowMap(light, light.ShadowResolution, meshMaterialLibrary, entities);
                        camera.HasChanged = true;
                        light.HasChanged = false;
                    }
                }
            }
        }

        private void CreateCubeShadowMap(PointLight light, int size, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities )
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
                    _graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1, 0);

                    meshMaterialLibrary.Draw(true, _graphicsDevice, LightViewProjection, true, light.HasChanged);
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

                    bool hasAnyObjectMoved = meshMaterialLibrary.FrustumCulling(entities, _boundingFrustumShadow, false, light.Position);

                    // Rendering!

                    if (!hasAnyObjectMoved) continue;

                    _graphicsDevice.SetRenderTarget(light.shadowMapCube, cubeMapFace);

                    meshMaterialLibrary.Draw(shadow: true,
                        graphicsDevice: _graphicsDevice,
                        viewProjection: LightViewProjection,
                        opaque: true,
                        lightViewPointChanged: light.HasChanged,
                        hasAnyObjectMoved: true);
                }

            //Culling!


            }

        }

        #endregion

        #region draw

        private void DrawEnvironmentMap(Camera camera)
        {
            if (!GameSettings.g_EnvironmentMapping) return;

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            Shaders.deferredEnvironmentParameterInverseViewProjection.SetValue(_inverseViewProjection);

           Shaders.deferredEnvironmentParameterCameraPosition.SetValue(camera.Position);

            Shaders.deferredEnvironment.CurrentTechnique = GameSettings.g_SSR
                ? Shaders.deferredEnvironment.Techniques["g_SSR"]
                : Shaders.deferredEnvironment.Techniques["Classic"];
            Shaders.deferredEnvironment.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
        }

        /// <summary>
        /// Draw all light sources in a deferred Renderer!
        /// </summary>
        /// <param name="pointLights"></param>
        /// <param name="camera"></param>
        private void DrawLights(List<PointLight> pointLights, Vector3 cameraOrigin)
        {
            _graphicsDevice.SetRenderTargets(_renderTargetLightBinding);

            DrawPointLights(pointLights, cameraOrigin);
        }

        //Draw the pointlights
        private void DrawPointLights( List<PointLight> pointLights,Vector3 cameraOrigin)
        {
            _graphicsDevice.BlendState = _lightBlendState;

            //If nothing has changed we don't need to update
            if (viewProjectionHasChanged)
            {
                Shaders.deferredPointLightParameterViewProjection.SetValue(_viewProjection);
                Shaders.deferredPointLightParameterCameraPosition.SetValue(cameraOrigin);
                Shaders.deferredPointLightParameterInverseViewProjection.SetValue(_inverseViewProjection);
            }

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            for (int index = 0; index < pointLights.Count; index++)
            {
                PointLight light = pointLights[index];
                DrawPointLight(light, cameraOrigin);
            }
        }

        //Draw each individual Point light
        private void DrawPointLight(PointLight light, Vector3 cameraOrigin)
        {
            //todo: Optimize the culling and do it with the HasChanged thing like the models!
            //first let's check if the light is even in bounds
            if (_boundingFrustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint ||
                !_boundingFrustum.Intersects(light.BoundingSphere))
                return;

            GameStats.LightsDrawn ++;

            Matrix sphereWorldMatrix = Matrix.CreateScale(light.Radius * 1.2f) * Matrix.CreateTranslation(light.Position);
            Shaders.deferredPointLightParameter_World.SetValue(sphereWorldMatrix);

            //light position
            Shaders.deferredPointLightParameter_LightPosition.SetValue(light.Position);
            //set the color, radius and Intensity
            Shaders.deferredPointLightParameter_LightColor.SetValue(light.Color.ToVector3());
            Shaders.deferredPointLightParameter_LightRadius.SetValue(light.Radius);
            Shaders.deferredPointLightParameter_LightIntensity.SetValue(light.Intensity);
            //parameters for specular computations

            float cameraToCenter = Vector3.Distance(cameraOrigin, light.Position);

            bool inside = cameraToCenter < light.Radius*1.2f;
            Shaders.deferredPointLightParameter_Inside.SetValue(inside);

            _graphicsDevice.RasterizerState = inside ? RasterizerState.CullClockwise : RasterizerState.CullCounterClockwise;
            
            Shaders.deferredPointLight.CurrentTechnique.Passes[0].Apply();

            foreach (ModelMesh mesh in _assets.Sphere.Meshes)
            {
                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                {
                    light.ApplyShader();
                    _graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
                    _graphicsDevice.Indices = (meshpart.IndexBuffer);
                    int primitiveCount = meshpart.PrimitiveCount;
                    int vertexOffset = meshpart.VertexOffset;
                    int startIndex = meshpart.StartIndex;

                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
                }
            }
        }

        private void DrawMapToScreenToCube(RenderTarget2D map, RenderTargetCube target, CubeMapFace? face)
        {

            if (face != null) _graphicsDevice.SetRenderTarget(target, (CubeMapFace)face);
           // _graphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp);
            _spriteBatch.Draw(map, new Rectangle(0, 0, map.Width, map.Height), Color.White);
            _spriteBatch.End();
        }

        private void DrawMapToScreenToFullScreen(RenderTarget2D map)
        {
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
            _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.AnisotropicClamp);
            _spriteBatch.Draw(map, new Rectangle(0, 0, width, height), Color.White);
            _spriteBatch.End();
        }

        private void Compose()
        {
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            _graphicsDevice.SetRenderTargets(_renderTargetFinalBinding);

            //Skull depth
            //_deferredCompose.Parameters["average_skull_depth"].SetValue(Vector3.Distance(camera.Position , new Vector3(29, 0, -6.5f)));


            //combine!
            Shaders.DeferredCompose.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
        }

        #endregion

        #region draw GBuffer
        ///////////////////////////////////////////// Draw to GBuffer /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Draw models and everything to the GBuffer
        /// </summary>
        /// <param name="meshMaterialLibrary"></param>
        /// <param name="entities"></param>
        public void DrawGBuffer(MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities)
        {
            //current state should be
            //blendstate.opaque
            //defaultDepthStencilState
            //renderTarget = albedo/normal/depth_v

            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            meshMaterialLibrary.Draw(false, _graphicsDevice, _viewProjection, true);
        }

        #endregion

        #region setup draw
        ///////////////////////////////////////////// Set up /////////////////////////////////////////////////////////////////


        private void ResetStats()
        {
            GameStats.MaterialDraws = 0;
            GameStats.MeshDraws = 0;
            GameStats.LightsDrawn = 0;
            GameStats.shadowMaps = 0;
            GameStats.activeShadowMaps = 0;
        }


        private void CheckRenderChanges()
        {
            //Check if supersampling has changed
            if (_supersampling != GameSettings.g_supersampling)
            {
                _supersampling = GameSettings.g_supersampling;
                SetUpRenderTargets(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight);
            }
        }

        /// <summary>
        /// Set up the view projection matrices
        /// </summary>
        /// <param name="camera"></param>
        private void UpdateViewProjection(Camera camera, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities)
        {
            viewProjectionHasChanged = camera.HasChanged;

            //If the camera didn't do anything we don't need to update this stuff
            if (viewProjectionHasChanged)
            {
                camera.HasChanged = false;

                _view = Matrix.CreateLookAt(camera.Position, camera.Lookat, camera.Up);

                _projection = Matrix.CreatePerspectiveFieldOfView(camera.FieldOfView,
                    GameSettings.g_ScreenWidth / (float)GameSettings.g_ScreenHeight, 1, GameSettings.g_FarPlane);

                Shaders.GBufferEffectParameter_View.SetValue(_view);

                _viewProjection = _view*_projection;
                _inverseViewProjection = Matrix.Invert(_viewProjection);

                if (_boundingFrustum == null)
                {
                    _boundingFrustum = new BoundingFrustum(_viewProjection);
                }
                else
                {
                    _boundingFrustum.Matrix = _viewProjection;
                }

            }

            //We need to update whether or not entities are in our boundingFrustum and then cull them or not!
            meshMaterialLibrary.FrustumCulling(entities, _boundingFrustum, viewProjectionHasChanged, camera.Position);

        }

        /// <summary>
        /// Initialize our GBuffer
        /// </summary>
        private void SetUpGBuffer()
        {
            _graphicsDevice.SetRenderTargets(_renderTargetBinding);

            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            //_graphicsDevice.Clear(ClearOptions.Stencil | ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkOrange, 1.0f, 0);

            //Clear the GBuffer
            Shaders.ClearGBufferEffect.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

            //todo: check if the above is already ccw
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        #endregion

        #region RenderTargetControl


        public void UpdateResolution()
        {
            SetUpRenderTargets(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight);
        }

        private void SetUpRenderTargets(int width, int height)
        {
            //Discard first

            if (_renderTargetAlbedo != null)
            {
                _renderTargetSkull.Dispose();
                _renderTargetAlbedo.Dispose();
                _renderTargetDepth.Dispose();
                _renderTargetNormal.Dispose();
                _renderTargetFinal.Dispose();
                _renderTargetDiffuse.Dispose();
                _renderTargetSpecular.Dispose();
                _renderTargetSSReflection.Dispose();
            }

            //SKULL

            _renderTargetSkull = new RenderTarget2D(_graphicsDevice, width / 2,
                height / 2, false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetSkullBinding[0] = new RenderTargetBinding(_renderTargetSkull);

            //DEFAULT

            float ssmultiplier = _supersampling;

            int target_width = (int) (width * ssmultiplier);
            int target_height = (int) (height * ssmultiplier);

            _renderTargetAlbedo = new RenderTarget2D(_graphicsDevice, target_width,
                target_height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetNormal = new RenderTarget2D(_graphicsDevice, target_width,
                target_height, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            _renderTargetDepth = new RenderTarget2D(_graphicsDevice, target_width,
                target_height, false, SurfaceFormat.Single, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            _renderTargetSSReflection = new RenderTarget2D(_graphicsDevice, target_width,
                target_height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            Shaders.SSReflectionEffectParameter_Resolution.SetValue(new Vector2(target_width, target_height));

            _renderTargetBinding[0] = new RenderTargetBinding(_renderTargetAlbedo);
            _renderTargetBinding[1] = new RenderTargetBinding(_renderTargetNormal);
            _renderTargetBinding[2] = new RenderTargetBinding(_renderTargetDepth);

            _renderTargetDiffuse = new RenderTarget2D(_graphicsDevice, target_width,
               target_height, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            _renderTargetSpecular = new RenderTarget2D(_graphicsDevice, target_width,
               target_height, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            Shaders.deferredPointLightParameterResolution.SetValue(new Vector2(target_width, target_height));

            _renderTargetLightBinding[0] = new RenderTargetBinding(_renderTargetDiffuse);
            _renderTargetLightBinding[1] = new RenderTargetBinding(_renderTargetSpecular);

            _renderTargetFinal = new RenderTarget2D(_graphicsDevice, target_width,
               target_height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            _renderTargetFinalBinding[0] = new RenderTargetBinding(_renderTargetFinal);

            UpdateRenderMapBindings();
        }

        private void UpdateRenderMapBindings()
        {
            Shaders.deferredPointLightParameter_AlbedoMap.SetValue(_renderTargetAlbedo);
            Shaders.deferredPointLightParameter_DepthMap.SetValue(_renderTargetDepth);
            Shaders.deferredPointLightParameter_NormalMap.SetValue(_renderTargetNormal);

            Shaders.deferredEnvironmentParameter_AlbedoMap.SetValue(_renderTargetAlbedo);
            Shaders.deferredEnvironmentParameter_DepthMap.SetValue(_renderTargetDepth);
            Shaders.deferredEnvironmentParameter_NormalMap.SetValue(_renderTargetNormal);

            Shaders.DeferredComposeEffectParameter_ColorMap.SetValue(_renderTargetAlbedo);
            Shaders.DeferredComposeEffectParameter_diffuseLightMap.SetValue(_renderTargetDiffuse);
            Shaders.DeferredComposeEffectParameter_specularLightMap.SetValue(_renderTargetSpecular);

        }

        #endregion

    }
}
