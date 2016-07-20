using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace EngineTest.Renderer
{
    public class Camera
    {
        public Vector3 Position;
        public Vector3 Up = -Vector3.UnitZ;
        public Vector3 Forward = Vector3.Up;

        public Camera(Vector3 position, Vector3 lookat)
        {
            Position = position;
            Forward = lookat - position;
            Forward.Normalize();
        }

        public Vector3 Lookat
        {
            get { return Position + Forward; }
            set
            {
                Forward = value - Position;
                Forward.Normalize();
            }
        }
    }
}
