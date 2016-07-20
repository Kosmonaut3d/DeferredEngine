using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
    class MaterialEffect : Effect
    {
        private Texture2D _diffuse;
        private Texture2D _specular;
        private Texture2D _mask;
        private Texture2D _normal;

        public bool HasDiffuse = false;
        public bool HasSpecular = false;
        public bool HasMask = false;
        public bool HasNormal = false;

        public Vector3 DiffuseColor;


        public Texture2D Diffuse
        {
            get { return _diffuse; }
            set { _diffuse = value;
                HasDiffuse = true;
            }
        }

        public Texture2D Specular
        {
            get { return _specular; }
            set
            {
                _specular = value;
                HasSpecular = true;
            }
        }

        public Texture2D Normal
        {
            get { return _normal; }
            set
            {
                _normal = value;
                HasNormal = true;
            }
        }

        public Texture2D Mask
        {
            get { return _mask; }
            set
            {
                _mask = value;
                HasMask = true;
            }
        }

        public MaterialEffect(Effect cloneSource) : base(cloneSource)
        {
        }

        public MaterialEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
        }

        public MaterialEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        {
        }
    }
}
