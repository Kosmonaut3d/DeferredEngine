using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EngineTest.Entities;
using EngineTest.Recources;
using EngineTest.Renderer;
using EngineTest.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DirectionalLight = EngineTest.Recources.DirectionalLight;

namespace EngineTest.Main
{
    public class MainLogic
    {
        #region FIELDS
        ////////////////////////////////////////////////////// FIELDS

        private Assets _assets;

        //Camera
        public Camera Camera;

        //Entities
        public MeshMaterialLibrary MeshMaterialLibrary;

        public List<BasicEntity> Entities = new List<BasicEntity>();
        public List<PointLight> PointLights = new List<PointLight>();
        public List<DirectionalLight> DirectionalLights = new List<DirectionalLight>();

        private int _renderModeCycle = 0;

        private PointLight shadowLight;

        private BasicEntity drake;
        private BasicEntity sponza;

        #endregion
        /////////////////////////////////////////////////////// METHODS
        
        //Done after Load
        public void Initialize(Assets assets)
        {
            _assets = assets;

            Camera = new Camera(new Vector3(-80, 0, 10), new Vector3(1, 1, -10));
            MeshMaterialLibrary = new MeshMaterialLibrary();

            ////////////////////////////////////////////////////////////////////////
            //Sponza scene
            
            //    //entities
            sponza = AddEntity(_assets.SponzaModel, Vector3.Zero, Math.PI/2, 0, 0, 0.1f);
            //AddEntity(_assets.TestTubes, _assets.emissiveMaterial2, new Vector3(0, 0, 40), 0,0, 0, 1.8f);
            //drake = AddEntity(_assets.DragonUvSmoothModel, _assets.emissiveMaterial, new Vector3(40, -10, 0), Math.PI / 2, 0, 0, 10);

            //AddEntity(_assets.Stormtrooper, _assets.stormtrooperMaterial, new Vector3(40, 0, 10), Math.PI / 2, 0, 0, 10);

            AddEntity(_assets.DragonUvSmoothModel, _assets.baseMaterial, new Vector3(30, 10, 0), Math.PI / 2, 0, 0, 10);

            AddEntity(_assets.TestCube, _assets.rockMaterial, new Vector3(20, 0, 10), Math.PI / 2, 0, 0, 5);

            //AddEntity(_assets.HelmetModel, new Vector3(70, 0, -10), -Math.PI / 2, 0, -Math.PI / 2, 1);

            //    //Hologram skulls
            //AddEntity(_assets.SkullModel, _assets.hologramMaterial, new Vector3(69, 0, -6.5f), -Math.PI / 2, 0, Math.PI / 2 + 0.3f, 0.9f);
            //AddEntity(_assets.SkullModel, _assets.hologramMaterial, new Vector3(69, 8.5f, -6.5f), -Math.PI / 2, 0, Math.PI / 2 + 0.3f, 0.8f);

            //    //lights
            //shadowLight = AddPointLight(position: new Vector3(-80, 2, 20), radius: 50, color: Color.Wheat, intensity: 20, castShadows: true);

            AddPointLight(position: new Vector3(-20, 0, 40), radius: 120, color: Color.White, intensity: 20, castShadows: true, shadowResolution: 1024, staticShadow: false);

            //for (int i = 0; i < 10; i++)
            //{
            //    AddPointLight(new Vector3(FastRand.NextSingle() * 250 - 125, FastRand.NextSingle() * 40 - 20, FastRand.NextSingle() * 10 - 13), 40, new Color(FastRand.NextInteger(255), FastRand.NextInteger(255), FastRand.NextInteger(255)), 20, true);
            //}
            //AddPointLight(position: new Vector3(+20, -10, -20), radius: 50, color: Color.Orange, intensity: 20, castShadows: true);

            ///////////////////////////////////////////////////////////////////////////////
            //Base scene

            //entities
            AddEntity(_assets.Plane, assets.metalRough02Material, new Vector3(0, 0, 0), 0, 0, 0, 30);

            AddEntity(_assets.Plane, assets.goldMaterial, new Vector3(80, 0, 0), 0, 0, 0, 30);
            //AddEntity(_assets.Plane, assets.metalRough02Material, new Vector3(0, 0, 0), 0, 0, 0, 10);
            //AddEntity(_assets.Plane, assets.silverMaterial, new Vector3(-20, 0, 0), 0, 0, 0, 10);
            //AddEntity(_assets.Plane, assets.metalRough02Material, new Vector3(-40, 0, 0), 0, 0, 0, 10);
            //AddEntity(_assets.Plane, assets.metalRough02Material, new Vector3(20, 20, 0), 0, 0, 0, 10);
            //AddEntity(_assets.Plane, assets.silverMaterial, new Vector3(0, 20, 0), 0, 0, 0, 10);
            //AddEntity(_assets.Plane, assets.metalRough02Material, new Vector3(-20, 20, 0), 0, 0, 0, 10);
            //AddEntity(_assets.Plane, assets.silverMaterial, new Vector3(-40, 20, 0), 0, 0, 0, 10);
            //AddEntity(_assets.Plane, assets.metalRough02Material, new Vector3(-350, 0, 0), 0, 0, 0, 15);

            AddEntity(_assets.HelmetModel, new Vector3(60, 0, 10), Math.PI/2, 0, Math.PI / 2, 1);
            
            //Hologram skulls
            AddEntity(_assets.SkullModel, _assets.hologramMaterial, new Vector3(59, 0, 6.5f), Math.PI / 2, 0, -Math.PI / 2 - 0.3f, 0.9f);
            //AddEntity(_assets.SkullModel, _assets.hologramMaterial, new Vector3(59, -8.5f, 6.5f), Math.PI / 2, 0, -Math.PI / 2 - 0.3f, 0.8f);

            //lights
            AddDirectionalLight(direction: new Vector3(0.2f, -0.2f, -1),
                intensity: 20,
                color: Color.White,
                position: Vector3.UnitZ * 0,
                drawShadows: true,
                shadowWorldSize: 250,
                shadowDepth: 180,
                shadowResolution: 2048,
                shadowFilteringFiltering: DirectionalLight.ShadowFilteringTypes.SoftPCF3x,
                screenspaceShadowBlur: true);

        }



