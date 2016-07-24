using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
        public class Art
        {

            List<Texture2D> SponzaTextures = new List<Texture2D>();
            private Texture2D background_ddn;
            private Texture2D chain_texture_ddn;
            private Texture2D chain_texture_mask;
            private Texture2D lion_ddn;
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
            private Texture2D sponza_thorn_ddn;
            private Texture2D sponza_thorn_mask;
            private Texture2D sponza_thorn_spec;
            private Texture2D vase_ddn;
            private Texture2D vase_plant_mask;
            private Texture2D vase_plant_spec;
            private Texture2D vase_round_ddn;
            private Texture2D vase_round_spec;


            public Model SkullModel { get; set; }

            public Model HelmetModel { get; set; }

            public Model DragonUvSmoothModel { get; set; }

            public Model SponzaModel { get; set; }

            public Model Sphere;

            public void Load(ContentManager content)
            {
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

                SponzaTextures.Add(sponza_details_spec = content.Load<Texture2D>("Sponza/textures/sponza_details_spec"));
                SponzaTextures.Add(sponza_flagpole_spec = content.Load<Texture2D>("Sponza/textures/sponza_flagpole_spec"));
                SponzaTextures.Add(sponza_floor_a_spec = content.Load<Texture2D>("Sponza/textures/sponza_floor_a_spec"));
                SponzaTextures.Add(sponza_thorn_ddn = content.Load<Texture2D>("Sponza/textures/sponza_thorn_ddn"));
                SponzaTextures.Add(sponza_thorn_mask = content.Load<Texture2D>("Sponza/textures/sponza_thorn_mask"));
                SponzaTextures.Add(sponza_thorn_spec = content.Load<Texture2D>("Sponza/textures/sponza_thorn_spec"));
                SponzaTextures.Add(vase_ddn = content.Load<Texture2D>("Sponza/textures/vase_ddn"));
                SponzaTextures.Add(vase_plant_mask = content.Load<Texture2D>("Sponza/textures/vase_plant_mask"));
                SponzaTextures.Add(vase_plant_spec = content.Load<Texture2D>("Sponza/textures/vase_plant_spec"));
                SponzaTextures.Add(vase_round_ddn = content.Load<Texture2D>("Sponza/textures/vase_round_ddn"));
                SponzaTextures.Add(vase_round_spec = content.Load<Texture2D>("Sponza/textures/vase_round_spec"));


                Sphere = content.Load<Model>("sphere");

                ProcessSponza();

                DragonUvSmoothModel.Meshes[0].MeshParts[0].Effect =
                    new MaterialEffect(DragonUvSmoothModel.Meshes[0].MeshParts[0].Effect)
                    {
                        DiffuseColor = Color.MonoGameOrange.ToVector3()
                    };

                ProcessHelmets();
            }

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
                            matEffect.MaterialType = 1;
                        }

                        if (i == 0)
                        {
                            matEffect.DiffuseColor = Color.Black.ToVector3();
                            matEffect.Roughness = 0.1f;
                            matEffect.MaterialType = 1;
                        }

                        if (i == 1)
                            matEffect.DiffuseColor = Color.Blue.ToVector3();

                        if (i == 2)
                        {
                            matEffect.DiffuseColor = Color.Black.ToVector3();
                            matEffect.Roughness = 0.1f;
                            matEffect.MaterialType = 2;
                        }

                        //Helmet color - should be gold!
                        if (i == 4)
                        {
                            matEffect.DiffuseColor = new Color(255,255,155).ToVector3()*0.5f;
                            matEffect.Roughness = 0.05f;
                            matEffect.MaterialType = 2;
                        }

                        if (i == 13)
                        {
                            matEffect.DiffuseColor = Color.Black.ToVector3();
                            matEffect.Roughness = 0.05f;
                            matEffect.MaterialType = 1;
                        }

                        meshPart.Effect = matEffect;
                    }
                }
            }

            private void ProcessSponza()
            {
                foreach (ModelMesh mesh in SponzaModel.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        MaterialEffect matEffect = new MaterialEffect(meshPart.Effect);

                        BasicEffect oEffect = meshPart.Effect as BasicEffect;


                        matEffect.DiffuseColor = oEffect.DiffuseColor;

                        if (oEffect.TextureEnabled)
                        {
                            matEffect.Diffuse = oEffect.Texture;

                            string[] name = matEffect.Diffuse.Name.Split('\\');

                            string compare = name[2].Replace("_0", "");

                            if (compare.Contains("vase"))
                            {
                                matEffect.Roughness = 0.1f;
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
                                        matEffect.Specular = tex2d;
                                    }

                                    if (ending == "_ddn")
                                    {
                                        matEffect.Normal = tex2d;
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
