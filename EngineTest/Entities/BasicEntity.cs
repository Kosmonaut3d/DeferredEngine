using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using EngineTest.Recources;
using EngineTest.Recources.Helper;
using EngineTest.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Entities
{
    public abstract class TransformableObject
    {
        public abstract Vector3 Position { get; set; }
        public abstract int Id { get; set; }
    }

    public class BasicEntity : TransformableObject
    {
        public Model Model;
        public MaterialEffect Material;

        public int _id;

        private Vector3 _position;

        public override Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                WorldTransform.HasChanged = true;
                _position = value;
            }
        }

        public override int Id {
            get { return _id; }
            set { _id = value; } }

        private double _angleZ;
        private double _angleX; //forward
        private double _angleY;

        public double AngleZ
        {
            get
            {
                return _angleZ;
            }
            set
            {
                WorldTransform.HasChanged = true;
                _angleZ = value;
            }
        }
        public double AngleX
        {
            get
            {
                return _angleX;
            }
            set
            {
                WorldTransform.HasChanged = true;
                _angleX = value;
            }
        } //forward
        public double AngleY
        {
            get
            {
                return _angleY;
            }
            set
            {
                WorldTransform.HasChanged = true;
                _angleY = value;
            }
        }

        public TransformMatrix WorldTransform;
        public Matrix RotationMatrix;
        public Matrix WorldOldMatrix;

        public float Scale = 1;



        public BasicEntity(Model model, MaterialEffect material, Vector3 position, double angleZ, double angleX, double angleY, float scale, MeshMaterialLibrary library)
        {
            Id = IdGenerator.GetNewId();
            WorldTransform = new TransformMatrix(Matrix.Identity, Id);

            Position = position;
            AngleZ = angleZ;
            AngleX = angleX;
            AngleY = angleY;
            Scale = scale;

            Material = material;
            Model = model;

            library.Register(Material, Model, WorldTransform);

            
        }

        public void Dispose(MeshMaterialLibrary library)
        {
            library.DeleteFromRegistry(this);
        }

        protected BasicEntity()
        {
            //throw new NotImplementedException();
        }

        public virtual void ApplyTransformation()
        {
            Matrix Rotation = Matrix.CreateRotationX((float)AngleX) * Matrix.CreateRotationY((float)AngleY) *
                               Matrix.CreateRotationZ((float)AngleZ);
            Matrix ScaleMatrix = Matrix.CreateScale(Scale);
            WorldOldMatrix = ScaleMatrix * Rotation * Matrix.CreateTranslation(Position);

            WorldTransform.Scale = Scale;
            WorldTransform.World = WorldOldMatrix;
            
        }

        public virtual void SetRenderMode(bool isRendered)
        {
            WorldTransform.Rendered = isRendered;
        }
    }

    public class TransformMatrix
    {
        public Matrix World;
        public bool Rendered = true;
        public bool HasChanged = true;
        public readonly int Id;

        public float Scale;

        public TransformMatrix(Matrix world, int id)
        {
            World = world;
            Id = id;
        }

        public Vector3 TransformMatrixSubModel(Vector3 translateSub)
        {
            return Vector3.Transform(translateSub, World);
        }
    }
}
