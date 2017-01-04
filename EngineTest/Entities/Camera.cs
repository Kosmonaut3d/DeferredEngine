using System;
using Microsoft.Xna.Framework;

namespace DeferredEngine.Entities
{
    public class Camera
    {
        private Vector3 _position;
        private Vector3 _up = Vector3.UnitZ;
        private Vector3 _forward = Vector3.Up;
        private float _fieldOfView = (float) Math.PI/4;

        public bool HasChanged = true;
        public bool HasMoved;

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
                    HasMoved = true;
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
            get { return _fieldOfView; }
            set
            {
                _fieldOfView = value;
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
