using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Entities;
using BEPUutilities;
using EngineTest.Recources;
using EngineTest.Recources.Helper;
using EngineTest.Renderer.Helper;
using Microsoft.Xna.Framework.Graphics;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;

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

        

        public readonly TransformMatrix WorldTransform;
        public Matrix RotationMatrix;
        private Matrix _worldOldMatrix = Matrix.Identity;
        private Matrix _worldNewMatrix = Matrix.Identity;
        public readonly float Scale = 1;
        
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
                RotationMatrix = Matrix.CreateRotationX((float) AngleX)*Matrix.CreateRotationY((float) AngleY)*
                                  Matrix.CreateRotationZ((float) AngleZ);
                Matrix scaleMatrix = Matrix.CreateScale(Scale);
                _worldOldMatrix = scaleMatrix* RotationMatrix * Matrix.CreateTranslation(Position);

                WorldTransform.Scale = Scale;
                WorldTransform.World = _worldOldMatrix;
                
                if (StaticPhysicsObject != null && !GameSettings.Editor_enable)
                {
                    AffineTransform change = new AffineTransform(
                            new BEPUutilities.Vector3(Scale, Scale, Scale),
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
