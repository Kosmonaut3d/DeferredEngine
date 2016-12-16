using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
    public class Assets
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Default Meshes + Editor

        public Model EditorArrow;
        public Model EditorArrowRound;

        public Model Sphere;
        public ModelMeshPart SphereMeshPart;
        public Model IsoSphere;

        public Model Plane;

        public Model Cube;

        public Texture2D IconLight;

        //Default Materials

        public MaterialEffect BaseMaterial;
        public MaterialEffect GoldMaterial;
        public MaterialEffect EmissiveMaterial;
        public MaterialEffect EmissiveMaterial2;
        public MaterialEffect EmissiveMaterial3;
        public MaterialEffect EmissiveMaterial4;
        public MaterialEffect SilverMaterial;
        public MaterialEffect HologramMaterial;
        public MaterialEffect MetalRough03Material;
        public MaterialEffect MetalRough01Material;

        //Shader stuff

        public Texture2D NoiseMap;

        public static Texture2D BaseTex;

        //Meshes and Materials

        public Model Trabant;
        public MaterialEffect TrabantBigParts;

        public Model SponzaModel;
        readonly List<Texture2D> _sponzaTextures = new List<Texture2D>();
        private Texture2D sponza_fabric_metallic;
        private Texture2D sponza_fabric_spec;
        private Texture2D sponza_curtain_metallic;

        public Model SkullModel;

        public Model HelmetModel;

        public Model StanfordDragon;
        
        public MaterialEffect RockMaterial;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            //Default Meshes + Editor
            EditorArrow = content.Load<Model>("Art/Editor/Arrow");
            EditorArrowRound = content.Load<Model>("Art/Editor/ArrowRound");

            IsoSphere = content.Load<Model>("Art/default/isosphere");
            Sphere = content.Load<Model>("Art/default/sphere");
            SphereMeshPart = Sphere.Meshes[0].MeshParts[0];

            Plane = content.Load<Model>("Art/Plane");

            Cube = content.Load<Model>("Art/test/cube");

            IconLight = content.Load<Texture2D>("Art/Editor/icon_light");

            //Default Materials

            BaseMaterial = CreateMaterial(Color.Red, 0.3f, 0);

            MetalRough03Material = CreateMaterial(Color.Silver, 0.3f, 1);
            MetalRough01Material = CreateMaterial(Color.Silver, 0.1f, 1);

            HologramMaterial = CreateMaterial(Color.White, 0.2f, 1, null, null, null, null, null, null, MaterialEffect.MaterialTypes.Hologram, 1);

            EmissiveMaterial = CreateMaterial(Color.White, 0.2f, 1, null, null, null, null, null, null, MaterialEffect.MaterialTypes.Emissive, 1.5f);

            EmissiveMaterial2 = CreateMaterial(Color.MonoGameOrange, 0.2f, 1, null, null, null, null, null, null, MaterialEffect.MaterialTypes.Emissive, 4.8f);
            EmissiveMaterial3 = CreateMaterial(Color.Violet, 0.2f, 1, null, null, null, null, null, null, MaterialEffect.MaterialTypes.Emissive, 3.8f);
            EmissiveMaterial4 = CreateMaterial(Color.LimeGreen, 0.2f, 1, null, null, null, null, null, null, MaterialEffect.MaterialTypes.Emissive, 2.8f);

            GoldMaterial = CreateMaterial(Color.Gold, 0.2f, 1);

            SilverMaterial = CreateMaterial(Color.Silver, 0.05f, 1);

            //Shader stuff

            BaseTex = new Texture2D(graphicsDevice, 1, 1);
            BaseTex.SetData(new Color[] { Color.White });

            NoiseMap = content.Load<Texture2D>("Shaders/noise_blur");

            //Meshes and Materials

            Trabant = content.Load<Model>("Art/test/source/trabant_realtime_v3");

            TrabantBigParts = CreateMaterial(Color.White, roughness: 1, metallic: 0,
                albedoMap: content.Load<Texture2D>("Art/test/textures/big_parts_col"),
                normalMap: content.Load<Texture2D>("Art/test/textures/big_parts_nor"),
                roughnessMap: content.Load<Texture2D>("Art/test/textures/big_parts_rough"));

            MaterialEffect TrabantWindow = CreateMaterial(Color.White, roughness: 0.04f, metallic: 0.5f);

            MaterialEffect TrabantSmallParts = CreateMaterial(Color.White, roughness: 1, metallic: 0,
                albedoMap: content.Load<Texture2D>("Art/test/textures/small_parts_col"),
                normalMap: null,
                roughnessMap: content.Load<Texture2D>("Art/test/textures/small_parts_rough"));

            Trabant.Meshes[0].MeshParts[0].Effect = TrabantWindow;
            Trabant.Meshes[1].MeshParts[0].Effect = TrabantBigParts;
            Trabant.Meshes[3].MeshParts[0].Effect = TrabantSmallParts;

            //

            StanfordDragon = content.Load<Model>("Art/default/dragon_uv_smooth");
            HelmetModel = content.Load<Model>("Art/default/daft_helmets");
            SkullModel = content.Load<Model>("Art/default/skull");

            //

            SponzaModel = content.Load<Model>("Sponza/Sponza");
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
            MaterialEffect mat = new MaterialEffect(Shaders.ClearGBufferEffect);
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
            foreach (ModelMesh mesh in SponzaModel.Meshes)
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
    }

}
