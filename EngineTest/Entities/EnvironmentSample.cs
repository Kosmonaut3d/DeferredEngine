using DeferredEngine.Recources.Helper;
using Microsoft.Xna.Framework;

namespace DeferredEngine.Entities
{
    public class EnvironmentSample : TransformableObject
    {
        public bool NeedsUpdate = true;
        private Vector3 _position;

        public override Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                if(AutoUpdate)
                    NeedsUpdate = true;
            }
        }

        public override Vector3 Scale { get; set; }

        public float SpecularStrength = 1;
        public float DiffuseStrength = 0.2f;

        public bool AutoUpdate = true;

        public bool UseSDFAO = false;

        public override int Id { get; set; }
        public override Matrix RotationMatrix { get; set; }
        public override bool IsEnabled { get; set; }
        public override TransformableObject Clone { get; }
        public override string Name { get; set; }

        public EnvironmentSample(Vector3 position)
        {
            Position = position;
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;
        }

        public void Update()
        {
            NeedsUpdate = true;
        }
    }
    
}
