using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

        private int _screenWidth;
        private int _screenHeight;

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

        private float glassRoughness = 0.1f;

        private bool PBR = true;

        private RenderModes _renderMode;
        private int _renderModeCycle = 0;

        private Vector3 _lightPosition;
        
        //Light
        private List<PointLight> pointLights = new List<PointLight>();
        private List<SpotLight> spotLights = new List<SpotLight>();

        //Matrices
        private Matrix _world;
        private Matrix _view;
        private Matrix _projection;

        private Camera _camera;

        private Art _art;
        private Vector3 DragonPosition = new Vector3(-10, 0, -10);
        private Vector3 Dragon2Position = new Vector3(80, 2, 1);

        private Matrix skullMatrix;
        private Matrix DragonMatrix;
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

        private RenderTarget2D _renderTargetSkull;
        private RenderTargetBinding[] _renderTargetSkullBinding = new RenderTargetBinding[1];

        private RenderTarget2D _renderTargetVSM;
        private RenderTargetBinding[] _renderTargetVSMBinding = new RenderTargetBinding[1];


        private RenderTarget2D _renderTargetVSMBlur;
        private RenderTargetBinding[] _renderTargetVSMBlurBinding = new RenderTargetBinding[1];

        private Matrix SSRmatrix;
        private Effect _deferredSpotLight;
        private Effect _glass;
        private Effect _gaussBlur;

        private static int transpLightAmount = 20;

        private PointLight[] Closestarray;
        private Vector3[] pointLightPosition = new Vector3[transpLightAmount];
        private Vector3[] pointLightColor = new Vector3[transpLightAmount];
        private float[] pointLightIntensity = new float[transpLightAmount];
        private float[] pointLightRadius = new float[transpLightAmount];

        private Matrix LightViewProjection;
        private Effect _virtualShadowMapGenerate;
        private EffectParameter _param_deferredLightWorld;
        private EffectParameter _param_deferredLightPosition;
        private EffectParameter _param_deferredLightColor;
        private EffectParameter _param_deferredLightRadius;
        private EffectParameter _param_deferredLightIntensity;
        private EffectParameter _param_deferredLightInside;
        private bool dynamicLights = true;
        private Effect _skullEffect;

        private RenderTargetCube _renderTargetCubeMap;
        private TextureCube EnvironmentMap;
        private Effect _deferredEnvironment;

        private enum RenderModes { Skull, Albedo, Normal, Depth, Deferred, Diffuse, Specular};

        public Renderer(GraphicsDevice graphicsDevice, ContentManager content)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);
            _camera = new Camera(new Vector3(0, 0, -10), new Vector3(1, 0, -10));
            _art = new Art();
            _art.Load(content);

            _screenHeight = _graphicsDevice.PresentationParameters.BackBufferHeight;
            _screenWidth = _graphicsDevice.PresentationParameters.BackBufferWidth;


            _virtualShadowMapGenerate = content.Load<Effect>("VirtualShadowMapsGenerate");
            _deferredLight = content.Load<Effect>("DeferredPointLight");
            _deferredSpotLight = content.Load<Effect>("DeferredSpotLight");
            _deferredEnvironment = content.Load<Effect>("DeferredEnvironmentMap");
            _deferredCompose = content.Load<Effect>("DeferredCompose");
            _glass = content.Load<Effect>("Glass");
            _gaussBlur = content.Load<Effect>("GaussianBlur");
            _skullEffect = content.Load<Effect>("skullEffect");

            skullMatrix = Matrix.CreateScale(0.9f)*
                          Matrix.CreateRotationX((float) (-Math.PI/2))*Matrix.CreateRotationY(0)*
                          Matrix.CreateRotationZ((float) (Math.PI/2 + 0.3f))*
                          Matrix.CreateTranslation(new Vector3(29, 0, -6.5f));

            SSRmatrix = new Matrix(0.5f,0,0,0,0,-0.5f, 0,0,0,0,1,0,0,0,0,1);


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

            _param_deferredLightWorld = _deferredLight.Parameters["World"];
            _param_deferredLightPosition = _deferredLight.Parameters["lightPosition"];
            _param_deferredLightColor = _deferredLight.Parameters["lightColor"];
            _param_deferredLightRadius = _deferredLight.Parameters["lightRadius"];
            _param_deferredLightIntensity = _deferredLight.Parameters["lightIntensity"];
            _param_deferredLightInside = _deferredLight.Parameters["inside"];

            quadRenderer = new QuadRenderer();

            _lightingEffectWorld = _lightingEffect.Parameters["World"];
            _lightingEffectWorldViewProj = _lightingEffect.Parameters["WorldViewProj"];
            _lightEffectView = _lightingEffect.Parameters["View"];
            _lightingEffectProjection = _lightingEffect.Parameters["Projection"];

            StaticRenderBuffers();

            _renderMode = RenderModes.Deferred;

            //pointLights.Add(new PointLight(new Vector3(-10, 0, -50), 200, Color.White, 2));

            pointLights.Add(new PointLight(new Vector3(-20, -5, -5), 20, Color.Wheat, 4));

            spotLights.Add(new SpotLight(new Vector3(-20, -3, -50), 120, Color.White, 3, -new Vector3(1,0,1)));
            //spotLights.Add(new SpotLight(new Vector3(-3,-3,-10), 150, Color.White, 6, Vector3.UnitX));
            

            GetClosestLights();

            InitializePointLights();

            RenderCubeMap();

            SetUpRenderTargets(_graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight);

            InitializePointLights();

            _graphicsDevice.SetRenderTarget(null);

        }


        private void RenderCubeMap()
        {
            PrepareShadowSettings();

            RenderShadowMap();

            _renderTargetCubeMap = new RenderTargetCube(_graphicsDevice, 512, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            SetUpRenderTargets(512, 512);

            InitializePointLights();

            _projection = Matrix.CreatePerspectiveFieldOfView((float) (Math.PI/2), 1, 1, 300);

            for (int i = 0; i < 6; i++)
            {
                // render the scene to all cubemap faces
                CubeMapFace cubeMapFace = (CubeMapFace)i;

                switch (cubeMapFace)
                {
                    case CubeMapFace.NegativeX:
                        {
                            _view = Matrix.CreateLookAt(_camera.Position, _camera.Position+ Vector3.Left, Vector3.Up);
                            break;
                        }
                    case CubeMapFace.NegativeY:
                        {
                            _view = Matrix.CreateLookAt(_camera.Position, _camera.Position + Vector3.Down, Vector3.Forward);
                            break;
                        }
                    case CubeMapFace.NegativeZ:
                        {
                            _view = Matrix.CreateLookAt(_camera.Position, _camera.Position + Vector3.Backward, Vector3.Up);
                            break;
                        }
                    case CubeMapFace.PositiveX:
                        {
                            _view = Matrix.CreateLookAt(_camera.Position, _camera.Position + Vector3.Right, Vector3.Up);
                            break;
                        }
                    case CubeMapFace.PositiveY:
                        {
                            _view = Matrix.CreateLookAt(_camera.Position, _camera.Position + Vector3.Up, Vector3.Backward);
                            break;
                        }
                    case CubeMapFace.PositiveZ:
                        {
                            _view = Matrix.CreateLookAt(_camera.Position, _camera.Position + Vector3.Forward, Vector3.Up);
                            break;
                        }
                }

                _graphicsDevice.RasterizerState = RasterizerState.CullNone;
                _graphicsDevice.BlendState = BlendState.Opaque;
                _graphicsDevice.DepthStencilState = DepthStencilState.Default;

                //Set up transformation
                _world = Matrix.Identity;

                Render(null, _renderTargetCubeMap, cubeMapFace, true);

                //_graphicsDevice.SetRenderTarget(_renderTargetCubeMap, cubeMapFace);
                //_graphicsDevice.Clear(Color.White);

            }


            _graphicsDevice.SetRenderTarget(null);


        }


        public void Update(GameTime gameTime, GameWindow window)
        {
            
            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();

            float mouseAmount = 0.01f;

            Vector3 direction = _camera.Forward;
            direction.Normalize();

            Vector3 normal = Vector3.Cross(direction, _camera.Up);

            if(dynamicLights)
            DragonMatrix = Matrix.CreateScale(10) *
                                   Matrix.CreateRotationZ((float)(-Math.PI / 2)) * Matrix.CreateRotationX((float)(gameTime.TotalGameTime.TotalSeconds * 0.4f)) *
                                   Matrix.CreateRotationY((float)(Math.PI / 2)) * Matrix.CreateTranslation(Dragon2Position);

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                float y = mouseState.Y - mouseLastState.Y;
                float x = mouseState.X - mouseLastState.X;

                _camera.Forward += x*mouseAmount*normal;

                _camera.Forward -= y*mouseAmount*_camera.Up;
                _camera.Forward.Normalize();
            }

            if(dynamicLights)
            for (var i = 1; i < pointLights.Count; i++)
            {
                PointLight point = pointLights[i];
                point.Position.Z = (float) (Math.Sin(gameTime.TotalGameTime.TotalSeconds*0.8f + i)*5 - 10);
            }

            

            float amount = 0.8f;

            float amountNormal = 0.2f;

            if (keyboardState.IsKeyDown(Keys.Space) && keyboardLastState.IsKeyUp(Keys.Space))
            {
                dynamicLights = !dynamicLights;
            }

            if (keyboardState.IsKeyDown(Keys.C) && keyboardLastState.IsKeyUp(Keys.C))
            {
                RenderCubeMap();

                SetUpRenderTargets(_graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight);

                InitializePointLights();
            }

            if (keyboardState.IsKeyDown(Keys.W))
            {
                _camera.Position += direction*amount;
            }

            if (keyboardState.IsKeyDown(Keys.L))
            {
                pointLights.Add(new PointLight(new Vector3((float) (random.NextDouble()*250-125), (float) (random.NextDouble()*50-25), (float) (-random.NextDouble()*10)-3), 20, new Color(random.Next(255),random.Next(255),random.Next(255)), 4));
                
                GetClosestLights();
                window.Title = pointLights.Count + "";
            }


            if (keyboardState.IsKeyDown(Keys.K))
            {
                spotLights.Add(new SpotLight(new Vector3((float)(random.NextDouble() * 250 - 125), (float)(random.NextDouble() * 50 - 25), (float)(-random.NextDouble() * 10) - 3), 20, new Color(random.Next(255), random.Next(255), random.Next(255)), 2, Vector3.Left));

                window.Title = pointLights.Count + "";
            }


            if (keyboardState.IsKeyDown(Keys.NumPad1))
            {
                glassRoughness = Math.Min(1, glassRoughness - 0.01f);
            }

            if (keyboardState.IsKeyDown(Keys.NumPad2))
            {
                glassRoughness = Math.Max(0, glassRoughness + 0.01f);
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
                _deferredSpotLight.CurrentTechnique = PBR ? _deferredSpotLight.Techniques["PBR"] : _deferredSpotLight.Techniques["Classic"];
                window.Title = "PBR = "+PBR.ToString();
            }

            if (keyboardState.IsKeyDown(Keys.F1) && keyboardLastState.IsKeyUp(Keys.F1))
            {
                _renderModeCycle++;
                if (_renderModeCycle > 6) _renderModeCycle = 0;

                switch (_renderModeCycle)
                {
                    case 0: _renderMode = RenderModes.Deferred;
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
                    case 6: _renderMode = RenderModes.Skull;
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
            //RenderCubeMap();

            //SetUpRenderTargets(_graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight);

            //InitializePointLights();

            PrepareSettings();

            PrepareShadowSettings();

            RenderShadowMap();

            RenderSkull();

            Render(null, null, null, false);

            //DrawMapToScreenToTarget(_renderTargetVSM, null);
        }

        private void RenderSkull()
        {
            _graphicsDevice.SetRenderTargets(_renderTargetSkullBinding);
            DrawModelSkull(_art.SkullModel, skullMatrix);

            DrawModelSkull(_art.SkullModel, Matrix.CreateScale(0.8f)*skullMatrix*Matrix.CreateTranslation(0.5f,8.5f,-0.5f));
        }

        private void RenderShadowMap()
        {
            _graphicsDevice.SetRenderTargets(_renderTargetVSM);

            Matrix localWorld = Matrix.CreateScale(10) * Matrix.CreateTranslation(DragonPosition) * Matrix.CreateRotationX((float)(-Math.PI / 2));

            _virtualShadowMapGenerate.Parameters["transparent"].SetValue(true);

            DrawModelVSM(_art.DragonUvSmoothModel, DragonMatrix);

            _virtualShadowMapGenerate.Parameters["transparent"].SetValue(false);

            DrawModelVSM(_art.SponzaModel, Matrix.CreateScale(0.1f) * Matrix.CreateRotationX((float)(-Math.PI / 2)));

            DrawModelVSM(_art.DragonUvSmoothModel, localWorld);

            DrawModelVSM(_art.DragonUvSmoothModel, Matrix.CreateScale(10) *
                                   Matrix.CreateRotationZ((float)(-Math.PI / 2)) * Matrix.CreateRotationX(-0.9f) *
                                   Matrix.CreateRotationY((float)(Math.PI / 2)) * Matrix.CreateTranslation(Dragon2Position + Vector3.Down * 15));

            //Blur it

            //DrawGaussianBlur();
            //DrawGaussianBlur();
            DrawGaussianBlur();
            
        }


        private void GetClosestLights()
        {
            Closestarray = pointLights.OrderBy(x => Vector3.Distance(Dragon2Position, x.Position)).ToArray();
        }

        private void PrepareShadowSettings()
        {
            Matrix LightView = Matrix.CreateLookAt(spotLights[0].Position,
                spotLights[0].Position - spotLights[0].Direction, Vector3.Up);
            Matrix LightProjection = Matrix.CreatePerspectiveFieldOfView((float) (Math.PI/2.0f), 1, 1, spotLights[0].Radius);

            _virtualShadowMapGenerate.Parameters["Projection"].SetValue(LightProjection);
            _deferredSpotLight.Parameters["LightProjection"].SetValue(LightProjection);

            LightViewProjection = LightView*LightProjection;
        }

        private void Render(RenderTarget2D target, RenderTargetCube targetCube, CubeMapFace? face, bool cubeMap)
        {
            //_graphicsDevice.SetRenderTargets(_renderTargetAlbedo, _renderTargetDepth);

            _graphicsDevice.SetRenderTargets(_renderTargetBinding);

            ClearGBuffer();

            //_graphicsDevice.Clear(Color.AliceBlue);

            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            DrawModels();

            if (_renderMode == RenderModes.Albedo)
                DrawMapToScreenToFullScreen(_renderTargetAlbedo);
            else if (_renderMode == RenderModes.Normal)
                DrawMapToScreenToFullScreen(_renderTargetNormal);
            else if (_renderMode == RenderModes.Depth)
                DrawMapToScreenToFullScreen(_renderTargetDepth);
            else if (_renderMode == RenderModes.Skull)
                DrawMapToScreenToFullScreen(_renderTargetSkull);
            else //if (_renderMode == RenderModes.Deferred)
            {

                _graphicsDevice.SetRenderTargets(_renderTargetLightBinding);

                _graphicsDevice.Clear(Color.TransparentBlack);

                DrawPointLights();
                DrawSpotLights();
                if(!cubeMap) DrawEnvironmentMap();

                Compose();

                if (!cubeMap)
                {
                    DrawMapToScreenToTarget(_renderTargetFinal, target);
                    DrawTransparents();
                }
                else
                {
                    DrawMapToScreenToCube(_renderTargetFinal, targetCube, face);
                }

                
                //
                //DrawReflection();
                if (_renderMode == RenderModes.Diffuse)
                    DrawMapToScreenToTarget(_renderTargetDiffuse, null);
                if (_renderMode == RenderModes.Specular)
                    DrawMapToScreenToTarget(_renderTargetSpecular, null);

            }
            
        }

        private void DrawEnvironmentMap()
        {
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _deferredEnvironment.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(_view*_projection));
            
            _deferredEnvironment.Parameters["cameraPosition"].SetValue(_camera.Position);
            _deferredEnvironment.Parameters["cameraDirection"].SetValue(_camera.Forward);

            _deferredEnvironment.CurrentTechnique.Passes[0].Apply();
            quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
        }

        private void DrawModelVSM(Model model, Matrix world)
        {
            for (int index = 0; index < model.Meshes.Count; index++)
            {
                ModelMesh mesh = model.Meshes[index];
                if (mesh.Name != "g sponza_04")
                    for (int i = 0; i < mesh.MeshParts.Count; i++)
                    {
                        ModelMeshPart meshpart = mesh.MeshParts[i];
                        _virtualShadowMapGenerate.Parameters["WorldViewProj"].SetValue(world*LightViewProjection);

                        _virtualShadowMapGenerate.CurrentTechnique.Passes[0].Apply();

                        _graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
                        _graphicsDevice.Indices = (meshpart.IndexBuffer);
                        int primitiveCount = meshpart.PrimitiveCount;
                        int vertexOffset = meshpart.VertexOffset;
                        int vCount = meshpart.NumVertices;
                        int startIndex = meshpart.StartIndex;

                        _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex,
                            primitiveCount);
                    }
            }
        }

        private void DrawModelSkull(Model model, Matrix world)
        {
            for (int index = 0; index < model.Meshes.Count; index++)
            {
                ModelMesh mesh = model.Meshes[index];
               
                    for (int i = 0; i < mesh.MeshParts.Count; i++)
                    {
                        ModelMeshPart meshpart = mesh.MeshParts[i];
                        _skullEffect.Parameters["WorldViewProj"].SetValue(world * _view * _projection);
                        _skullEffect.Parameters["World"].SetValue(world);
                        _skullEffect.CurrentTechnique.Passes[0].Apply();

                        _graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
                        _graphicsDevice.Indices = (meshpart.IndexBuffer);
                        int primitiveCount = meshpart.PrimitiveCount;
                        int vertexOffset = meshpart.VertexOffset;
                        int vCount = meshpart.NumVertices;
                        int startIndex = meshpart.StartIndex;

                        _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex,
                            primitiveCount);
                    }
            }
        }

        private void DrawTransparents()
        {
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            _glass.Parameters["World"].SetValue(DragonMatrix);

            _glass.Parameters["View"].SetValue(_view);
            _glass.Parameters["Projection"].SetValue(_projection);
            _glass.Parameters["WorldViewProj"].SetValue(DragonMatrix * _view * _projection);

            _glass.Parameters["LightPosition"].SetValue(spotLights[0].Position);
            _glass.Parameters["LightDirection"].SetValue(spotLights[0].Direction);
            _glass.Parameters["LightIntensity"].SetValue(spotLights[0].Intensity);
            _glass.Parameters["LightColor"].SetValue(spotLights[0].Color.ToVector3());
            _glass.Parameters["LightRadius"].SetValue(spotLights[0].Radius);

            _glass.Parameters["CameraPosition"].SetValue(_camera.Position);
            _glass.Parameters["CameraDirection"].SetValue(_camera.Lookat - _camera.Position);

            //Point lights
            int lowerBound = Math.Min(transpLightAmount, pointLights.Count);

            _glass.Parameters["lowerBound"].SetValue(lowerBound);

            for (var i = 0; i < lowerBound; i++)
            {
                pointLightPosition[i] = Closestarray[i].Position;
                pointLightColor[i] = Closestarray[i].Color.ToVector3();
                pointLightIntensity[i] = Closestarray[i].Intensity;
                pointLightRadius[i] = Closestarray[i].Radius;
            }
            //Sort the poitnLights by how close they are

            _glass.Parameters["PointLightPosition"].SetValue(pointLightPosition);
            _glass.Parameters["PointLightColor"].SetValue(pointLightColor);
            _glass.Parameters["PointLightRadius"].SetValue(pointLightRadius);
            _glass.Parameters["PointLightIntensity"].SetValue(pointLightIntensity);
            
            foreach (Vector3 Position in pointLightPosition)
            {
                float distance = Vector3.Distance(Dragon2Position, Position);

            }

            _glass.Parameters["Roughness"].SetValue(glassRoughness);



            foreach (ModelMesh mesh in _art.DragonUvSmoothModel.Meshes)
            {
                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                {
                    
                    

                    _glass.CurrentTechnique.Passes[0].Apply();

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

        private void Compose()
        {
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            _graphicsDevice.SetRenderTargets(_renderTargetFinalBinding);

            //Skull depth
            //_deferredCompose.Parameters["average_skull_depth"].SetValue(Vector3.Distance(_camera.Position , new Vector3(29, 0, -6.5f)));

            //combine!
            _deferredCompose.CurrentTechnique.Passes[0].Apply();
            quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

        }

        private void DrawReflection()
        {
            _graphicsDevice.SetRenderTarget(null);

            _raymarchingEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(_view * _projection));
            _raymarchingEffect.Parameters["View"].SetValue(_view);
            _raymarchingEffect.Parameters["Projection"].SetValue(_projection);
            _raymarchingEffect.Parameters["ViewProjection"].SetValue(_view*_projection);
            _raymarchingEffect.Parameters["SSProjection"].SetValue(SSRmatrix*_projection);
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

            _deferredSpotLight.Parameters["colorMap"].SetValue(_renderTargetAlbedo);
            _deferredSpotLight.Parameters["normalMap"].SetValue(_renderTargetNormal);
            _deferredSpotLight.Parameters["depthMap"].SetValue(_renderTargetDepth);
            _deferredSpotLight.Parameters["shadowMap"].SetValue(_renderTargetVSM);

            _raymarchingEffect.Parameters["colorMap"].SetValue(_renderTargetFinal);
            _raymarchingEffect.Parameters["normalMap"].SetValue(_renderTargetNormal);
            _raymarchingEffect.Parameters["depthMap"].SetValue(_renderTargetDepth);

            _deferredCompose.Parameters["colorMap"].SetValue(_renderTargetAlbedo);
            _deferredCompose.Parameters["diffuseLightMap"].SetValue(_renderTargetDiffuse);
            _deferredCompose.Parameters["specularLightMap"].SetValue(_renderTargetSpecular);

            
            _deferredEnvironment.Parameters["colorMap"].SetValue(_renderTargetAlbedo);
            _deferredEnvironment.Parameters["normalMap"].SetValue(_renderTargetNormal);
            _deferredEnvironment.Parameters["depthMap"].SetValue(_renderTargetDepth);
            _deferredEnvironment.Parameters["ReflectionCubeMap"].SetValue(_renderTargetCubeMap);

            _deferredCompose.Parameters["skull"].SetValue(_renderTargetSkull);

            _glass.Parameters["Depth"].SetValue(_renderTargetDepth);
            _glass.Parameters["colorMap"].SetValue(_renderTargetFinal);
            _glass.Parameters["ReflectionCubeMap"].SetValue(_renderTargetCubeMap);

        }

        private void DrawPointLights()
        {
            _graphicsDevice.BlendState = LightBlendState;

            _deferredLight.Parameters["View"].SetValue(_view);
            _deferredLight.Parameters["Projection"].SetValue(_projection);

            _deferredLight.Parameters["cameraPosition"].SetValue(_camera.Position);
            _deferredLight.Parameters["cameraDirection"].SetValue(_camera.Forward);
            _deferredLight.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(_view * _projection));

            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            for (int index = 0; index < pointLights.Count; index++)
            {
                PointLight light = pointLights[index];
                DrawPointLight(light);
            }
        }

        private void DrawSpotLights()
        {
            _graphicsDevice.BlendState = LightBlendState;

            _deferredSpotLight.Parameters["View"].SetValue(_view);
            _deferredSpotLight.Parameters["Projection"].SetValue(_projection);

            _deferredSpotLight.Parameters["cameraPosition"].SetValue(_camera.Position);
            _deferredSpotLight.Parameters["cameraDirection"].SetValue(_camera.Forward);
            _deferredSpotLight.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(_view * _projection));

            _deferredSpotLight.Parameters["LightViewProjection"].SetValue(LightViewProjection);

            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            foreach (SpotLight light in spotLights)
            {
                DrawSpotLight(light);
            }

        }

        private void DrawSpotLight(SpotLight light)
        {
            Matrix sphereWorldMatrix = Matrix.CreateScale(light.Radius) * Matrix.CreateTranslation(light.Position);
            _deferredSpotLight.Parameters["World"].SetValue(sphereWorldMatrix);

            //light position
            _deferredSpotLight.Parameters["lightPosition"].SetValue(light.Position);
            _deferredSpotLight.Parameters["lightDirection"].SetValue(light.Direction);
            //set the color, radius and Intensity
            _deferredSpotLight.Parameters["lightColor"].SetValue(light.Color.ToVector3());
            _deferredSpotLight.Parameters["lightRadius"].SetValue(light.Radius);
            _deferredSpotLight.Parameters["lightIntensity"].SetValue(light.Intensity);
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
                    _deferredSpotLight.CurrentTechnique.Passes[0].Apply();

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

       

        private void DrawPointLight(PointLight light)
        {

            Matrix sphereWorldMatrix = Matrix.CreateScale(light.Radius+1) * Matrix.CreateTranslation(light.Position);
            _param_deferredLightWorld.SetValue(sphereWorldMatrix);
            
            //light position
            _param_deferredLightPosition.SetValue(light.Position);
            //set the color, radius and Intensity
            _param_deferredLightColor.SetValue(light.Color.ToVector3());
            _param_deferredLightRadius.SetValue(light.Radius);
            _param_deferredLightIntensity.SetValue(light.Intensity);
            //parameters for specular computations

            _deferredLight.CurrentTechnique.Passes[0].Apply();
            //quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

            float cameraToCenter = Vector3.Distance(_camera.Position, light.Position);

            if (cameraToCenter < light.Radius)
            {
                _param_deferredLightInside.SetValue(true);
                _graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            }
            else
            {
                _param_deferredLightInside.SetValue(false);
                _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }

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


        private void DrawModels()
        {
            Matrix localWorld = Matrix.CreateScale(10)*Matrix.CreateTranslation(DragonPosition)* Matrix.CreateRotationX((float) (-Math.PI/2));

            
                DrawModel(_art.SponzaModel, Matrix.CreateScale(0.1f) * Matrix.CreateRotationX((float)(-Math.PI / 2)), false);

                _lightingEffect.Parameters["F0"].SetValue(0.3f);
                _lightingEffect.Parameters["Roughness"].SetValue(0.1f);
                _lightingEffect.Parameters["DiffuseColor"].SetValue(Color.IndianRed.ToVector3());
                DrawModel(_art.DragonUvSmoothModel, localWorld, true);

                _lightingEffect.Parameters["F0"].SetValue(0.1f);
                _lightingEffect.Parameters["Roughness"].SetValue(0.05f);
                _lightingEffect.Parameters["DiffuseColor"].SetValue(Color.DarkRed.ToVector3()*0.5f);
                DrawModel(_art.DragonUvSmoothModel, Matrix.CreateScale(10) *
                                   Matrix.CreateRotationZ((float)(-Math.PI / 2)) * Matrix.CreateRotationX(-0.9f) *
                                   Matrix.CreateRotationY((float)(Math.PI / 2)) * Matrix.CreateTranslation(Dragon2Position + Vector3.Down*15),true);


                DrawModel(_art.HelmetModel, Matrix.CreateScale(1) *
                                   Matrix.CreateRotationX((float)(-Math.PI / 2)) * Matrix.CreateRotationY(0) *
                                   Matrix.CreateRotationZ((float)(-Math.PI / 2)) * Matrix.CreateTranslation(new Vector3(30,0,-10)),false);


            
        }

        private void DrawModel(Model model, Matrix world, bool drake)
        {
            
            

            foreach (ModelMesh mesh in model.Meshes)
            {
                if(mesh.Name != "g sponza_04") 
                    

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

                        if (!drake)
                        {
                            _lightingEffect.Parameters["Roughness"].SetValue(effect.Roughness);
                            _lightingEffect.Parameters["DiffuseColor"].SetValue(effect.DiffuseColor);
                        }

                        _lightingEffect.Parameters["MaterialType"].SetValue(effect.MaterialType);
                       

                    }

                    _lightingEffectWorld.SetValue(world);
                    _lightingEffectWorldViewProj.SetValue(world * _view * _projection);
                   
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
            _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp);
            _spriteBatch.Draw(map, new Rectangle(0, 0, width, height), Color.White);
            _spriteBatch.End();
        }

        private void DrawMapToScreenToCube(RenderTarget2D map, RenderTargetCube target, CubeMapFace? face)
        {
            
            if (face != null) _graphicsDevice.SetRenderTarget(target, (CubeMapFace) face);
            _graphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp);
            _spriteBatch.Draw(map, new Rectangle(0, 0, map.Width, map.Height), Color.White);
            _spriteBatch.End();
        }

        private void DrawGaussianBlur()
        {
            _graphicsDevice.SetRenderTargets(_renderTargetVSMBlurBinding);

            _gaussBlur.Parameters["InverseRenderTargetDimension"].SetValue(new Vector2(1.0f / 1024.0f, 1.0f / 1024.0f));
            _gaussBlur.Parameters["SceneMap"].SetValue(_renderTargetVSM);

            _gaussBlur.Techniques["GaussianBlur"].Passes["Horizontal"].Apply();
            quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

            _gaussBlur.Parameters["SceneMap"].SetValue(_renderTargetVSMBlur);
            _gaussBlur.Techniques["GaussianBlur"].Passes["Vertical"].Apply();
            _graphicsDevice.SetRenderTargets(_renderTargetVSMBinding);
            quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
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

        private void SetUpRenderTargets(int width, int height)
        {

            //SKULL

            _renderTargetSkull = new RenderTarget2D(_graphicsDevice, width/2,
                height/2, false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetSkullBinding[0] = new RenderTargetBinding(_renderTargetSkull);


            //DEFAULT

            int multiplier = 1;

            int special_width = width*multiplier;
            int special_height = height*multiplier;

            _renderTargetAlbedo = new RenderTarget2D(_graphicsDevice, special_width,
                special_height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetNormal = new RenderTarget2D(_graphicsDevice, special_width,
                special_height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetDepth = new RenderTarget2D(_graphicsDevice, special_width,
                special_height, false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetBinding[0] = new RenderTargetBinding(_renderTargetAlbedo);
            _renderTargetBinding[1] = new RenderTargetBinding(_renderTargetNormal);
            _renderTargetBinding[2] = new RenderTargetBinding(_renderTargetDepth);

            //_renderTargetAccumulation = new RenderTarget2D(_graphicsDevice, width,
            //    height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            //_renderTargetLightReflection = new RenderTarget2D(_graphicsDevice, width,
            //    height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            //_renderTargetLightDepth = new RenderTarget2D(_graphicsDevice, width,
            //    height, false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            //_renderTargetBinding2[0] = new RenderTargetBinding(_renderTargetAccumulation);
            //_renderTargetBinding2[1] = new RenderTargetBinding(_renderTargetLightReflection);
            //_renderTargetBinding2[2] = new RenderTargetBinding(_renderTargetLightDepth);


            _renderTargetDiffuse = new RenderTarget2D(_graphicsDevice, width,
               height, false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetSpecular = new RenderTarget2D(_graphicsDevice, width,
               height, false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetLightBinding[0] = new RenderTargetBinding(_renderTargetDiffuse);
            _renderTargetLightBinding[1] = new RenderTargetBinding(_renderTargetSpecular);

            _renderTargetFinal = new RenderTarget2D(_graphicsDevice, width,
               height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            _renderTargetFinalBinding[0] = new RenderTargetBinding(_renderTargetFinal);
        }

        private void StaticRenderBuffers()
        {
            _renderTargetVSM = new RenderTarget2D(_graphicsDevice, 1024, 1024, false, SurfaceFormat.Vector2, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            _renderTargetVSMBinding[0] = new RenderTargetBinding(_renderTargetVSM);

            _renderTargetVSMBlur = new RenderTarget2D(_graphicsDevice, 1024, 1024, false, SurfaceFormat.Vector2, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            _renderTargetVSMBlurBinding[0] = new RenderTargetBinding(_renderTargetVSMBlur);

        }

        private void ClearGBuffer()
        {

            clearBufferEffect.Techniques["Technique1"].Passes[0].Apply();
            quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
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
    }

}
