using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineTest.Entities;
using EngineTest.Recources;
using EngineTest.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Renderer
{
    public class Renderer
    {
        ///////////////////////////////////////////////////// FIELDS ////////////////////////////////////


        //Graphics
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private QuadRenderer _quadRenderer;

        private int _screenHeight;
        private int _screenWidth;

        private Assets _assets;

        //View Projection
        private bool viewProjectionHasChanged;

        private Matrix _view;
        private Matrix _projection;
        private Matrix _viewProjection;
        private Matrix _inverseViewProjection;

        private BoundingFrustum _boundingFrustum;

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

        //BlendStates
        
        private BlendState _lightBlendState;
        

        /////////////////////////////////////////////////////// FUNCTIONS ////////////////////////////////

        //Done after Load
        public void Initialize(GraphicsDevice graphicsDevice, Assets assets)
        {
            _graphicsDevice = graphicsDevice;
            _quadRenderer = new QuadRenderer();
            _spriteBatch = new SpriteBatch(graphicsDevice);

            _screenWidth = _graphicsDevice.PresentationParameters.BackBufferWidth;
            _screenHeight = _graphicsDevice.PresentationParameters.BackBufferHeight;
            SetUpRenderTargets(_graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight);

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

        //Main Draw!
        public void Draw(Camera camera, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities, List<PointLight> pointLights)
        {
            //Reset the mesh count

            ResetStats();

            UpdateViewProjection(camera, meshMaterialLibrary, entities);

            // DrawShadows

            SetUpGBuffer();

            DrawGBuffer(meshMaterialLibrary, entities);

            DrawLights(pointLights, camera);

            Compose();

            RenderMode();
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

        #region draw

        /// <summary>
        /// Draw all light sources in a deferred Renderer!
        /// </summary>
        /// <param name="pointLights"></param>
        /// <param name="camera"></param>
        private void DrawLights(List<PointLight> pointLights, Camera camera)
        {
            _graphicsDevice.SetRenderTargets(_renderTargetLightBinding);

            DrawPointLights(pointLights, camera);
        }

        //Draw the pointlights
        private void DrawPointLights( List<PointLight> pointLights,Camera camera)
        {
            _graphicsDevice.BlendState = _lightBlendState;

            //If nothing has changed we don't need to update
            if (viewProjectionHasChanged)
            {
                Shaders.deferredPointLightParameterViewProjection.SetValue(_viewProjection);
                Shaders.deferredPointLightParameterCameraPosition.SetValue(camera.Position);
                Shaders.deferredPointLightParameterInverseViewProjection.SetValue(_inverseViewProjection);
            }

            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            for (int index = 0; index < pointLights.Count; index++)
            {
                PointLight light = pointLights[index];
                DrawPointLight(light, camera);
            }
        }

        //Draw each individual Point light
        private void DrawPointLight(PointLight light, Camera _camera)
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

            float cameraToCenter = Vector3.Distance(_camera.Position, light.Position);

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

        private void DrawMapToScreenToFullScreen(RenderTarget2D map)
        {
            int height;
            int width;
            if (Math.Abs(map.Width / (float)map.Height - _screenWidth / (float)_screenHeight) < 0.001)
            //If same aspectration
            {
                height = _screenHeight;
                width = _screenWidth;
            }
            else
            {
                if (_screenHeight < _screenWidth)
                {
                    height = _screenHeight;
                    width = _screenHeight;
                }
                else
                {
                    height = _screenWidth;
                    width = _screenWidth;
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
            //_deferredCompose.Parameters["average_skull_depth"].SetValue(Vector3.Distance(_camera.Position , new Vector3(29, 0, -6.5f)));


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
            //renderTarget = albedo/normal/depth

            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            meshMaterialLibrary.Draw(false, _graphicsDevice, _view*_projection, true);
        }

        #endregion

        #region setup draw
        ///////////////////////////////////////////// Set up /////////////////////////////////////////////////////////////////


        private void ResetStats()
        {
            GameStats.MaterialDraws = 0;
            GameStats.MeshDraws = 0;
            GameStats.LightsDrawn = 0;
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
                    _graphicsDevice.Viewport.AspectRatio, 1, GameSettings.g_FarPlane);

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
            meshMaterialLibrary.MeshCulling(entities, _boundingFrustum, viewProjectionHasChanged);

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
            }

            //SKULL

            _renderTargetSkull = new RenderTarget2D(_graphicsDevice, width / 2,
                height / 2, false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetSkullBinding[0] = new RenderTargetBinding(_renderTargetSkull);

            //DEFAULT

            int ssmultiplier = 1;

            if (GameSettings.g_supersample) ssmultiplier = 2;

            int target_width = width * ssmultiplier;
            int target_height = height * ssmultiplier;

            _renderTargetAlbedo = new RenderTarget2D(_graphicsDevice, target_width,
                target_height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetNormal = new RenderTarget2D(_graphicsDevice, target_width,
                target_height, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetDepth = new RenderTarget2D(_graphicsDevice, target_width,
                target_height, false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

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

            _renderTargetLightBinding[0] = new RenderTargetBinding(_renderTargetDiffuse);
            _renderTargetLightBinding[1] = new RenderTargetBinding(_renderTargetSpecular);

            _renderTargetFinal = new RenderTarget2D(_graphicsDevice, target_width,
               target_height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetFinalBinding[0] = new RenderTargetBinding(_renderTargetFinal);

            UpdateRenderMapBindings();
        }

        private void UpdateRenderMapBindings()
        {
            Shaders.deferredPointLightParameter_AlbedoMap.SetValue(_renderTargetAlbedo);
            Shaders.deferredPointLightParameter_DepthMap.SetValue(_renderTargetDepth);
            Shaders.deferredPointLightParameter_NormalMap.SetValue(_renderTargetNormal);

            Shaders.DeferredComposeEffectParameter_ColorMap.SetValue(_renderTargetAlbedo);
            Shaders.DeferredComposeEffectParameter_diffuseLightMap.SetValue(_renderTargetDiffuse);
            Shaders.DeferredComposeEffectParameter_specularLightMap.SetValue(_renderTargetSpecular);

        }

        #endregion
    }
}
