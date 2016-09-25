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

        private int _renderModeCycle = 0;

        private PointLight shadowLight;

        private BasicEntity drake;

#endregion
        /////////////////////////////////////////////////////// METHODS
        
        //Done after Load
        public void Initialize(Assets assets)
        {
            _assets = assets;

            Camera = new Camera(new Vector3(-80, 0, -10), new Vector3(1, 0, -10));
            MeshMaterialLibrary = new MeshMaterialLibrary();

            //Entities

            drake = AddEntity(_assets.DragonUvSmoothModel, _assets.goldMaterial, new Vector3(40, -10, 1), -Math.PI/2, 0, 0, 10);

            AddEntity(_assets.SponzaModel, Vector3.Zero, -Math.PI/2, 0, 0, 0.1f);

            AddEntity(_assets.HelmetModel, new Vector3(10, 0, -10), -Math.PI / 2, 0, -Math.PI / 2, 1);

            shadowLight = AddPointLight(position: new Vector3(2, 2, -20), radius: 50, color: Color.Wheat, intensity: 20, castShadows: true);

            AddPointLight(position: new Vector3(-20, 0, -20), radius: 100, color: Color.White, intensity: 20, castShadows: true, shadowResolution: 1024, staticShadow: true);

            //AddPointLight(position: new Vector3(-20, 0, -100), radius: 200, color: Color.White, intensity: 20, castShadows: true, shadowResolution: 1024, staticShadow: true);


            //for (int i = 0; i < 10; i++)
            //{
            //    AddPointLight(new Vector3(FastRand.NextSingle() * 250 - 125, FastRand.NextSingle() * 40 - 20, FastRand.NextSingle() * 10 - 13), 40, new Color(FastRand.NextInteger(255), FastRand.NextInteger(255), FastRand.NextInteger(255)), 20, true);
            //}
            AddPointLight(position: new Vector3(+20, -10, -20), radius: 50, color: Color.Orange, intensity: 20, castShadows: true);
        
        }






        //////////////////////////////////////////// ADD FUNCTIONS ///////////////////////////////////////////////

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
        public void Update(GameTime gameTime)
        {
            Input.Update(gameTime, Camera);

            //Make the lights move up and down
            //for (var i = 2; i < PointLights.Count; i++)
            //{
            //    PointLight point = PointLights[i];
            //    point.Position = new Vector3(point.Position.X, point.Position.Y, (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 0.8f + i) * 10 - 13));
            //}

            //KeyInputs for specific tasks

            if (DebugScreen.ConsoleOpen) return;

            float delta = (float) (gameTime.ElapsedGameTime.TotalMilliseconds*60/1000);

            if (Input.keyboardState.IsKeyDown(Keys.L))
            {
                AddPointLight(new Vector3(FastRand.NextSingle() * 250 - 125, FastRand.NextSingle() * 50 - 25, FastRand.NextSingle() * 10 - 3), 30, new Color(FastRand.NextInteger(255), FastRand.NextInteger(255), FastRand.NextInteger(255)), 10, false);

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

            if (Input.keyboardState.IsKeyDown(Keys.NumPad6))
            {
                drake.Position -= Vector3.UnitY * delta;
            }
            if (Input.keyboardState.IsKeyDown(Keys.NumPad4))
            {
                drake.Position += Vector3.UnitY * delta;
            }
            if (Input.keyboardState.IsKeyDown(Keys.NumPad8))
            {
                drake.Position -= Vector3.UnitX * delta;
            }
            if (Input.keyboardState.IsKeyDown(Keys.NumPad2))
            {
                drake.Position += Vector3.UnitX * delta;
            }

            if (Input.WasKeyPressed(Keys.F1))
            {
                _renderModeCycle++;
                if (_renderModeCycle > 7) _renderModeCycle = 0;

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
