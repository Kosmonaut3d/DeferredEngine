using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Entities;
using BEPUutilities;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework.Graphics;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Quaternion = BEPUutilities.Quaternion;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace DeferredEngine.Entities
{
    public sealed class BasicEntity : TransformableObject
    {
        public readonly Model Model;
        public readonly MaterialEffect Material;

        private int _id;

        private Vector3 _position;

        private Entity _dynamicPhysicsObject;
        public StaticMesh StaticPhysicsObject = null;

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

        private Vector3 _scale;
        public override Vector3 Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                WorldTransform.HasChanged = true;
                _scale = value;
            }
        }

        public override int Id {
            get { return _id; }
            set { _id = value; } }

        public Matrix _rotationMatrix;

        public override Matrix RotationMatrix
        {
            get { return _rotationMatrix; }
            set
            {
                _rotationMatrix = value;
                WorldTransform.HasChanged = true;
            }
        }
        
        public override bool IsEnabled { get; set; }

        public override TransformableObject Clone {
            get
            {
                return new BasicEntity(Model, Material, Position, RotationMatrix, Scale );   
            }  
        }

        public override string Name { get; set; }


        public readonly TransformMatrix WorldTransform;
        private Matrix _worldOldMatrix = Matrix.Identity;
        private Matrix _worldNewMatrix = Matrix.Identity;
        
        public BasicEntity(Model model, MaterialEffect material, Vector3 position, double angleZ, double angleX, double angleY, Vector3 scale, MeshMaterialLibrary library = null, Entity physicsObject = null)
        {
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;
            WorldTransform = new TransformMatrix(Matrix.Identity, Id);
            Model = model;
            Material = material;
            Position = position;
            Scale = scale;
            
            RotationMatrix = Matrix.CreateRotationX((float)angleX) * Matrix.CreateRotationY((float)angleY) *
                                  Matrix.CreateRotationZ((float)angleZ);

            if (library != null)
                RegisterInLibrary(library);

            if (physicsObject != null)
                RegisterPhysics(physicsObject);

        }

        public BasicEntity(Model model, MaterialEffect material, Vector3 position, Matrix rotationMatrix, Vector3 scale)
        {
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;
            WorldTransform = new TransformMatrix(Matrix.Identity, Id);
            Model = model;
            Material = material;
            Position = position;
            RotationMatrix = rotationMatrix;
            Scale = scale;
            RotationMatrix = rotationMatrix;
        }

        public void RegisterInLibrary(MeshMaterialLibrary library)
        {
            library.Register(Material, Model, WorldTransform);
        }

        private void RegisterPhysics(Entity physisEntity)
        {
            _dynamicPhysicsObject = physisEntity;
            _dynamicPhysicsObject.Position = new BEPUutilities.Vector3(Position.X, Position.Y, Position.Z);
        }

        public void Dispose(MeshMaterialLibrary library)
        {
            library.DeleteFromRegistry(this);
        }

        public void ApplyTransformation()
        {
            if (_dynamicPhysicsObject == null)
            {
                //RotationMatrix = Matrix.CreateRotationX((float) AngleX)*Matrix.CreateRotationY((float) AngleY)*
                //                  Matrix.CreateRotationZ((float) AngleZ);
                Matrix scaleMatrix = Matrix.CreateScale(Scale);
                _worldOldMatrix = scaleMatrix* RotationMatrix * Matrix.CreateTranslation(Position);

                WorldTransform.Scale = Scale;
                WorldTransform.World = _worldOldMatrix;
                
                if (StaticPhysicsObject != null && !GameSettings.Editor_enable)
                {
                    AffineTransform change = new AffineTransform(
                            new BEPUutilities.Vector3(Scale.X, Scale.Y, Scale.Z),
                            Quaternion.CreateFromRotationMatrix(MathConverter.Convert(RotationMatrix)),
                            MathConverter.Convert(Position));

                    if (!MathConverter.Equals(change.Matrix, StaticPhysicsObject.WorldTransform.Matrix))
                    {
                        //StaticPhysicsMatrix = MathConverter.Copy(Change.Matrix);

                        StaticPhysicsObject.WorldTransform = change;
                    }
                }
            }
            else
            {
                //Has something changed?
                WorldTransform.Scale = Scale;
                _worldOldMatrix = Extensions.CopyFromBepuMatrix(_worldOldMatrix, _dynamicPhysicsObject.WorldTransform);
                Matrix scaleMatrix = Matrix.CreateScale(Scale);
                //WorldOldMatrix = Matrix.CreateScale(Scale)*WorldOldMatrix; 
                WorldTransform.World = scaleMatrix * _worldOldMatrix;

            }
        }

        internal void CheckPhysics()
        {
            if (_dynamicPhysicsObject == null) return;

            _worldNewMatrix = Extensions.CopyFromBepuMatrix(_worldNewMatrix, _dynamicPhysicsObject.WorldTransform);

            if (_worldNewMatrix != _worldOldMatrix)
            {
                WorldTransform.HasChanged = true;
                _worldOldMatrix = _worldNewMatrix;
                Position = _worldOldMatrix.Translation;
            }
            else
            {
                if (Position != _worldNewMatrix.Translation && GameSettings.Editor_enable)
                {
                    //DynamicPhysicsObject.Position = new BEPUutilities.Vector3(Position.X, Position.Y, Position.Z);
                    _dynamicPhysicsObject.Position = MathConverter.Convert(Position);
                }
                //    DynamicPhysicsObject.Position = new BEPUutilities.Vector3(Position.X, Position.Y, Position.Z);
                //    //WorldNewMatrix = Extensions.CopyFromBepuMatrix(WorldNewMatrix, DynamicPhysicsObject.WorldTransform);
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

        public Vector3 Scale;

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
