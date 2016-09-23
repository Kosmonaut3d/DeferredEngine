using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineTest.Recources;
using EngineTest.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Entities
{
    public class BasicEntity
    {
        public Model Model;
        public MaterialEffect Material;

        private Vector3 _position;

        public Vector3 Position
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

        public double AngleZ;
        public double AngleX; //forward
        public double AngleY;

        public TransformMatrix WorldTransform = new TransformMatrix(Matrix.Identity);
        public Matrix RotationMatrix;
        public Matrix WorldOldMatrix;

        public float Scale = 1;



        public BasicEntity(Model model, MaterialEffect material, Vector3 position, double angleZ, double angleX, double angleY, float scale, MeshMaterialLibrary library)
        {
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

        public float Scale;

        public TransformMatrix(Matrix world)
        {
            World = world;
        }

        public Vector3 TransformMatrixSubModel(Vector3 translateSub)
        {
            return Vector3.Transform(translateSub, World);
        }
    }
}
