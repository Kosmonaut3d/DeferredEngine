using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeferredEngine.Recources.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{
    public class SignedDistanceField
    {
        public Texture2D SdfTexture;
        
        public Vector3 VolumeSize;
        public Vector3 TextureResolution = new Vector3(50,50,50);
        public Vector3 Offset;

        public string TexturePath;
        public bool NeedsToBeGenerated = false;
        public bool IsLoaded = false;

        //For stuff like background etc. that doesn't need an SDF
        public bool IsUsed = true;

        /// <summary>
        /// A volume texture around for meshes. Sample points give the minimum distance.
        /// </summary>
        /// <param name="texturepath"></param>
        /// <param name="graphics"></param>
        public SignedDistanceField(string texturepath, GraphicsDevice graphics, BoundingBox boundingBox, Vector3 offset, Vector3 textureResolution)
        {
            TexturePath = texturepath;
            TextureResolution = textureResolution;

            //Automatic padding
            VolumeSize = (boundingBox.Max - boundingBox.Min) / 2.0f/* * (textureResolution+Vector3.One *2) / textureResolution;*/ ;
            Offset = offset;

            //Check if our file is available
            int zdepth;
            if (DataStream.LoadFromFile(graphics, texturepath, out zdepth, out SdfTexture))
            {
                TextureResolution.Y = SdfTexture.Height;
                TextureResolution.Z = zdepth;
                TextureResolution.X = SdfTexture.Width / zdepth;
                IsLoaded = true;
            }
            else //otherwise create a new one
            {
                NeedsToBeGenerated = true;
            }
        }
        
    }
}
