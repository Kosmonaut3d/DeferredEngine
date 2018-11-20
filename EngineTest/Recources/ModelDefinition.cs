using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeferredEngine.Entities;
using DeferredEngine.Recources.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{
    public class ModelDefinition
    {
        public BoundingBox BoundingBox;
        public Vector3 BoundingBoxOffset;
        public Model Model;
        public SignedDistanceField SDF;

        public ModelDefinition(ContentManager content, string assetpath, GraphicsDevice graphics, bool UseSDF, Vector3 sdfResolution /*default = 50^3*/)
        {
            
            Model = content.Load<Model>(assetpath);

            string bbxpath = content.RootDirectory + "/" + assetpath + ".bbox";
            //Look if there is a bounding box already created, otherwise create a new one
            if (!File.Exists(bbxpath) || !DataStream.LoadBoundingBox(bbxpath, out BoundingBox))
            {
                CreateBoundingBox(Model);

                //Optionally save that new one
                if (GameSettings.e_saveBoundingBoxes)
                {
                    DataStream.SaveBoundingBoxData(BoundingBox, bbxpath);
                }
            }

            //Find the middle
            BoundingBoxOffset = (BoundingBox.Max + BoundingBox.Min) / 2.0f;

            //SDF
            SDF = new SignedDistanceField(content.RootDirectory + "/" + assetpath + ".sdft", graphics, BoundingBox, BoundingBoxOffset, sdfResolution);
            SDF.IsUsed = UseSDF;
        }

        public ModelDefinition(ContentManager content, string assetpath, GraphicsDevice graphics, bool UseSDF = false) : this(content,
            assetpath, graphics, UseSDF, new Vector3(50, 50, 50))
        { }
        

        public ModelDefinition(Model model, BoundingBox box)
        {
            Model = model;
            BoundingBox = box;
            BoundingBoxOffset = (BoundingBox.Max + BoundingBox.Min) / 2.0f;
        }
        
        private void CreateBoundingBox(Model model)
        {
            Vector3[] vertices;
            int[] indices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(model, out vertices, out indices);

            BoundingBox = BoundingBox.CreateFromPoints(vertices);

        }

    }
    
}
