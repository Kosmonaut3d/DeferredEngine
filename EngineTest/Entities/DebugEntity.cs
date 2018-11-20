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
    public class DebugEntity : TransformableObject
    {
        public sealed override Vector3 Position { get; set; }
        public override Vector3 Scale { get; set; }
        public sealed override int Id { get; set; }
        public override Matrix RotationMatrix { get; set; }
        public override bool IsEnabled { get; set; }
        public override TransformableObject Clone { get; }
        public sealed override string Name { get; set; }
        
        public Vector3 Size;

        public Vector3 Resolution = 2 * Vector3.One;

        public Vector3 Offset;

        public Texture2D Texture;
        public bool NeedsUpdate = false;

        public DebugEntity(string texturepath, GraphicsDevice graphics, Vector3 position, Vector3 size)
        {
            throw new NotImplementedException();
            //Position = position;

            //Id = IdGenerator.GetNewId();
            //Name = GetType().Name + " " + Id;
            //int zdepth;
            //Texture = DataStream.LoadFromFile(graphics, texturepath, out zdepth);
            //Resolution.Y = Texture.Height;
            //Resolution.Z = zdepth;
            //Resolution.X = Texture.Width / zdepth;

            //Size = size;

        }
    }
}
