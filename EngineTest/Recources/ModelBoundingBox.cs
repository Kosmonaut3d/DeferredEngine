using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using DeferredEngine.Recources.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{
    public class ModelBoundingBox
    {
        public BoundingBox BoundingBox;
        public Vector3 BoundingBoxOffset;
        public Model Model;

        public ModelBoundingBox(ContentManager content, string assetpath)
        {
            
            Model = content.Load<Model>(assetpath);

            //Look if there is a bounding box already created, otherwise create a new one
            if (DataStream.LoadBoundingBox(content.RootDirectory + "/" + assetpath + ".bbox", out BoundingBox) == false)
            {
                CreateBoundingBox(Model);

                //Optionally save that new one
                if (GameSettings.e_saveBoundingBoxes)
                {
                    DataStream.SaveBoundingBoxData(BoundingBox, content.RootDirectory + "/" + assetpath + ".bbox");
                }
            }

            //Find the middle
            BoundingBoxOffset = (BoundingBox.Max + BoundingBox.Min) / 2.0f;
        }

        public ModelBoundingBox(Model model, BoundingBox box)
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
