using System;
using System.Collections.Generic;
using System.IO;
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
        public Vector4 TextureResolution = new Vector4(50,50,50,0); //x,y,z and w is index/starty in atlas
        public Vector3 Offset;

        public string TexturePath;
        public bool NeedsToBeGenerated = false;
        public bool IsLoaded = false;

        //For stuff like background etc. that doesn't need an SDF
        public bool IsUsed = true;

        public int ArrayIndex;

        /// <summary>
        /// A volume texture around for meshes. Sample points give the minimum distance.
        /// </summary>
        /// <param name="texturepath"></param>
        /// <param name="graphics"></param>
        public SignedDistanceField(string texturepath, GraphicsDevice graphics, BoundingBox boundingBox, Vector3 offset, Vector3 textureResolution)
        {
            TexturePath = texturepath;
            TextureResolution = new Vector4(textureResolution, 0);

            //Automatic padding
            VolumeSize = (boundingBox.Max - boundingBox.Min) / 2.0f/* * (textureResolution+Vector3.One *2) / textureResolution;*/ ;
            Offset = offset;

            //Check if our file is available
            int zdepth;
            if (File.Exists(texturepath) && DataStream.LoadFromFile(graphics, texturepath, out zdepth, out SdfTexture))
            {
                TextureResolution.Y = SdfTexture.Height;
                TextureResolution.Z = zdepth;
                TextureResolution.X = SdfTexture.Width / zdepth;

                //Need new?
                if (Vector3.Distance(new Vector3(TextureResolution.X, TextureResolution.Y, TextureResolution.Z),
                        new Vector3(textureResolution.X, textureResolution.Y, textureResolution.Z)) > 0.1f)
                {
                    TextureResolution = new Vector4(textureResolution, 0);
                    NeedsToBeGenerated = true;
                }
                else
                {
                    IsLoaded = true;
                }
            }
            else //otherwise create a new one
            {
                NeedsToBeGenerated = true;
            }
        }
        
    }
}
