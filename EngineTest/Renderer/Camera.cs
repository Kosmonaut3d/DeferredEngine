using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace EngineTest.Renderer
{
    public class Camera
    {
        private Vector3 _position;
        private Vector3 _up = -Vector3.UnitZ;
        private Vector3 _forward = Vector3.Up;
        private float fieldOfView = (float) Math.PI/4;

        public bool HasChanged = true;

        public Camera(Vector3 position, Vector3 lookat)
        {
            _position = position;
            _forward = lookat - position;
            _forward.Normalize();
        }

        public Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (_position != value)
                {
                    _position = value;
                    HasChanged = true;
                }
            }
        }
        
        public Vector3 Up
        {
            get
            {
                return _up;
            }
            set
            {
                if (_up != value)
                {
                    _up = value;
                    HasChanged = true;
                }
            }
        }

        public Vector3 Forward
        {
            get
            {
                return _forward;
            }
            set
            {
                if (_forward != value)
                {
                    _forward = value;
                    HasChanged = true;
                }
            }
        }

        public float FieldOfView
        {
            get { return fieldOfView; }
            set
            {
                fieldOfView = value;
                HasChanged = true;
            }
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
