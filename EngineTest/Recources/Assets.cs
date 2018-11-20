using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{
    public class Assets : IDisposable
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Default Meshes + Editor

        public Model EditorArrow;
        public Model EditorArrowRound;

        public Model Sphere;
        public ModelMeshPart SphereMeshPart;
        public ModelDefinition IsoSphere;

        public ModelDefinition Plane;

        public ModelDefinition Cube;

        //https://sketchfab.com/models/95c4008c4c764c078f679d4c320e7b18
        public ModelDefinition Tiger;

        public ModelDefinition HumanModel;

        public Texture2D IconLight;
        public Texture2D IconEnvmap;
        public Texture2D IconDecal;

        //Default Materials

        public MaterialEffect MaterialSSS_Red;
        public MaterialEffect MaterialSSS_Green;
        public MaterialEffect MaterialSSS_Cyan;
        public MaterialEffect BaseMaterial;
        public MaterialEffect BaseMaterialGray;
        public MaterialEffect GoldMaterial;
        public MaterialEffect EmissiveMaterial;
        public MaterialEffect EmissiveMaterial2;
        public MaterialEffect EmissiveMaterial3;
        public MaterialEffect EmissiveMaterial4;
        public MaterialEffect SilverMaterial;
        public MaterialEffect HologramMaterial;
        public MaterialEffect MetalRough03Material;
        public MaterialEffect AlphaBlendRim;
        public MaterialEffect MirrorMaterial;

        //Shader stuff

        public Texture2D NoiseMap;

        public static Texture2D BaseTex;

        //Meshes and Materials

        //public Model Trabant;
        //public MaterialEffect TrabantBigParts;

        public ModelDefinition SponzaModel;
        readonly List<Texture2D> _sponzaTextures = new List<Texture2D>();
        private Texture2D sponza_fabric_metallic;
        private Texture2D sponza_fabric_spec;
        private Texture2D sponza_curtain_metallic;

        public Model SkullModel;

        public Model HelmetModel;

        public ModelDefinition StanfordDragon;
        public ModelDefinition StanfordDragonLowpoly;

        public MaterialEffect RockMaterial;


        public SpriteFont DefaultFont;
        public SpriteFont MonospaceFont;
        
        public MaterialEffect DragonLowPolyMaterial;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            //Default Meshes + Editor
            EditorArrow = content.Load<Model>("Art/Editor/Arrow");
            EditorArrowRound = content.Load<Model>("Art/Editor/ArrowRound");

            IsoSphere = new ModelDefinition(content, "Art/default/isosphere", graphicsDevice, true, new Vector3(50, 50, 50));
            
            Sphere = content.Load<Model>("Art/default/sphere");
            SphereMeshPart = Sphere.Meshes[0].MeshParts[0];

            Plane = new ModelDefinition(content, "Art/Plane", graphicsDevice);

            Cube = new ModelDefinition(content, "Art/test/cube", graphicsDevice, true, new Vector3(50, 50, 50));

            Tiger = new ModelDefinition(content, "Art/Tiger/Tiger", graphicsDevice, true, new Vector3(50,50,50));
            HumanModel = new ModelDefinition(content, "Art/Human/human", graphicsDevice, true, new Vector3(50, 50, 50));

            IconDecal = content.Load<Texture2D>("Art/Editor/icon_decal");
            IconLight = content.Load<Texture2D>("Art/Editor/icon_light");
            IconEnvmap = content.Load<Texture2D>("Art/Editor/icon_envmap");
            //Default Materials

            BaseMaterial = CreateMaterial(Color.Red, 0.5f, 0, type: MaterialEffect.MaterialTypes.Basic);

            MaterialSSS_Red = CreateMaterial(Color.Red, 0.5f, 0, type: MaterialEffect.MaterialTypes.SubsurfaceScattering);
            MaterialSSS_Green = CreateMaterial(Color.Lime, 0.5f, 0, type: MaterialEffect.MaterialTypes.Basic);
            MaterialSSS_Cyan = CreateMaterial(Color.Cyan, 0.5f, 0, type: MaterialEffect.MaterialTypes.SubsurfaceScattering);

            BaseMaterialGray = CreateMaterial(Color.LightGray, 0.8f, 0, type: MaterialEffect.MaterialTypes.Basic);

            MetalRough03Material = CreateMaterial(Color.Silver, 0.2f, 1);
            AlphaBlendRim = CreateMaterial(Color.Silver, 0.05f, 1, type: MaterialEffect.MaterialTypes.ForwardShaded);
            MirrorMaterial = CreateMaterial(Color.White, 0.05f, 1);

            HologramMaterial = CreateMaterial(Color.White, 0.2f, 1, null, null, null, null, null, null, MaterialEffect.MaterialTypes.Hologram, 1);

            EmissiveMaterial = CreateMaterial(Color.White, 0.2f, 1, null, null, null, null, null, null, MaterialEffect.MaterialTypes.Emissive, 1.5f);

            EmissiveMaterial2 = CreateMaterial(Color.MonoGameOrange, 0.2f, 1, null, null, null, null, null, null, MaterialEffect.MaterialTypes.Emissive, 1.8f);
            EmissiveMaterial3 = CreateMaterial(Color.Violet, 0.2f, 1, null, null, null, null, null, null, MaterialEffect.MaterialTypes.Emissive, 1.8f);
            EmissiveMaterial4 = CreateMaterial(Color.LimeGreen, 0.2f, 1, null, null, null, null, null, null, MaterialEffect.MaterialTypes.Emissive, 1.8f);

            GoldMaterial = CreateMaterial(Color.Gold, 0.2f, 1);

            SilverMaterial = CreateMaterial(Color.Silver, 0.05f, 1);

            //Shader stuff

            BaseTex = new Texture2D(graphicsDevice, 1, 1);
            BaseTex.SetData(new Color[] { Color.White });

            NoiseMap = content.Load<Texture2D>("Shaders/noise_blur");
            //Meshes and Materials

            //Trabant = content.Load<Model>("Art/test/source/trabant_realtime_v3");

            //TrabantBigParts = CreateMaterial(Color.White, roughness: 1, metallic: 0,
            //    albedoMap: content.Load<Texture2D>("Art/test/textures/big_parts_col"),
            //    normalMap: content.Load<Texture2D>("Art/test/textures/big_parts_nor"),
            //    roughnessMap: content.Load<Texture2D>("Art/test/textures/big_parts_rough"));

            //MaterialEffect TrabantWindow = CreateMaterial(Color.White, roughness: 0.04f, metallic: 0.5f);

            //MaterialEffect TrabantSmallParts = CreateMaterial(Color.White, roughness: 1, metallic: 0,
            //    albedoMap: content.Load<Texture2D>("Art/test/textures/small_parts_col"),
            //    normalMap: null,
            //    roughnessMap: content.Load<Texture2D>("Art/test/textures/small_parts_rough"));

            //Trabant.Meshes[0].MeshParts[0].Effect = TrabantWindow;
            //Trabant.Meshes[1].MeshParts[0].Effect = TrabantBigParts;
            //Trabant.Meshes[3].MeshParts[0].Effect = TrabantSmallParts;

            //

            StanfordDragon = new ModelDefinition(content, "Art/default/dragon_uv_smooth", graphicsDevice, false, new Vector3(70, 70, 70)); 
            StanfordDragonLowpoly = new ModelDefinition(content, "Art/default/dragon_lowpoly", graphicsDevice, true, new Vector3(60, 60,60));

            DragonLowPolyMaterial = CreateMaterial(Color.Red, 0.5f, 0, type: MaterialEffect.MaterialTypes.Basic, normalMap: content.Load<Texture2D>("Art/default/dragon_normal"));

            HelmetModel = content.Load<Model>("Art/default/daft_helmets");
            SkullModel = content.Load<Model>("Art/default/skull");

            //

            SponzaModel = new ModelDefinition(content, "Sponza/Sponza", graphicsDevice, false);
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/background_ddn"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/chain_texture_ddn"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/chain_texture_mask"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/lion_ddn"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/lion2_ddn"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/spnza_bricks_a_ddn"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/spnza_bricks_a_spec"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/sponza_arch_ddn"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/sponza_arch_spec"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/sponza_ceiling_a_spec"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/sponza_column_a_ddn"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/sponza_column_a_spec"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/sponza_column_b_spec"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/sponza_column_b_ddn"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/sponza_column_c_spec"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/sponza_column_c_ddn"));
            _sponzaTextures.Add(sponza_fabric_spec = content.Load<Texture2D>("Sponza/textures/sponza_fabric_spec"));
            _sponzaTextures.Add(sponza_fabric_metallic = content.Load<Texture2D>("Sponza/textures/sponza_fabric_metallic"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_curtain_green_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_curtain_blue_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_curtain_spec"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/sponza_details_spec"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/sponza_flagpole_spec"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/sponza_thorn_ddn"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/sponza_thorn_mask"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/sponza_thorn_spec"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/vase_ddn"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/vase_plant_mask"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/vase_plant_spec"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/vase_round_ddn"));
            _sponzaTextures.Add( content.Load<Texture2D>("Sponza/textures/vase_round_spec"));

            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_floor_a_spec"));
            _sponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_floor_a_ddn"));
            
            sponza_curtain_metallic = content.Load<Texture2D>("Sponza/textures/sponza_curtain_metallic");

            ProcessSponza();
            
            ProcessHelmets();

            RockMaterial = CreateMaterial(Color.White, roughness: 1, metallic: 0,
                albedoMap: content.Load<Texture2D>("Art/test/squarebricks-diffuse"),
                normalMap: content.Load<Texture2D>("Art/test/squarebricks-normal"),
                roughnessMap: null,
                metallicMap: null,
                mask: null,
                displacementMap: content.Load<Texture2D>("Art/test/squarebricks-depth")
            );

            //Fonts

            DefaultFont = content.Load<SpriteFont>("Fonts/defaultFont");
            MonospaceFont = content.Load<SpriteFont>("Fonts/monospace");

        }

        /// <summary>
        /// Create custom materials, you can add certain maps like Albedo, normal, etc. if you like.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="roughness"></param>
        /// <param name="metallic"></param>
        /// <param name="albedoMap"></param>
        /// <param name="normalMap"></param>
        /// <param name="roughnessMap"></param>
        /// <param name="metallicMap"></param>
        /// <param name="mask"></param>
        /// <param name="type">2: hologram, 3:emissive</param>
        /// <param name="emissiveStrength"></param>
        /// <returns></returns>
        private MaterialEffect CreateMaterial(Color color, float roughness, float metallic, Texture2D albedoMap = null, Texture2D normalMap = null, Texture2D roughnessMap = null, Texture2D metallicMap = null, Texture2D mask = null, Texture2D displacementMap = null, MaterialEffect.MaterialTypes type = 0, float emissiveStrength = 0)
        {
            MaterialEffect mat = new MaterialEffect(Shaders.DeferredClear);
            mat.Initialize(color, roughness, metallic, albedoMap, normalMap, roughnessMap, metallicMap, mask, displacementMap, type, emissiveStrength);
            return mat;
        }

        /// <summary>
        /// The helmets have many submaterials and I want specific values for each one of them!
        /// </summary>
        private void ProcessHelmets()
        {
            for (int i = 0; i < HelmetModel.Meshes.Count; i++)
            {
                ModelMesh mesh = HelmetModel.Meshes[i];
                for (int index = 0; index < mesh.MeshParts.Count; index++)
                {
                    ModelMeshPart meshPart = mesh.MeshParts[index];
                    MaterialEffect matEffect = new MaterialEffect(meshPart.Effect);

                    matEffect.DiffuseColor = Color.Gray.ToVector3();

                    if (mesh.Name == "Helmet1_Interior")
                    {
                        matEffect.DiffuseColor = Color.White.ToVector3();
                    }

                    if (i == 5)
                    {
                        matEffect.DiffuseColor = new Color(0, 0.49f, 0.95f).ToVector3();
                        matEffect.Type = MaterialEffect.MaterialTypes.Hologram;
                    }

                    if (i == 0)
                    {
                        matEffect.DiffuseColor = Color.Black.ToVector3();
                        matEffect.Roughness = 0.1f;
                        matEffect.Type = MaterialEffect.MaterialTypes.ProjectHologram;
                    }

                    if (i == 1)
                    {
                        matEffect.DiffuseColor = new Color(0, 0.49f, 0.95f).ToVector3();
                    }

                    if (i == 2)
                    {
                        matEffect.DiffuseColor = Color.Silver.ToVector3();
                        matEffect.Metallic = 1;
                        matEffect.Roughness = 0.1f;
                    }

                    //Helmet color - should be gold!
                    if (i == 4)
                    {
                        matEffect.DiffuseColor = new Color(255, 255, 155).ToVector3() * 0.5f;
                        matEffect.Roughness = 0.3f;
                        matEffect.Metallic = 0.8f;
                    }

                    if (i == 13)
                    {
                        matEffect.DiffuseColor = Color.Black.ToVector3();
                        matEffect.Roughness = 0.05f;
                        matEffect.Type = MaterialEffect.MaterialTypes.ProjectHologram;
                    }

                    meshPart.Effect = matEffect;
                }
            }
        }
        
        private Model ProcessModel(Model model)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    MaterialEffect matEffect = new MaterialEffect(meshPart.Effect);

                    if (!(meshPart.Effect is BasicEffect))
                    {
                        throw new Exception("Can only process models with basic effect");
                    }

                    BasicEffect oEffect = meshPart.Effect as BasicEffect;

                    if (oEffect.TextureEnabled)
                        matEffect.AlbedoMap = oEffect.Texture;

                    matEffect.DiffuseColor = oEffect.DiffuseColor;

                    meshPart.Effect = matEffect;
                }
            }

            return model;
        }

        //Assign specific materials to submeshes
        private void ProcessSponza()
        {
            foreach (ModelMesh mesh in SponzaModel.Model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    MaterialEffect matEffect = new MaterialEffect(meshPart.Effect);

                    BasicEffect oEffect = meshPart.Effect as BasicEffect;

                    //I want to remove this mesh
                    if (mesh.Name == "g sponza_04")
                    {
                        //Put the boudning sphere into space?
                        mesh.BoundingSphere = new BoundingSphere(new Vector3(-100000, 0, 0), 0);

                        //Make it transparent
                        matEffect.IsTransparent = true;
                    }

                    matEffect.DiffuseColor = oEffect.DiffuseColor;

                    if (oEffect.TextureEnabled)
                    {
                        matEffect.AlbedoMap = oEffect.Texture;

                        string[] name = matEffect.AlbedoMap.Name.Split('\\');

                        string compare = name[2].Replace("_0", "");

                        if (compare.Contains("vase_round") || compare.Contains("vase_hanging"))
                        {
                            matEffect.Roughness = 0.1f;
                            matEffect.Metallic = 0.5f;
                        }

                        //Make the vases emissive!

                        //if (compare.Contains("vase_hanging"))
                        //{
                        //    matEffect.EmissiveStrength = 2;
                        //    matEffect.Type = MaterialEffect.MaterialTypes.Emissive;
                        //    matEffect.DiffuseColor = Color.Gold.ToVector3();

                        //    matEffect.AlbedoMap = null;
                        //    matEffect.HasDiffuse = false;
                        //}

                        //if (compare.Contains("floor"))
                        //{
                        //    matEffect.Roughness = 0.2f;
                        //    matEffect.Metallic = 1;
                        //    //matEffect.HasDiffuse = false;
                        //}


                        if (compare.Contains("chain"))
                        {
                            matEffect.Roughness = 0.5f;
                            matEffect.Metallic = 1f;
                        }

                        if (compare.Contains("curtain"))
                        {
                            matEffect.MetallicMap = sponza_curtain_metallic;
                        }

                        if (compare.Contains("sponza_fabric"))
                        {
                            matEffect.MetallicMap = sponza_fabric_metallic;
                            matEffect.RoughnessMap = sponza_fabric_spec;
                        }


                        if (compare.Contains("lion"))
                        {
                            matEffect.Metallic = 0.9f;
                        }

                        if (compare.Contains("_diff"))
                        {
                            compare = compare.Replace("_diff", "");
                        }

                        foreach (Texture2D tex2d in _sponzaTextures)
                        {
                            if (tex2d.Name.Contains(compare))
                            {
                                //We got a match!

                                string ending = tex2d.Name.Replace(compare, "");

                                ending = ending.Replace("Sponza/textures/", "");

                                if (ending == "_spec")
                                {
                                    matEffect.RoughnessMap = tex2d;
                                }

                                if (ending == "_metallic")
                                {
                                    matEffect.MetallicMap = tex2d;
                                }

                                if (ending == "_ddn")
                                {
                                    matEffect.NormalMap = tex2d;
                                }

                                if (ending == "_mask")
                                {
                                    matEffect.Mask = tex2d;
                                }

                            }
                        }


                    }
                    meshPart.Effect = matEffect;
                }


            }
        }

        public void Dispose()
        {
            IconLight?.Dispose();
            IconEnvmap?.Dispose();
            IconDecal?.Dispose();
            BaseMaterial?.Dispose();
            GoldMaterial?.Dispose();
            EmissiveMaterial?.Dispose();
            EmissiveMaterial2?.Dispose();
            EmissiveMaterial3?.Dispose();
            EmissiveMaterial4?.Dispose();
            SilverMaterial?.Dispose();
            HologramMaterial?.Dispose();
            MetalRough03Material?.Dispose();
            AlphaBlendRim?.Dispose();
            MirrorMaterial?.Dispose();
            NoiseMap?.Dispose();
            sponza_fabric_metallic?.Dispose();
            sponza_fabric_spec?.Dispose();
            sponza_curtain_metallic?.Dispose();
            RockMaterial?.Dispose();
        }
    }

}
