using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources.Helper
{
    public class DataStream
    {
        /*
         * I created this data stream to save SDF 32 bit files.
         * Format: int width, int height, int zdepth, float[] data
         * 
         * 
         * 
         * 
         */


        #region SDF Data

        public static void SaveImageData(float[] data, int width, int height, int zdepth, string path)
        {
            
            // create a byte array and copy the floats into it...

            if (data.Length != width * height * zdepth)
            {
                throw new Exception("Your output dimensions do not match!");
            }

            var byteArray = new byte[data.Length * 4];
            Buffer.BlockCopy(data, 0, byteArray, 0, byteArray.Length);
            
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.OpenOrCreate);

                //Write resolution first
                BinaryWriter Writer = new BinaryWriter(fs);

                //
                Writer.Write(BitConverter.GetBytes(width));
                Writer.Write(BitConverter.GetBytes(height));
                Writer.Write(BitConverter.GetBytes(zdepth));

                Writer.Write(byteArray);

                Writer.Flush();
                Writer.Close();
                fs.Close();
            }
            finally
            {
                if(fs!=null) fs.Dispose();
            }
        }

        /// <summary>
        /// Returns a true and a texture if the file is available. Otherwise false and nulls.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="path"></param>
        /// <param name="zdepth"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static bool LoadFromFile(GraphicsDevice graphics, string path, out int zdepth, out Texture2D output)
        {
            float[] data;
            int width;
            int height;
            if (LoadFloatArray(path, out data, out width, out height, out zdepth))
            {
                output = new Texture2D(graphics, width * zdepth, height, false, SurfaceFormat.Single);
                output.SetData(data);
                return true;
            }
            output = null;
            return false;
        }

        //Returns true if successful, else false
        public static bool LoadFloatArray(string path, out float[] floatArray, out int width, out int height, out int zdepth )
        {
            //Debug.WriteLine(path);  

            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.OpenOrCreate);
                BinaryReader Reader = new BinaryReader(fs);

                width = Reader.ReadInt32();
                height = Reader.ReadInt32();
                zdepth = Reader.ReadInt32();

                byte[] byteArray = Reader.ReadBytes(width * height * zdepth * 4);

                Reader.Close();
                fs.Close();

                floatArray = new float[byteArray.Length / 4];
                Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
            }
            catch (Exception e)
            {
                width = 0;
                height = 0;
                zdepth = 0;
                floatArray = null;

                Debug.WriteLine(e.Message);
                return false;


                //throw e;
                
            }
            finally
            {
                if (fs != null) fs.Dispose();
            }
            return true;
        }

        #endregion


        public static void SaveBoundingBoxData(BoundingBox bbox, string path)
        {
            if (bbox == null)
            {
                throw new Exception("Bounding Box not yet initialized");
            }

            var byteArray = new byte[6 * 4];

            float[] data = new float[6];
            data[0] = bbox.Min.X;
            data[1] = bbox.Min.Y;
            data[2] = bbox.Min.Z;
            data[3] = bbox.Max.X;
            data[4] = bbox.Max.Y;
            data[5] = bbox.Max.Z;
            Buffer.BlockCopy(data, 0, byteArray, 0, byteArray.Length);

            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.OpenOrCreate);

                BinaryWriter Writer = new BinaryWriter(fs);

                Writer.Write(byteArray);

                Writer.Flush();
                Writer.Close();
                fs.Close();
            }
            finally
            {
                if (fs != null) fs.Dispose();
            }
        }


        /// <summary>
        /// Returns true if loaded, otherwise false = doesn't exist
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public static bool LoadBoundingBox(string path, out BoundingBox bbox)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.OpenOrCreate);
                BinaryReader Reader = new BinaryReader(fs);
                
                byte[] byteArray = Reader.ReadBytes(6 * 4);

                Reader.Close();
                fs.Close();

                float[] fa = new float[byteArray.Length / 4];
                Buffer.BlockCopy(byteArray, 0, fa, 0, byteArray.Length);

                bbox = new BoundingBox(new Vector3(fa[0], fa[1], fa[2]), new Vector3(fa[3], fa[4], fa[5]));
            }
            catch (Exception e)
            {
                bbox = new BoundingBox();
                return false;
            }
            finally
            {
                if (fs != null) fs.Dispose();
            }
            return true;
        }

    }
}
