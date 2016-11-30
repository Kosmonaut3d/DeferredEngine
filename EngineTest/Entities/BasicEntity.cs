using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using ConversionHelper;
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
        public abstract double AngleZ { get; set; }
        public abstract double AngleX { get; set; }
        public abstract double AngleY { get; set; }

        public abstract TransformableObject Clone { get; }
    }

    public class BasicEntity : TransformableObject
    {
        public Model Model;
        public MaterialEffect Material;

        public int _id;

        private Vector3 _position;

        public Entity PhysicsAttachment = null;

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

        public override double AngleZ
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
        public override double AngleX
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
        public override double AngleY
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

        public override TransformableObject Clone {
            get
            {
                return new BasicEntity(Model, Material, Position, AngleZ, AngleX, AngleY, Scale );   
            }  
        }

        public TransformMatrix WorldTransform;
        public Matrix RotationMatrix;
        public Matrix WorldOldMatrix = Matrix.Identity;
        public Matrix WorldNewMatrix = Matrix.Identity;
        public float Scale = 1;
        
        public BasicEntity(Model model, MaterialEffect material, Vector3 position, double angleZ, double angleX, double angleY, float scale, MeshMaterialLibrary library = null, Entity physicsObject = null)
        {
            Id = IdGenerator.GetNewId();
            WorldTransform = new TransformMatrix(Matrix.Identity, Id);

            Position = position;
            AngleZ = angleZ;
            AngleX = angleX;
            AngleY = angleY;
            Scale = scale;

            RotationMatrix = Matrix.CreateRotationX((float)AngleX) * Matrix.CreateRotationY((float)AngleY) *
                                  Matrix.CreateRotationZ((float)AngleZ);

            Material = material;
            Model = model;

            if(library!=null)
            RegisterInLibrary(library);

            if(physicsObject!=null)
                RegisterPhysics(physicsObject);
            
        }

        public void RegisterInLibrary(MeshMaterialLibrary library)
        {
            library.Register(Material, Model, WorldTransform);
        }

        public void RegisterPhysics(Entity PhysisEntity)
        {
            PhysicsAttachment = PhysisEntity;
            PhysicsAttachment.Position = new BEPUutilities.Vector3(Position.X, Position.Y, Position.Z);
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
            if (PhysicsAttachment == null)
            {
                RotationMatrix = Matrix.CreateRotationX((float) AngleX)*Matrix.CreateRotationY((float) AngleY)*
                                  Matrix.CreateRotationZ((float) AngleZ);
                Matrix ScaleMatrix = Matrix.CreateScale(Scale);
                WorldOldMatrix = ScaleMatrix* RotationMatrix * Matrix.CreateTranslation(Position);

                WorldTransform.Scale = Scale;
                WorldTransform.World = WorldOldMatrix;
            }
            else
            {
                //Has something changed?
                WorldTransform.Scale = Scale;
                WorldOldMatrix = Extensions.CopyFromBepuMatrix(WorldOldMatrix, PhysicsAttachment.WorldTransform);
                Matrix ScaleMatrix = Matrix.CreateScale(Scale);


                //WorldOldMatrix = Matrix.CreateScale(Scale)*WorldOldMatrix; 
                WorldTransform.World = ScaleMatrix * WorldOldMatrix;
            }
        }

        public virtual void SetRenderMode(bool isRendered)
        {
            WorldTransform.Rendered = isRendered;
        }

        internal void CheckPhysics()
        {
            if (PhysicsAttachment == null) return;

            WorldNewMatrix = Extensions.CopyFromBepuMatrix(WorldNewMatrix, PhysicsAttachment.WorldTransform);

            if (WorldNewMatrix != WorldOldMatrix)
            {
                WorldTransform.HasChanged = true;
                WorldOldMatrix = WorldNewMatrix;
                Position = WorldOldMatrix.Translation;
            }
            else
            {
                if (Position != WorldNewMatrix.Translation && GameSettings.Editor_enable)
                {
                    //PhysicsAttachment.Position = new BEPUutilities.Vector3(Position.X, Position.Y, Position.Z);
                    PhysicsAttachment.Position = MathConverter.Convert(Position);
                }
                //    PhysicsAttachment.Position = new BEPUutilities.Vector3(Position.X, Position.Y, Position.Z);
                //    //WorldNewMatrix = Extensions.CopyFromBepuMatrix(WorldNewMatrix, PhysicsAttachment.WorldTransform);
                //    //if (Position != WorldNewMatrix.Translation)
                //    //{
                //    //    var i = 0;
                //    //}

                //}
                //else
                //{
                //    //Position = WorldOldMatrix.Translation;
                //}
            }

            
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
