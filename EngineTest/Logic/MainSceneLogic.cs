using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.Helper.HelperGeometry;
using DeferredEngine.Renderer.RenderModules.Signed_Distance_Fields.SDF_Generator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DirectionalLight = DeferredEngine.Entities.DirectionalLight;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Quaternion = BEPUutilities.Quaternion;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace DeferredEngine.Logic
{
    public class MainSceneLogic
    {
        #region FIELDS

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        private Assets _assets;
        
        public Camera Camera;


        //mesh library, holds all the meshes and their materials
        public MeshMaterialLibrary MeshMaterialLibrary;

        public readonly List<BasicEntity> BasicEntities = new List<BasicEntity>();
        public readonly List<Decal> Decals = new List<Decal>();
        public readonly List<PointLight> PointLights = new List<PointLight>();
        public readonly List<DirectionalLight> DirectionalLights = new List<DirectionalLight>();
        public readonly List<DebugEntity> DebugEntities = new List<DebugEntity>();
        public EnvironmentSample EnvironmentSample;

        //Which render target are we currently displaying?
        private int _renderModeCycle;
        private Space _physicsSpace;

        //SDF
        public SdfGenerator _sdfGenerator;

        private BasicEntity testEntity;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //  MAIN FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Done after Load
        public void Initialize(Assets assets, Space space, GraphicsDevice graphicsDevice)
        {
            _assets = assets;
            _physicsSpace = space;

            MeshMaterialLibrary = new MeshMaterialLibrary(graphicsDevice);

            SetUpEditorScene(graphicsDevice);
        }

        //Load our default setup!
        private void SetUpEditorScene(GraphicsDevice graphics)
        {
            ////////////////////////////////////////////////////////////////////////
            // Camera

            //Set up our starting camera position

            // NOTE: Coordinate system depends on Camera.up,
            //       Right now z is going up, it's not depth!

            Camera = new Camera(position: new Vector3(-88, -11f, 4), lookat: new Vector3(38, 8, 32));

            EnvironmentSample = new EnvironmentSample(new Vector3(-45, -5, 5));
            
            _sdfGenerator = new SdfGenerator();

            ////////////////////////////////////////////////////////////////////////
            // GUI

            ////////////////////////////////////////////////////////////////////////
            // Static geometry

            // NOTE: If you don't pass a materialEffect it will use the default material from the object

            BasicEntity testEntity = AddEntity(model: _assets.SponzaModel,
                position: Vector3.Zero,
                angleX: Math.PI / 2,
                angleY: 0,
                angleZ: 0,
                scale: 0.1f,
                hasStaticPhysics: false);//CHANGE BACK


            //AddEntity(model: _assets.CloneTrooper,
            //    position: new Vector3(20, 0, 10),
            //    angleX: Math.PI / 2,
            //    angleY: 0,
            //    angleZ: 0,
            //    scale: 10.4f);

            for (int x = -5; x <= 5; x++)
            {
                for (int y = -5; y <= 5; y++)
                {
                    AddEntity(model: _assets.Plane,
                        materialEffect: ((x + 5 + y + 5) % 2 == 1) ? _assets.MirrorMaterial : _assets.MetalRough03Material,
                        position: new Vector3(30 + x * 4, y * 4 + 4, 0),
                        angleX: 0,
                        angleY: 0,
                        angleZ: 0,
                        scale: 2);
                }
            }

            AddEntity(model: _assets.StanfordDragonLowpoly,
                materialEffect: _assets.BaseMaterial,
                position: new Vector3(40, -10, 0),
                angleX: Math.PI / 2,
                angleY: 0,
                angleZ: 0,
                scale: 10);

            ////////////////////////////////////////////////////////////////////////
            // Dynamic geometry

            // NOTE: We first have to create a physics object and then apply said object to a rendered model
            // BEPU could use non-default meshes, but that is much much more expensive so I am using just default ones right now
            // ... so -> spheres, boxes etc.
            // For dynamic meshes I could use the same way i have static meshes, but use - MobileMesh - instead

            // NOTE: Our physics entity's position will be overwritten, so it doesn't matter
            // NOTE: If a physics object has mass it will move, otherwise it is static

            Entity physicsEntity;

            //Just a ground box where nothing should fall through
            //_physicsSpace.Add(new Box(new BEPUutilities.Vector3(0, 0, -0.5f), 1000, 1000, 1));

            _physicsSpace.Add(physicsEntity = new Sphere(position: BEPUutilities.Vector3.Zero, radius: 5, mass: 50));
            AddEntity(model: _assets.IsoSphere,
                materialEffect: _assets.AlphaBlendRim,
                position: new Vector3(20, 0, 10),
                angleX: Math.PI / 2,
                angleY: 0,
                angleZ: 0,
                scale: 5,
                PhysicsEntity: physicsEntity);

            testEntity.ApplyTransformation();

            for (int i = 0; i < 10; i++)
            {
                MaterialEffect test = _assets.SilverMaterial.Clone();
                test.Roughness = i / 9.0f + 0.1f;
                test.Metallic = 1;
                _physicsSpace.Add(physicsEntity = new Sphere(position: BEPUutilities.Vector3.Zero, radius: 5, mass: 50));
                AddEntity(model: _assets.IsoSphere,
                    materialEffect: test,
                    position: new Vector3(30 + i * 10, 0, 10),
                    angleX: Math.PI / 2,
                    angleY: 0,
                    angleZ: 0,
                    scale: 5,
                    PhysicsEntity: physicsEntity);
            }

            ////////////////////////////////////////////////////////////////////////
            // Decals

            Decals.Add(new Decal(_assets.IconDecal, new Vector3(-6, 22, 15), 0, -Math.PI / 2, 0, Vector3.One * 10));

            ////////////////////////////////////////////////////////////////////////
            // Dynamic lights

            AddPointLight(position: new Vector3(-61, 0, 107),
                        radius: 150,
                        color: new Color(104, 163, 223),
                        intensity: 20,
                        castShadows: false,
                        shadowResolution: 1024,
                        staticShadow: false,
                        isVolumetric: true);

            AddPointLight(position: new Vector3(15, 0, 107),
                        radius: 150,
                        color: new Color(104, 163, 223),
                        intensity: 30,
                        castShadows: false,
                        shadowResolution: 1024,
                        staticShadow: false,
                        isVolumetric: true);

            AddPointLight(position: new Vector3(66, 0, 40),
                radius: 120,
                color: new Color(255, 248, 232),
                intensity: 120,
                castShadows: true,
                shadowResolution: 1024,
                softShadowBlurAmount: 0,
                staticShadow: false,
                isVolumetric: false);

            //volumetric light!
            //AddPointLight(position: new Vector3(-4, 40, 66),
            //    radius: 80,
            //    color: Color.White,
            //    intensity: 50,
            //    castShadows: true,
            //    shadowResolution: 1024,
            //    staticShadow: false,
            //    isVolumetric: true,
            //    volumetricDensity: 3);


            // Spawn a lot of lights to test performance 

            //int sides = 4;
            //float distance = 20;
            //Vector3 startPosition = new Vector3(-30, 30, 1);

            ////amount of lights is sides*sides* sides*2

            //for (int x = 0; x < sides * 2; x++)
            //    for (int y = 0; y < sides; y++)
            //        for (int z = 0; z < sides; z++)
            //        {
            //            Vector3 position = new Vector3(x, -y, z) * distance + startPosition;
            //            AddPointLight(position, distance, FastRand.NextColor(), 50, false, false, 0.9f);
            //        }


            //AddDirectionalLight(direction: new Vector3(0.2f, 0.2f, -1),
            //    intensity: 100,
            //    color: Color.White,
            //    position: Vector3.UnitZ * 2,
            //    drawShadows: true,
            //    shadowWorldSize: 450,
            //    shadowDepth: 180,
            //    shadowResolution: 1024,
            //    shadowFilteringFiltering: DirectionalLightSource.ShadowFilteringTypes.SoftPCF3x,
            //    screenspaceShadowBlur: false);
        }



        /// <summary>
        /// Main logic update function. Is called once per frame. Use this for all program logic and user inputs
        /// </summary>
        /// <param name="gameTime">Can use this to compute the delta between frames</param>
        /// <param name="isActive">The window status. If this is not the active window we shouldn't do anything</param>
        public void Update(GameTime gameTime, bool isActive)
        {
            if (!isActive) return;

            //Upd
            Input.Update(gameTime, Camera);

            //VolumeTexture.RotationMatrix = testEntity.WorldTransform.InverseWorld;
            //VolumeTexture.Scale = testEntity.WorldTransform.Scale;
            
            //Make the lights move up and down
            //for (var i = 2; i < PointLights.Count; i++)
            //{
            //    PointLight point = PointLights[i];
            //    point.Position = new Vector3(point.Position.X, point.Position.Y, (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 0.8f + i) * 10 - 13));
            //}

            //KeyInputs for specific tasks


            //If we are currently typing stuff into the console we should ignore the following keyboard inputs
            if (DebugScreen.ConsoleOpen) return;

            //Starts the "editor mode" where we can manipulate objects
            if (Input.WasKeyPressed(Keys.Space))
            {
                GameSettings.e_enableeditor = !GameSettings.e_enableeditor;
            }

            
            
            //Spawns a new light on the ground
            if (Input.keyboardState.IsKeyDown(Keys.L))
            {
                AddPointLight(position: new Vector3(FastRand.NextSingle() * 250 - 125, FastRand.NextSingle() * 50 - 25, FastRand.NextSingle() * 30 - 19), 
                    radius: 20, 
                    color: FastRand.NextColor(), 
                    intensity: 40, 
                    castShadows: false,
                    isVolumetric: true);
            }
            
            //Switch which rendertargets we show
            if (Input.WasKeyPressed(Keys.F1))
            {
                _renderModeCycle++;
                if (_renderModeCycle > Enum.GetNames(typeof(Renderer.Renderer.RenderModes)).Length - 1) _renderModeCycle = 0;

                GameSettings.g_rendermode = (Renderer.Renderer.RenderModes) _renderModeCycle;
            }
        }
        

        //Load content
        public void Load(ContentManager content)
        {
            //...
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  HELPER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Spawn a directional light (omni light). This light covers everything and comes from a single point from infinte distance. 
        /// Good for something like a sun
        /// </summary>
        /// <param name="direction">The direction the light is facing in world space coordinates</param>
        /// <param name="intensity"></param>
        /// <param name="color"></param>
        /// <param name="position">The position is only relevant if drawing shadows</param>
        /// <param name="drawShadows"></param>
        /// <param name="shadowWorldSize">WorldSize is the width/height of the view projection for shadow mapping</param>
        /// <param name="shadowDepth">FarClip for shadow mapping</param>
        /// <param name="shadowResolution"></param>
        /// <param name="shadowFilteringFiltering"></param>
        /// <param name="screenspaceShadowBlur"></param>
        /// <param name="staticshadows">These shadows will not be updated once they are created, moving objects will be shadowed incorrectly</param>
        /// <returns></returns>
        private DirectionalLight AddDirectionalLight(Vector3 direction, int intensity, Color color, Vector3 position = default(Vector3), bool drawShadows = false, float shadowWorldSize = 100, float shadowDepth = 100, int shadowResolution = 512, DirectionalLight.ShadowFilteringTypes shadowFilteringFiltering = DirectionalLight.ShadowFilteringTypes.Poisson, bool screenspaceShadowBlur = false, bool staticshadows = false )
        {
            DirectionalLight light = new DirectionalLight(color: color, 
                intensity: intensity, 
                direction: direction, 
                position: position, 
                castShadows: drawShadows, 
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
        /// <param name="isVolumetric">does it have a fog volume?</param>
        /// <param name="volumetricDensity">How dense is the volume?</param>
        /// <param name="shadowResolution">shadow map resolution per face. Optional</param>
        /// <param name="staticShadow">if set to true the shadows will not update at all. Dynamic shadows in contrast update only when needed.</param>
        /// <returns></returns>
        private PointLight AddPointLight(Vector3 position, float radius, Color color, float intensity, bool castShadows, bool isVolumetric = false, float volumetricDensity = 1, int shadowResolution = 256, int softShadowBlurAmount = 0, bool staticShadow = false)
        {
            PointLight light = new PointLight(position, radius, color, intensity, castShadows, isVolumetric, shadowResolution, softShadowBlurAmount, staticShadow, volumetricDensity);
            PointLights.Add(light);
            return light;
        }


        /// <summary>
        /// Create a basic rendered model without custom material, use materialEffect: for materials instead
        /// The material used is the one found in the imported model file, usually that means diffuse texture only
        /// </summary>
        /// <param name="model"></param>
        /// <param name="position"></param>
        /// <param name="angleX"></param>
        /// <param name="angleY"></param>
        /// <param name="angleZ"></param>
        /// <param name="scale"></param>
        /// <param name="PhysicsEntity">attached physical object</param>
        /// <param name="hasStaticPhysics">if "true" a static mesh will be computed based on the model mesh. Other physical objects can collide with the entity</param>
        /// <returns>returns the basicEntity we created</returns>
        private BasicEntity AddEntity(ModelDefinition model, Vector3 position, double angleX, double angleY, double angleZ, float scale, Entity PhysicsEntity = null, bool hasStaticPhysics = false)
        {
            BasicEntity entity = new BasicEntity(model,
                null, 
                position: position, 
                angleZ: angleZ, 
                angleX: angleX, 
                angleY: angleY, 
                scale: Vector3.One * scale,
                library: MeshMaterialLibrary,
                physicsObject: PhysicsEntity);
            BasicEntities.Add(entity);

            if (hasStaticPhysics) AddStaticPhysics(entity);

            return entity;
        }

        /// <summary>
        /// Create a basic rendered model with custom material
        /// </summary>
        /// <param name="model"></param>
        /// <param name="materialEffect">custom material</param>
        /// <param name="position"></param>
        /// <param name="angleX"></param>
        /// <param name="angleY"></param>
        /// <param name="angleZ"></param>
        /// <param name="scale"></param>
        /// <param name="PhysicsEntity">attached physical object</param>
        /// <param name="hasStaticPhysics">if "true" a static mesh will be computed based on the model mesh. Other physical objects can collide with the entity</param>
        /// <returns>returns the basicEntity we created</returns>
        private BasicEntity AddEntity(ModelDefinition model, MaterialEffect materialEffect, Vector3 position, double angleX, double angleY, double angleZ, float scale, Entity PhysicsEntity = null, bool hasStaticPhysics = false )
        {
            BasicEntity entity = new BasicEntity(model,
                materialEffect,
                position: position,
                angleZ: angleZ,
                angleX: angleX,
                angleY: angleY,
                scale: Vector3.One * scale,
                library: MeshMaterialLibrary,
                physicsObject: PhysicsEntity);
            BasicEntities.Add(entity);

            if(hasStaticPhysics) AddStaticPhysics(entity);

            return entity;
        }

        /// <summary>
        /// Create a static physics mesh from a model and scale.
        /// </summary>
        /// <param name="entity"></param>
        private void AddStaticPhysics(BasicEntity entity)
        {
            BEPUutilities.Vector3[] vertices;
            int[] indices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(entity.Model, out vertices, out indices);
            var mesh = new StaticMesh(vertices, indices, 
                new AffineTransform(
                    new BEPUutilities.Vector3(entity.Scale.X, entity.Scale.Y, entity.Scale.Z), 
                Quaternion.CreateFromRotationMatrix(MathConverter.Convert(entity.RotationMatrix)), 
                MathConverter.Convert(entity.Position)));

            entity.StaticPhysicsObject = mesh;
            _physicsSpace.Add(mesh);
        }

    }
}
