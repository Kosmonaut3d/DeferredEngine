using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        public static void SaveImageData(float[] data, int width, int height, int zdepth, string path)
        {
            
            // create a byte array and copy the floats into it...

            if (data.Length != width * height * zdepth)
            {
                throw new Exception("Your output dimensions do not match!");
                return;
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

        public static Texture2D LoadFromFile(GraphicsDevice graphics, string path, out int zdepth)
        {
            Texture2D output;
            float[] data;
            int width;
            int height;
            LoadFloatArray(path, out data, out width, out height, out zdepth);

            output = new Texture2D(graphics, width * zdepth, height, false, SurfaceFormat.Single);
            output.SetData(data);

            return output;
        }

        public static void LoadFloatArray(string path, out float[] floatArray, out int width, out int height, out int zdepth )
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
                throw e;
            }
            finally
            {
                if (fs != null) fs.Dispose();
            }
        }

    }
}
