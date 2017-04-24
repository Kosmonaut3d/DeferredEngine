using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace DeferredEngine.Recources.Helper
{
   public static class Extensions
    {
        public static void SetValue(this EffectParameter param, int[] value)
        {
            for (var i = 0; i < value.Length; i++)
                param.Elements[i].SetValue(value[i]);

            //param.StateKey = unchecked(param.NextStateKey++);
        }

        public static Vector3 Xyz(this Vector4 vec3)
       {
           return new Vector3(vec3.X, vec3.Y, vec3.Z);
       }

       public static Vector2 Xy(this Vector4 vec3)
       {
           return new Vector2(vec3.X, vec3.Y);
       }

       public static Vector3 Xyz(this HalfVector4 vec3)
       {
           return vec3.ToVector4().Xyz();
       }

        public static Vector3 Pow(this Vector3 vec3, float power)
        {
            return new Vector3((float) Math.Pow(vec3.X, power), (float)Math.Pow(vec3.Y, power), (float)Math.Pow(vec3.Z, power));
        }

        public static Matrix CopyFromBepuMatrix(Matrix mat, BEPUutilities.Matrix matrix)
        {
            mat.M11 = matrix.M11;
            mat.M12 = matrix.M12;
            mat.M13 = matrix.M13;
            mat.M14 = matrix.M14;

            mat.M21 = matrix.M21;
            mat.M22 = matrix.M22;
            mat.M23 = matrix.M23;
            mat.M24 = matrix.M24;

            mat.M31 = matrix.M31;
            mat.M32 = matrix.M32;
            mat.M33 = matrix.M33;
            mat.M34 = matrix.M34;

            mat.M41 = matrix.M41;
            mat.M42 = matrix.M42;
            mat.M43 = matrix.M43;
            mat.M44 = matrix.M44;

            return mat;
        }
    }
}
