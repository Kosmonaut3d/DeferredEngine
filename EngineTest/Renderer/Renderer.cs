using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineTest.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EngineTest.Renderer
{
    public class Renderer
    {
       
        private GraphicsDevice _graphicsDevice;

        private SpriteBatch _spriteBatch;

        //Mouse Key
        private KeyboardState keyboardState;
        private MouseState mouseState;

        private MouseState mouseLastState;
        private KeyboardState keyboardLastState;

        //FX
        private Effect _lightingEffect;
        private Effect clearBufferEffect;
        private Effect _deferredLight;

        private QuadRenderer quadRenderer;

        private float currentRoughness = 0.3f;

        private bool PBR = true;

        private RenderModes _renderMode;
        private int _renderModeCycle = 1;

        private Vector3 _lightPosition;
        
        //Light
        private List<PointLight> pointLights = new List<PointLight>();


        //Matrices
        private Matrix _world;
        private Matrix _view;
        private Matrix _projection;

        private Camera _camera;

        private Art _art;
        private Vector3 DragonPosition = new Vector3(-10, 0, -10);
        private RenderTarget2D _renderTargetAlbedo;
        private RenderTargetBinding[] _renderTargetBinding = new RenderTargetBinding[3];
        private RenderTargetBinding[] _renderTargetBinding2 = new RenderTargetBinding[3];
        private EffectParameter _lightingEffectWorld;
        private EffectParameter _lightingEffectWorldViewProj;
        private EffectParameter _lightingEffectProjection;
        private EffectParameter _lightEffectView;
        private RenderTarget2D _renderTargetDepth;
        private RenderTarget2D _renderTargetNormal;
        private RenderTarget2D _renderTargetAccumulation;
        private RenderTarget2D _renderTargetLightReflection;
        private RenderTarget2D _renderTargetLightDepth;
        private Effect _deferredLightGI;
        private Effect _deferredCombine;
        private RenderTarget2D _renderTargetDiffuse;
        private RenderTarget2D _renderTargetSpecular;
        private RenderTargetBinding[] _renderTargetLightBinding = new RenderTargetBinding[2];
        public BlendState LightBlendState;
        private Effect _deferredCompose;

        private Random random = new Random(1231);
        private Effect _raymarchingEffect;
        private RenderTarget2D _renderTargetFinal;
        private RenderTargetBinding[] _renderTargetFinalBinding = new RenderTargetBinding[1];

        private enum RenderModes { Default, Albedo, Normal, Depth, Deferred, Diffuse, Specular};

        public Renderer(GraphicsDevice graphicsDevice, ContentManager content)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);
            _camera = new Camera(new Vector3(0, 0, -10), new Vector3(1, 0, -10));
            _art = new Art();
            _art.Load(content);

            _deferredLight = content.Load<Effect>("DeferredPointLight");
            _deferredCompose = content.Load<Effect>("DeferredCompose");

            //_deferredLightGI = content.Load<Effect>("DeferredPointLightGI");
            //_deferredCombine = content.Load<Effect>("DeferredRecombine");

            _lightingEffect = content.Load<Effect>("LightingEffect");
            clearBufferEffect = content.Load<Effect>("ClearGBuffer");

            _raymarchingEffect = content.Load<Effect>("RayMarchReflection");

            LightBlendState = new BlendState
            {
                AlphaSourceBlend = Blend.One,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                AlphaDestinationBlend = Blend.One
            };
            
            

            quadRenderer = new QuadRenderer();

            _lightingEffectWorld = _lightingEffect.Parameters["World"];
            _lightingEffectWorldViewProj = _lightingEffect.Parameters["WorldViewProj"];
            _lightEffectView = _lightingEffect.Parameters["View"];
            _lightingEffectProjection = _lightingEffect.Parameters["Projection"];

            SetUpRenderTargets();

            _renderMode = RenderModes.Deferred;

            pointLights.Add(new PointLight(new Vector3(-10, 0, -50), 200, Color.White, 2));

            pointLights.Add(new PointLight(new Vector3(40,10,-10),40,Color.White,4));
            pointLights.Add(new PointLight(new Vector3(20, 0, -10), 40, Color.BlueViolet, 4));
            pointLights.Add(new PointLight(new Vector3(-20, -5, -5), 40, Color.Yellow, 4));
            InitializePointLights();
            
        }


        public void Update(GameTime gameTime, GameWindow window)
        {
            
            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();

            float mouseAmount = 0.01f;

            Vector3 direction = _camera.Forward;
            direction.Normalize();

            Vector3 normal = Vector3.Cross(direction, _camera.Up);

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                float y = mouseState.Y - mouseLastState.Y;
                float x = mouseState.X - mouseLastState.X;

                _camera.Forward += x*mouseAmount*normal;

                _camera.Forward -= y*mouseAmount*_camera.Up;
                _camera.Forward.Normalize();
            }

            for (var i = 1; i < pointLights.Count; i++)
            {
                PointLight point = pointLights[i];
                point.Position.Z = (float) (Math.Sin(gameTime.TotalGameTime.TotalSeconds*0.8f + i)*5 - 10);
            }

            

            float amount = 0.8f;

            float amountNormal = 0.2f;

            if (keyboardState.IsKeyDown(Keys.W))
            {
                _camera.Position += direction*amount;
            }

            if (keyboardState.IsKeyDown(Keys.L))
            {
                pointLights.Add(new PointLight(new Vector3((float) (random.NextDouble()*250-125), (float) (random.NextDouble()*50-25), (float) (-random.NextDouble()*10)-3), 20, new Color(random.Next(255),random.Next(255),random.Next(255)), 2));

                window.Title = pointLights.Count + "";
            }

            if (keyboardState.IsKeyDown(Keys.S))
            {
                _camera.Position -= direction * amount;
            }

            if (keyboardState.IsKeyDown(Keys.D))
            {
                _camera.Position += normal * amountNormal;
            }

            if (keyboardState.IsKeyDown(Keys.A))
            {
                _camera.Position -= normal * amountNormal;
            }

            if (keyboardState.IsKeyDown(Keys.F5) && keyboardLastState.IsKeyUp(Keys.F5))
            {
                PBR = !PBR;
                _deferredLight.CurrentTechnique = PBR ? _deferredLight.Techniques["PBR"] : _deferredLight.Techniques["Classic"];

                window.Title = "PBR = "+PBR.ToString();
            }

            if (keyboardState.IsKeyDown(Keys.F1) && keyboardLastState.IsKeyUp(Keys.F1))
            {
                _renderModeCycle++;
                if (_renderModeCycle > 6) _renderModeCycle = 0;

                switch (_renderModeCycle)
                {
                    case 0: _renderMode = RenderModes.Default;
                        break;
                    case 1: _renderMode = RenderModes.Albedo;
                        break;
                    case 2: _renderMode = RenderModes.Normal;
                        break;
                    case 3: _renderMode = RenderModes.Depth;
                        break;
                    case 4: _renderMode = RenderModes.Diffuse;
                        break;
                    case 5: _renderMode = RenderModes.Specular;
                        break;
                    case 6: _renderMode = RenderModes.Deferred;
                        break;


                }

                window.Title = _renderMode.ToString();
            }

            //if (keyboardState.IsKeyDown(Keys.Space) && keyboardLastState.IsKeyUp(Keys.Space))
            //{
            //    lightStopped = !lightStopped;
            //}

            //if (keyboardState.IsKeyDown(Keys.F2) && keyboardLastState.IsKeyUp(Keys.F2))
            //{
            //    _renderMode = _renderMode == RenderModes.Deferred ? RenderModes.Combined : RenderModes.Deferred;
            //    window.Title = _renderMode.ToString();
            //}

            mouseLastState = mouseState;
            keyboardLastState = keyboardState;
        }

        public void Draw()
        {
            PrepareSettings();

            Render();
        }

        private void Render()
        {
            //_graphicsDevice.SetRenderTargets(_renderTargetAlbedo, _renderTargetDepth);

            _graphicsDevice.SetRenderTargets(_renderTargetBinding);

            ClearGBuffer();

            _graphicsDevice.Clear(Color.AliceBlue);

            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            DrawModels();



            if (_renderMode == RenderModes.Albedo)
                DrawMapToScreenToTarget(_renderTargetAlbedo, null);
            else if (_renderMode == RenderModes.Normal)
                DrawMapToScreenToTarget(_renderTargetNormal, null);
            else if (_renderMode == RenderModes.Depth)
                DrawMapToScreenToTarget(_renderTargetDepth, null);
            else //if (_renderMode == RenderModes.Deferred)
            {

                //_graphicsDevice.SetRenderTarget(null);
                //DrawPointLight();
                
                _graphicsDevice.SetRenderTargets(_renderTargetLightBinding);
                DrawPointLights();

                if(_renderMode == RenderModes.Diffuse)
                    DrawMapToScreenToTarget(_renderTargetDiffuse, null);
                if(_renderMode == RenderModes.Specular)
                    DrawMapToScreenToTarget(_renderTargetSpecular, null);

                DrawReflection();
            }
            
            //else //GI!
            //{
                
            //    _graphicsDevice.SetRenderTargets(_renderTargetBinding2);
            //    DrawPointLightGI();

            //    if (_renderMode == RenderModes.LightReflection)
            //    {
            //        DrawMapToScreenToTarget(_renderTargetLightReflection, null);
            //    }
            //    else if (_renderMode == RenderModes.LightDepth)
            //    {
            //        DrawMapToScreenToTarget(_renderTargetLightDepth, null);
            //    }
            //    else if (_renderMode == RenderModes.Combined)
            //    {
            //        _graphicsDevice.SetRenderTarget(null);
            //        CombineRender();
            //    }

            //}

            
        }

        private void DrawReflection()
        {
            _graphicsDevice.SetRenderTarget(null);

            _raymarchingEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(_view * _projection));
            _raymarchingEffect.Parameters["ViewProjection"].SetValue(_view * _projection);
            _raymarchingEffect.Parameters["Projection"].SetValue(_projection);
            _raymarchingEffect.Parameters["cameraPosition"].SetValue(_camera.Position);
            _raymarchingEffect.Parameters["cameraDir"].SetValue(_camera.Lookat-_camera.Position);
            _raymarchingEffect.CurrentTechnique.Passes[0].Apply();
            quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
        }

        private void InitializePointLights()
        {

            _deferredLight.Parameters["colorMap"].SetValue(_renderTargetAlbedo);
            _deferredLight.Parameters["normalMap"].SetValue(_renderTargetNormal);
            _deferredLight.Parameters["depthMap"].SetValue(_renderTargetDepth);

            _raymarchingEffect.Parameters["colorMap"].SetValue(_renderTargetFinal);
            _raymarchingEffect.Parameters["normalMap"].SetValue(_renderTargetNormal);
            //_raymarchingEffect.Parameters["depthMap"].SetValue(_renderTargetDepth);


            _deferredCompose.Parameters["colorMap"].SetValue(_renderTargetAlbedo);
            _deferredCompose.Parameters["diffuseLightMap"].SetValue(_renderTargetDiffuse);
            _deferredCompose.Parameters["specularLightMap"].SetValue(_renderTargetSpecular);
        }

        private void DrawPointLights()
        {
            _graphicsDevice.Clear(Color.TransparentBlack);

            _graphicsDevice.BlendState = LightBlendState;

            _deferredLight.Parameters["View"].SetValue(_view);
            _deferredLight.Parameters["Projection"].SetValue(_projection);

            _deferredLight.Parameters["cameraPosition"].SetValue(_camera.Position);
            _deferredLight.Parameters["cameraDirection"].SetValue(_camera.Forward);
            _deferredLight.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(_view * _projection));

            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            foreach (PointLight light in pointLights)
            {
                DrawPointLight(light);
            }

            _graphicsDevice.SetRenderTargets(_renderTargetFinalBinding);
                //combine!
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            _deferredCompose.CurrentTechnique.Passes[0].Apply();
            quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

            //DrawMapToScreenToTarget(_renderTargetSpecular, null);
        }

        //private void CombineRender()
        //{
        //    _deferredCombine.Parameters["colorMap"].SetValue(_renderTargetAccumulation);
        //    _deferredCombine.Parameters["lightReflectionMap"].SetValue(_renderTargetLightReflection);
        //    _deferredCombine.Parameters["lightDepthMap"].SetValue(_renderTargetLightDepth);
        //    _deferredCombine.Parameters["depthMap"].SetValue(_renderTargetDepth);

        //    Vector3 lightPosition = _lightPosition;

        //    Matrix sphereWorldMatrix = Matrix.CreateScale(lightRadius) * Matrix.CreateTranslation(lightPosition);
        //    _deferredCombine.Parameters["World"].SetValue(sphereWorldMatrix);
        //    _deferredCombine.Parameters["View"].SetValue(_view);
        //    _deferredCombine.Parameters["Projection"].SetValue(_projection);
        //    //light position
        //    _deferredCombine.Parameters["lightPosition"].SetValue(lightPosition);
        //    //set the color, radius and Intensity
        //    _deferredCombine.Parameters["lightColor"].SetValue(Vector3.One);
        //    _deferredCombine.Parameters["lightRadius"].SetValue(lightRadius);
        //    _deferredCombine.Parameters["lightIntensity"].SetValue(lightIntensity);
        //    //parameters for specular computations
        //    _deferredCombine.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(_view * _projection));

        //    _deferredCombine.CurrentTechnique.Passes[0].Apply();
        //    quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
        //}

        private void DrawPointLight(PointLight light)
        {

            Matrix sphereWorldMatrix = Matrix.CreateScale(light.Radius) * Matrix.CreateTranslation(light.Position);
            _deferredLight.Parameters["World"].SetValue(sphereWorldMatrix);
            
            //light position
            _deferredLight.Parameters["lightPosition"].SetValue(light.Position);
            //set the color, radius and Intensity
            _deferredLight.Parameters["lightColor"].SetValue(light.Color.ToVector3());
            _deferredLight.Parameters["lightRadius"].SetValue(light.Radius);
            _deferredLight.Parameters["lightIntensity"].SetValue(light.Intensity);
            //parameters for specular computations

            _deferredLight.CurrentTechnique.Passes[0].Apply();
            //quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

            float cameraToCenter = Vector3.Distance(_camera.Position, light.Position);

            if (cameraToCenter < light.Radius)
               _graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            else
                _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            foreach (ModelMesh mesh in _art.Sphere.Meshes)
            {
                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                {
                    _deferredLight.CurrentTechnique.Passes[0].Apply();

                    _graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
                    _graphicsDevice.Indices = (meshpart.IndexBuffer);
                    int primitiveCount = meshpart.PrimitiveCount;
                    int vertexOffset = meshpart.VertexOffset;
                    int vCount = meshpart.NumVertices;
                    int startIndex = meshpart.StartIndex;

                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
                }
            }


        }

        //private void DrawPointLightGI()
        //{
        //    //Could be done once at the beginning
        //    _deferredLightGI.Parameters["colorMap"].SetValue(_renderTargetAlbedo);
        //    _deferredLightGI.Parameters["normalMap"].SetValue(_renderTargetNormal);
        //    _deferredLightGI.Parameters["depthMap"].SetValue(_renderTargetDepth);


        //    Vector3 lightPosition = _lightPosition;


        //    Matrix sphereWorldMatrix = Matrix.CreateScale(lightRadius) * Matrix.CreateTranslation(lightPosition);
        //    _deferredLightGI.Parameters["World"].SetValue(sphereWorldMatrix);
        //    _deferredLightGI.Parameters["View"].SetValue(_view);
        //    _deferredLightGI.Parameters["Projection"].SetValue(_projection);
        //    //light position
        //    _deferredLightGI.Parameters["lightPosition"].SetValue(lightPosition);
        //    //set the color, radius and Intensity
        //    _deferredLightGI.Parameters["lightColor"].SetValue(Vector3.One);
        //    _deferredLightGI.Parameters["lightRadius"].SetValue(lightRadius);
        //    _deferredLightGI.Parameters["lightIntensity"].SetValue(lightIntensity);
        //    //parameters for specular computations
        //    _deferredLightGI.Parameters["cameraPosition"].SetValue(_camera.Position);
        //    _deferredLightGI.Parameters["cameraDirection"].SetValue(_camera.Forward);
        //    _deferredLightGI.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(_view * _projection));

        //    _deferredLightGI.CurrentTechnique.Passes[0].Apply();
        //    quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

        //}

        private void DrawModels()
        {
            Matrix localWorld = Matrix.CreateScale(10)*Matrix.CreateTranslation(DragonPosition)* Matrix.CreateRotationX((float) (-Math.PI/2));

            if (_renderMode == RenderModes.Default)
            {

                //_art.DragonUvSmoothModel.Draw(localWorld, _view, _projection);

                //_art.SponzaModel.Draw(Matrix.Identity*Matrix.CreateRotationX((float) (-Math.PI/2)), _view, _projection);
            }
            else
            {
                DrawModel(_art.SponzaModel, Matrix.CreateScale(0.1f) * Matrix.CreateRotationX((float)(-Math.PI / 2)), false);

                DrawModel(_art.DragonUvSmoothModel, localWorld, true);
            }
        }

        private void DrawModel(Model model, Matrix world, bool drake)
        {
            
            

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                {


                    if (meshpart.Effect is MaterialEffect)
                    {
                        MaterialEffect effect = meshpart.Effect as MaterialEffect;

                        if (effect.HasMask) //Has diffuse for sure then
                        {
                            if (effect.HasNormal && effect.HasSpecular)
                            {
                                _lightingEffect.Parameters["Mask"].SetValue(effect.Mask);
                                _lightingEffect.Parameters["Texture"].SetValue(effect.Diffuse);
                                _lightingEffect.Parameters["NormalMap"].SetValue(effect.Normal);
                                _lightingEffect.Parameters["Specular"].SetValue(effect.Specular);
                                _lightingEffect.CurrentTechnique =
                                    _lightingEffect.Techniques["DrawTextureSpecularNormalMask"];
                            }

                            else if (effect.HasNormal)
                            {
                                _lightingEffect.Parameters["Mask"].SetValue(effect.Mask);
                                _lightingEffect.Parameters["Texture"].SetValue(effect.Diffuse);
                                _lightingEffect.Parameters["NormalMap"].SetValue(effect.Normal);
                                _lightingEffect.CurrentTechnique = _lightingEffect.Techniques["DrawTextureNormalMask"];
                            }

                            else if (effect.HasSpecular)
                            {
                                _lightingEffect.Parameters["Mask"].SetValue(effect.Mask);
                                _lightingEffect.Parameters["Texture"].SetValue(effect.Diffuse);
                                _lightingEffect.Parameters["Specular"].SetValue(effect.Specular);
                                _lightingEffect.CurrentTechnique = _lightingEffect.Techniques["DrawTextureSpecularMask"];
                            }
                            else
                            {
                                _lightingEffect.Parameters["Mask"].SetValue(effect.Mask);
                                _lightingEffect.Parameters["Texture"].SetValue(effect.Diffuse);
                                _lightingEffect.CurrentTechnique = _lightingEffect.Techniques["DrawTextureMask"];
                            }
                        }


                        else
                        {
                            
                            if (effect.HasNormal && effect.HasSpecular && effect.HasDiffuse)
                            {
                                _lightingEffect.Parameters["Texture"].SetValue(effect.Diffuse);
                                _lightingEffect.Parameters["NormalMap"].SetValue(effect.Normal);
                                _lightingEffect.Parameters["Specular"].SetValue(effect.Specular);
                                _lightingEffect.CurrentTechnique =
                                    _lightingEffect.Techniques["DrawTextureSpecularNormal"];
                            }

                            else if (effect.HasNormal && effect.HasDiffuse)
                            {
                                _lightingEffect.Parameters["Texture"].SetValue(effect.Diffuse);
                                _lightingEffect.Parameters["NormalMap"].SetValue(effect.Normal);
                                _lightingEffect.CurrentTechnique = _lightingEffect.Techniques["DrawTextureNormal"];
                            }

                            else if (effect.HasSpecular && effect.HasDiffuse)
                            {
                                _lightingEffect.Parameters["Texture"].SetValue(effect.Diffuse);
                                _lightingEffect.Parameters["Specular"].SetValue(effect.Specular);
                                _lightingEffect.CurrentTechnique = _lightingEffect.Techniques["DrawTextureSpecular"];
                            }

                            else if (effect.HasDiffuse)
                            {
                                _lightingEffect.Parameters["Texture"].SetValue(effect.Diffuse);
                                _lightingEffect.CurrentTechnique = _lightingEffect.Techniques["DrawTexture"];
                            }

                            else
                            {
                                _lightingEffect.CurrentTechnique = _lightingEffect.Techniques["DrawBasic"];
                            }
                        }
                    

                    _lightingEffect.Parameters["DiffuseColor"].SetValue(effect.DiffuseColor);

                       _lightingEffectWorld.SetValue(world);
                       _lightingEffectWorldViewProj.SetValue(world * _view * _projection);

                       _lightingEffect.Parameters["F0"].SetValue(0.1f);
                       _lightingEffect.Parameters["Roughness"].SetValue(effect.Roughness);

                       if (drake)
                       {
                           _lightingEffect.Parameters["F0"].SetValue(0.3f);
                           _lightingEffect.Parameters["Roughness"].SetValue(0.1f);
                       }

                    }

                   
                    _lightingEffect.CurrentTechnique.Passes[0].Apply();

                    _graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
                    _graphicsDevice.Indices = (meshpart.IndexBuffer);
                    int primitiveCount = meshpart.PrimitiveCount;
                    int vertexOffset = meshpart.VertexOffset;
                    int vCount = meshpart.NumVertices;
                    int startIndex = meshpart.StartIndex;

                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
                }
                
            }
        }

        private void DrawMapToScreenToTarget(RenderTarget2D map, RenderTarget2D target)
        {
            _graphicsDevice.SetRenderTarget(target);
            _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp);
            _spriteBatch.Draw(map, new Rectangle(0, 0, map.Width, map.Height), Color.White);
            _spriteBatch.End();

            //_graphicsDevice.Textures[0] = null;
            //_graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }

        private void PrepareSettings()
        {
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.Clear(ClearOptions.Stencil | ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkOrange, 1.0f, 0);

            //Set up transformation
            _world = Matrix.Identity;
            _view = Matrix.CreateLookAt(_camera.Position, _camera.Lookat, _camera.Up);

            _projection = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4,
                    _graphicsDevice.Viewport.AspectRatio, 1, 300);

            _lightEffectView.SetValue(_view);
            _lightingEffectProjection.SetValue(_projection);
        }

        private void SetUpRenderTargets()
        {


            _renderTargetAlbedo = new RenderTarget2D(_graphicsDevice, _graphicsDevice.PresentationParameters.BackBufferWidth,
                _graphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetNormal = new RenderTarget2D(_graphicsDevice, _graphicsDevice.PresentationParameters.BackBufferWidth,
                _graphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetDepth = new RenderTarget2D(_graphicsDevice, _graphicsDevice.PresentationParameters.BackBufferWidth,
                _graphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetBinding[0] = new RenderTargetBinding(_renderTargetAlbedo);
            _renderTargetBinding[1] = new RenderTargetBinding(_renderTargetNormal);
            _renderTargetBinding[2] = new RenderTargetBinding(_renderTargetDepth);

            //_renderTargetAccumulation = new RenderTarget2D(_graphicsDevice, _graphicsDevice.PresentationParameters.BackBufferWidth,
            //    _graphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            //_renderTargetLightReflection = new RenderTarget2D(_graphicsDevice, _graphicsDevice.PresentationParameters.BackBufferWidth,
            //    _graphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            //_renderTargetLightDepth = new RenderTarget2D(_graphicsDevice, _graphicsDevice.PresentationParameters.BackBufferWidth,
            //    _graphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            //_renderTargetBinding2[0] = new RenderTargetBinding(_renderTargetAccumulation);
            //_renderTargetBinding2[1] = new RenderTargetBinding(_renderTargetLightReflection);
            //_renderTargetBinding2[2] = new RenderTargetBinding(_renderTargetLightDepth);


            _renderTargetDiffuse = new RenderTarget2D(_graphicsDevice, _graphicsDevice.PresentationParameters.BackBufferWidth,
               _graphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetSpecular = new RenderTarget2D(_graphicsDevice, _graphicsDevice.PresentationParameters.BackBufferWidth,
               _graphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetLightBinding[0] = new RenderTargetBinding(_renderTargetDiffuse);
            _renderTargetLightBinding[1] = new RenderTargetBinding(_renderTargetSpecular);

            _renderTargetFinal = new RenderTarget2D(_graphicsDevice, _graphicsDevice.PresentationParameters.BackBufferWidth,
               _graphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetFinalBinding[0] = new RenderTargetBinding(_renderTargetFinal);
         }

        private void ClearGBuffer()
        {

            clearBufferEffect.Techniques["Technique1"].Passes[0].Apply();
            quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
        }

    }

}
