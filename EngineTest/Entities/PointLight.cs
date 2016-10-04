using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
    public class DirectionalLight
    {
        public Color Color;
        public float Intensity;
        private Vector3 _direction;
        public bool HasChanged;

        /// <summary>
        /// A point light is a light that shines in all directions
        /// </summary>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        /// <param name="intensity"></param>
        /// <param name="drawShadows">will render shadow maps</param>
        /// <param name="shadowResolution">shadow map resolution per face. Optional</param>
        /// <param name="staticShadow">if set to true the shadows will not update at all. Dynamic shadows in contrast update only when needed.</param>
        /// <returns></returns>
        public DirectionalLight(Color color, float intensity, Vector3 direction)
        {
            Color = color;
            Intensity = intensity;

            Vector3 normalizedDirection = direction;
            normalizedDirection.Normalize();
            Direction = normalizedDirection;
        }

        public Vector3 Direction
        {
            get { return _direction;}
            set
            {
                _direction = value;
                HasChanged = true;
            }
        }

        public virtual void ApplyShader()
        {
            Shaders.deferredDirectionalLightUnshadowed.Passes[0].Apply();
        }
    }
}
