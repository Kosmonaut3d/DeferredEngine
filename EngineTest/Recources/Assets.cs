using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
        public class Assets
        {

            List<Texture2D> SponzaTextures = new List<Texture2D>();
            private Texture2D background_ddn;
            private Texture2D chain_texture_ddn;
            private Texture2D chain_texture_mask;
            public Texture2D lion_ddn;
            private Texture2D lion2_ddn;
            private Texture2D spnza_bricks_a_ddn;
            private Texture2D spnza_bricks_a_spec;
            private Texture2D sponza_arch_ddn;
            private Texture2D sponza_arch_spec;
            private Texture2D sponza_ceiling_a_spec;
            private Texture2D sponza_column_a_ddn;
            private Texture2D sponza_column_a_spec;
            private Texture2D sponza_column_b_spec;
            private Texture2D sponza_column_b_ddn;
            private Texture2D sponza_column_c_spec;
            private Texture2D sponza_column_c_ddn;
            private Texture2D sponza_details_spec;
            private Texture2D sponza_flagpole_spec;
            private Texture2D sponza_floor_a_spec;
            private Texture2D sponza_floor_a_ddn;
            private Texture2D sponza_thorn_ddn;
            private Texture2D sponza_thorn_mask;
            private Texture2D sponza_thorn_spec;
            private Texture2D vase_ddn;
            private Texture2D vase_plant_mask;
            private Texture2D vase_plant_spec;
            private Texture2D vase_round_ddn;
            private Texture2D vase_round_spec;

            private Texture2D sponza_curtain_metallic;

            public static Texture2D BaseTex;

            public Texture2D Icon_Light;

            public Model SkullModel { get; set; }

            public Model HelmetModel { get; set; }

            public Model DragonUvSmoothModel { get; set; }

            public Model SponzaModel { get; set; }

            public Model Plane;

            public Model TestTubes { get; set; }

            public Model EditorArrow;
            public Model EditorArrowRound;

            public Model Sphere;
            public MaterialEffect baseMaterial;
            public MaterialEffect goldMaterial;
            public MaterialEffect emissiveMaterial;
            public MaterialEffect emissiveMaterial2;
            public MaterialEffect silverMaterial;
            public MaterialEffect hologramMaterial;

            public void Load(ContentManager content, GraphicsDevice graphicsDevice)
            {
                BaseTex = new Texture2D(graphicsDevice, 1, 1);
                BaseTex.SetData(new Color[] { Color.White });

                DragonUvSmoothModel = content.Load<Model>("dragon_uv_smooth");

                SponzaModel = content.Load<Model>("Sponza/Sponza");
                HelmetModel = content.Load<Model>("daft_helmets");
                SkullModel = content.Load<Model>("skull");

                SponzaTextures.Add(background_ddn = content.Load<Texture2D>("Sponza/textures/background_ddn"));
                SponzaTextures.Add(chain_texture_ddn = content.Load<Texture2D>("Sponza/textures/chain_texture_ddn"));
                SponzaTextures.Add(chain_texture_mask = content.Load<Texture2D>("Sponza/textures/chain_texture_mask"));
                SponzaTextures.Add(lion_ddn = content.Load<Texture2D>("Sponza/textures/lion_ddn"));
                SponzaTextures.Add(lion2_ddn = content.Load<Texture2D>("Sponza/textures/lion2_ddn"));
                SponzaTextures.Add(spnza_bricks_a_ddn = content.Load<Texture2D>("Sponza/textures/spnza_bricks_a_ddn"));
                SponzaTextures.Add(spnza_bricks_a_spec = content.Load<Texture2D>("Sponza/textures/spnza_bricks_a_spec"));
                SponzaTextures.Add(sponza_arch_ddn = content.Load<Texture2D>("Sponza/textures/sponza_arch_ddn"));
                SponzaTextures.Add(sponza_arch_spec = content.Load<Texture2D>("Sponza/textures/sponza_arch_spec"));
                SponzaTextures.Add(sponza_ceiling_a_spec = content.Load<Texture2D>("Sponza/textures/sponza_ceiling_a_spec"));
                SponzaTextures.Add(sponza_column_a_ddn = content.Load<Texture2D>("Sponza/textures/sponza_column_a_ddn"));
                SponzaTextures.Add(sponza_column_a_spec = content.Load<Texture2D>("Sponza/textures/sponza_column_a_spec"));
                SponzaTextures.Add(sponza_column_b_spec = content.Load<Texture2D>("Sponza/textures/sponza_column_b_spec"));
                SponzaTextures.Add(sponza_column_b_ddn = content.Load<Texture2D>("Sponza/textures/sponza_column_b_ddn"));
                SponzaTextures.Add(sponza_column_c_spec = content.Load<Texture2D>("Sponza/textures/sponza_column_c_spec"));
                SponzaTextures.Add(sponza_column_c_ddn = content.Load<Texture2D>("Sponza/textures/sponza_column_c_ddn"));

                SponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_fabric_spec"));
                SponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_curtain_green_spec"));
                SponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_curtain_blue_spec"));
                SponzaTextures.Add(content.Load<Texture2D>("Sponza/textures/sponza_curtain_spec"));

                SponzaTextures.Add(sponza_details_spec = content.Load<Texture2D>("Sponza/textures/sponza_details_spec"));
                SponzaTextures.Add(sponza_flagpole_spec = content.Load<Texture2D>("Sponza/textures/sponza_flagpole_spec"));

                SponzaTextures.Add(sponza_floor_a_spec = content.Load<Texture2D>("Sponza/textures/sponza_floor_a_spec"));
                //SponzaTextures.Add(sponza_floor_a_ddn = content.Load<Texture2D>("Sponza/textures/sponza_floor_a_ddn"));

                SponzaTextures.Add(sponza_thorn_ddn = content.Load<Texture2D>("Sponza/textures/sponza_thorn_ddn"));
                SponzaTextures.Add(sponza_thorn_mask = content.Load<Texture2D>("Sponza/textures/sponza_thorn_mask"));
                SponzaTextures.Add(sponza_thorn_spec = content.Load<Texture2D>("Sponza/textures/sponza_thorn_spec"));
                SponzaTextures.Add(vase_ddn = content.Load<Texture2D>("Sponza/textures/vase_ddn"));
                SponzaTextures.Add(vase_plant_mask = content.Load<Texture2D>("Sponza/textures/vase_plant_mask"));
                SponzaTextures.Add(vase_plant_spec = content.Load<Texture2D>("Sponza/textures/vase_plant_spec"));
                SponzaTextures.Add(vase_round_ddn = content.Load<Texture2D>("Sponza/textures/vase_round_ddn"));
                SponzaTextures.Add(vase_round_spec = content.Load<Texture2D>("Sponza/textures/vase_round_spec"));

                sponza_curtain_metallic = content.Load<Texture2D>("Sponza/textures/sponza_curtain_metallic");

                Sphere = content.Load<Model>("sphere");

                Icon_Light = content.Load<Texture2D>("Art/Editor/icon_light");

                ProcessSponza();

                DragonUvSmoothModel.Meshes[0].MeshParts[0].Effect =
                    new MaterialEffect(DragonUvSmoothModel.Meshes[0].MeshParts[0].Effect)
                    {
                        DiffuseColor = Color.MonoGameOrange.ToVector3()
                    };

                TestTubes = content.Load<Model>("Art/test/tubes");

                ProcessHelmets();

                Plane = content.Load<Model>("Art/Plane");

                EditorArrow = content.Load<Model>("Art/Editor/Arrow");
                EditorArrowRound = content.Load<Model>("Art/Editor/ArrowRound");

                baseMaterial = CreateMaterial(Color.Red, 0.3f, 0);

                hologramMaterial = CreateMaterial(Color.White, 0.2f, 1, null, null, null, null, null, MaterialEffect.MaterialTypes.Hologram, 1);

                emissiveMaterial = CreateMaterial(Color.White, 0.2f, 1, null, null, null, null, null, MaterialEffect.MaterialTypes.Emissive, 1.5f);

                emissiveMaterial2 = CreateMaterial(Color.LimeGreen, 0.2f, 1, null, null, null, null, null, MaterialEffect.MaterialTypes.Emissive, 0.8f);

                goldMaterial = CreateMaterial(Color.Gold, 0.2f, 1);

                silverMaterial = CreateMaterial(Color.Silver, 0.05f, 1);
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
            private MaterialEffect CreateMaterial(Color color, float roughness, float metallic, Texture2D albedoMap = null, Texture2D normalMap = null, Texture2D roughnessMap = null, Texture2D metallicMap = null, Texture2D mask = null, MaterialEffect.MaterialTypes type = 0, float emissiveStrength = 0)
            {
                MaterialEffect mat = new MaterialEffect(Shaders.ClearGBufferEffect);
                mat.Initialize(color, roughness, metallic, albedoMap, normalMap, roughnessMap, metallicMap, mask, type, emissiveStrength);
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
                            matEffect.DiffuseColor = new Color(0, 0.49f,  0.95f).ToVector3();
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
                            matEffect.DiffuseColor = new Color(255,255,155).ToVector3()*0.5f;
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


                            if (compare.Contains("chain"))
                            {
                                matEffect.Roughness = 0.5f;
                                matEffect.Metallic = 1f;
                            }

                            if (compare.Contains("curtain"))
                            {
                                matEffect.MetallicMap = sponza_curtain_metallic;
                            }


                            if (compare.Contains("lion"))
                            {
                                matEffect.Metallic = 0.9f;
                            }

                            if (compare.Contains("_diff"))
                            {
                                compare = compare.Replace("_diff", "");
                            }
                            
                            foreach(Texture2D tex2d in SponzaTextures)
                            {
                                if(tex2d.Name.Contains(compare))
                                {
                                    //We got a match!

                                    string ending = tex2d.Name.Replace(compare, "");

                                    ending = ending.Replace("Sponza/textures/", "");

                                    if(ending == "_spec")
                                    {
                                        matEffect.RoughnessMap = tex2d;
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
