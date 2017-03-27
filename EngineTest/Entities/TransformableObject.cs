using Microsoft.Xna.Framework;

namespace DeferredEngine.Entities
{
    public abstract class TransformableObject
    {
        public abstract Vector3 Position { get; set; }
        public abstract Vector3 Scale { get; set; }

        public abstract int Id { get; set; }
        public abstract Matrix RotationMatrix { get; set; }

        public abstract bool IsEnabled { get; set; }

        public abstract TransformableObject Clone { get; }

        public abstract string Name { get; set; }
    }
}