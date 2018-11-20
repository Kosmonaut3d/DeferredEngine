using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeferredEngine.Recources.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Entities
{
    public class Decal : TransformableObject
    {
        public Vector3 _scale;
        private Vector3 _position;
        private Matrix _rotationMatrix;

        public override Vector3 Position
        {
            get { return _position; }
            set { _position = value;
                UpdateWorldMatrix();
            }
        }

        public override int Id { get; set; }

        public sealed override Matrix RotationMatrix
        {
            get { return _rotationMatrix; }
            set { _rotationMatrix = value;
                UpdateWorldMatrix();
            }
        }

        public override bool IsEnabled { get; set; }

        public override TransformableObject Clone
        {
            get { return new Decal(Texture, Position, RotationMatrix, Scale);}
        }

        public override string Name { get; set; }

        public override Vector3 Scale
        {
            get { return _scale; }
            set
            {
                _scale = value; 
                UpdateWorldMatrix();
            }
        }

        public Matrix World;
        public Matrix InverseWorld;
        public Texture2D Texture;

        public Decal(Texture2D texture, Vector3 position, double angleZ, double angleX, double angleY, Vector3 scale) : 
            this(texture, position, Matrix.CreateRotationX((float)angleX) * Matrix.CreateRotationY((float)angleY) *
                                  Matrix.CreateRotationZ((float)angleZ), scale)
        { }

        public Decal(Texture2D texture, Vector3 position, Matrix rotationMatrix, Vector3 scale)
        {
            Texture = texture;
            Position = position;
            RotationMatrix = rotationMatrix;

            _scale = scale;
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;

            UpdateWorldMatrix();
        }

        public void UpdateWorldMatrix()
        {
            World = Matrix.CreateScale(Scale) * RotationMatrix * Matrix.CreateTranslation(Position);
            InverseWorld = Matrix.Invert(World);
        }
    }
}
