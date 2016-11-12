using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace EngineTest.Recources.Helper
{
   public static class Extensions
    {
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
    }
}
