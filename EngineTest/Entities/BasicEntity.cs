using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Entities;
using BEPUutilities;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Quaternion = BEPUutilities.Quaternion;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace DeferredEngine.Entities
{
    public sealed class BasicEntity : TransformableObject
    {
        //Avoid nesting, but i could also just provide the ModelDefinition instead
        public readonly ModelDefinition ModelDefinition;
        public readonly Model Model;
        public readonly BoundingBox BoundingBox;
        public readonly Vector3 BoundingBoxOffset;
        public readonly SignedDistanceField SignedDistanceField;
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
                //Not very clean...
                return new BasicEntity(ModelDefinition, Material, Position, RotationMatrix, Scale );   
            }  
        }

        public override string Name { get; set; }


        public readonly TransformMatrix WorldTransform;
        private Matrix _worldOldMatrix = Matrix.Identity;
        private Matrix _worldNewMatrix = Matrix.Identity;
        
        public BasicEntity(ModelDefinition modelbb, MaterialEffect material, Vector3 position, double angleZ, double angleX, double angleY, Vector3 scale, MeshMaterialLibrary library = null, Entity physicsObject = null)
        {
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;
            WorldTransform = new TransformMatrix(Matrix.Identity, Id);
            ModelDefinition = modelbb;
            Model = modelbb.Model;
            BoundingBox = modelbb.BoundingBox;
            BoundingBoxOffset = modelbb.BoundingBoxOffset;
            SignedDistanceField = modelbb.SDF;
            
            Material = material;
            Position = position;
            Scale = scale;
            
            RotationMatrix = Matrix.CreateRotationX((float)angleX) * Matrix.CreateRotationY((float)angleY) *
                                  Matrix.CreateRotationZ((float)angleZ);

            if (library != null)
                RegisterInLibrary(library);

            if (physicsObject != null)
                RegisterPhysics(physicsObject);

            WorldTransform.World = Matrix.CreateScale(Scale) * RotationMatrix * Matrix.CreateTranslation(Position);
            WorldTransform.Scale = Scale;
            WorldTransform.InverseWorld = Matrix.Invert(Matrix.CreateTranslation(BoundingBoxOffset * Scale) * RotationMatrix * Matrix.CreateTranslation(Position));
        }

        public BasicEntity(ModelDefinition modelbb, MaterialEffect material, Vector3 position, Matrix rotationMatrix, Vector3 scale)
        {
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;
            WorldTransform = new TransformMatrix(Matrix.Identity, Id);
            Model = modelbb.Model;
            ModelDefinition = modelbb;
            BoundingBox = modelbb.BoundingBox;
            BoundingBoxOffset = modelbb.BoundingBoxOffset;
            SignedDistanceField = modelbb.SDF;

            Material = material;
            Position = position;
            RotationMatrix = rotationMatrix;
            Scale = scale;
            RotationMatrix = rotationMatrix;

            WorldTransform.World = Matrix.CreateScale(Scale) * RotationMatrix * Matrix.CreateTranslation(Position);
            WorldTransform.Scale = Scale;
            WorldTransform.InverseWorld = Matrix.Invert(Matrix.CreateTranslation(BoundingBoxOffset * Scale) * RotationMatrix * Matrix.CreateTranslation(Position));
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

                WorldTransform.InverseWorld = Matrix.Invert(Matrix.CreateTranslation(BoundingBoxOffset * Scale) * RotationMatrix * Matrix.CreateTranslation(Position));
                
                if (StaticPhysicsObject != null && !GameSettings.e_enableeditor)
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

                WorldTransform.InverseWorld = Matrix.Invert(Matrix.CreateTranslation(BoundingBoxOffset * Scale) * RotationMatrix * Matrix.CreateTranslation(Position));

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
                if (Position != _worldNewMatrix.Translation && GameSettings.e_enableeditor)
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
        public Matrix InverseWorld;
        public bool Rendered = true;
        public bool HasChanged = true;
        public readonly int Id;

        public Vector3 Scale;

        public Matrix World;

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