        //////////////////////////////////////////// ADD FUNCTIONS ///////////////////////////////////////////////

        private DirectionalLight AddDirectionalLight(Vector3 direction, int intensity, Color color, Vector3 position = default(Vector3), bool drawShadows = false, float shadowWorldSize = 100, float shadowDepth = 100, int shadowResolution = 512, DirectionalLight.ShadowFilteringTypes shadowFilteringFiltering = DirectionalLight.ShadowFilteringTypes.Poisson, bool screenspaceShadowBlur = false, bool staticshadows = false )
        {
            DirectionalLight light = new DirectionalLight(color: color, 
                intensity: intensity, 
                direction: direction, 
                position: position, 
                drawShadows: drawShadows, 
                shadowSize: shadowWorldSize, 
                shadowDepth: shadowDepth, 
                shadowResolution: shadowResolution, 
                shadowFiltering: shadowFilteringFiltering, 
                screenspaceshadowblur: screenspaceShadowBlur, 
                staticshadows: staticshadows);
            DirectionalLights.Add(light);
            return light;
        }

        //The function to use for new pointlights
        /// <summary>
        /// Add a point light to the list of drawn point lights
        /// </summary>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        /// <param name="intensity"></param>
        /// <param name="castShadows">will render shadow maps</param>
        /// <param name="shadowResolution">shadow map resolution per face. Optional</param>
        /// <param name="staticShadow">if set to true the shadows will not update at all. Dynamic shadows in contrast update only when needed.</param>
        /// <returns></returns>
        private PointLight AddPointLight(Vector3 position, float radius, Color color, float intensity, bool castShadows, int shadowResolution = 256, bool staticShadow = false)
        {
            PointLight light = new PointLight(position, radius, color, intensity, castShadows, shadowResolution, staticShadow);
            PointLights.Add(light);
            return light;
        }



        private BasicEntity AddEntity(Model model, Vector3 position, double angleX, double angleY, double angleZ, float scale)
        {
            BasicEntity entity = new BasicEntity(model,
                null, 
                position: position, 
                angleZ: angleZ, 
                angleX: angleX, 
                angleY: angleY, 
                scale: scale,
                library: MeshMaterialLibrary);
            Entities.Add(entity);

            return entity;
        }

        private BasicEntity AddEntity(Model model, MaterialEffect materialEffect, Vector3 position, double angleX, double angleY, double angleZ, float scale)
        {
            BasicEntity entity = new BasicEntity(model,
                materialEffect,
                position: position,
                angleZ: angleZ,
                angleX: angleX,
                angleY: angleY,
                scale: scale,
                library: MeshMaterialLibrary);
            Entities.Add(entity);

            return entity;
        }

        //Update per frame
        public void Update(GameTime gameTime, bool isActive)
        {
            Input.Update(gameTime, Camera, isActive);

            if (!isActive) return;
            
            float delta = (float) (gameTime.ElapsedGameTime.TotalMilliseconds*60/1000);

            //Make the lights move up and down
            //for (var i = 2; i < PointLights.Count; i++)
            //{
            //    PointLight point = PointLights[i];
            //    point.Position = new Vector3(point.Position.X, point.Position.Y, (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 0.8f + i) * 10 - 13));
            //}

            //drake.AngleZ += 0.02f*delta;

            //_assets.emissiveMaterial2.EmissiveStrength = (float) (Math.Sin(gameTime.TotalGameTime.TotalSeconds*2)+1)/4+1;

            //KeyInputs for specific tasks

            if (DebugScreen.ConsoleOpen) return;

            if (Input.WasKeyPressed(Keys.Space))
            {
                GameSettings.Editor_enable = !GameSettings.Editor_enable;
            }

            if (Input.keyboardState.IsKeyDown(Keys.L))
            {
                AddPointLight(new Vector3(FastRand.NextSingle() * 250 - 125, FastRand.NextSingle() * 50 - 25, FastRand.NextSingle() * 30 - 19), 30, new Color(FastRand.NextInteger(255), FastRand.NextInteger(255), FastRand.NextInteger(255)), 10, false);

            }

            if (Input.keyboardState.IsKeyDown(Keys.NumPad1))
            {
                _assets.silverMaterial.Roughness = Math.Min(1, _assets.silverMaterial.Roughness += 0.02f);
            }
            if (Input.keyboardState.IsKeyDown(Keys.NumPad3))
            {
                _assets.silverMaterial.Roughness = Math.Max(0, _assets.silverMaterial.Roughness -= 0.02f);
            }

            if (Input.keyboardState.IsKeyDown(Keys.Up))
            {
                shadowLight.Position += Vector3.UnitX * delta;
            }
            if (Input.keyboardState.IsKeyDown(Keys.Down))
            {
                shadowLight.Position -= Vector3.UnitX * delta;
            }
            if (Input.keyboardState.IsKeyDown(Keys.Left))
            {
                shadowLight.Position -= Vector3.UnitY * delta;
            }
            if (Input.keyboardState.IsKeyDown(Keys.Right))
            {
                shadowLight.Position += Vector3.UnitY * delta;
            }

            if (Input.WasKeyPressed(Keys.F1))
            {
                _renderModeCycle++;
                if (_renderModeCycle > 10) _renderModeCycle = 0;

                switch (_renderModeCycle)
                {
                    case 0:
                        GameSettings.g_RenderMode = Renderer.Renderer.RenderModes.Deferred;
                        break;
                    case 1:
                        GameSettings.g_RenderMode = Renderer.Renderer.RenderModes.Albedo;
                        break;
                    case 2:
                        GameSettings.g_RenderMode = Renderer.Renderer.RenderModes.Normal;
                        break;
                    case 3:
                        GameSettings.g_RenderMode = Renderer.Renderer.RenderModes.Depth;
                        break;
                    case 4:
                        GameSettings.g_RenderMode = Renderer.Renderer.RenderModes.Diffuse;
                        break;
                    case 5:
                        GameSettings.g_RenderMode = Renderer.Renderer.RenderModes.Specular;
                        break;
                    case 6:
                        GameSettings.g_RenderMode = Renderer.Renderer.RenderModes.SSAO;
                        break;
                    case 7:
                        GameSettings.g_RenderMode = Renderer.Renderer.RenderModes.Hologram;
                        break;
                    case 8:
                        GameSettings.g_RenderMode = Renderer.Renderer.RenderModes.Emissive;
                        break;
                    case 9:
                        GameSettings.g_RenderMode = Renderer.Renderer.RenderModes.DirectionalShadow;
                        break;
                    case 10:
                        GameSettings.g_RenderMode = Renderer.Renderer.RenderModes.SSR;
                        break;

                }
            }
        }


        //Load content
        public void Load(ContentManager content)
        {
            //...
        }
    }
}
