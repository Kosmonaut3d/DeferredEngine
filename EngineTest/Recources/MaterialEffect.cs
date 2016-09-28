using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
    public class MaterialEffect : Effect, IEquatable<MaterialEffect>
    {
        private Texture2D _albedoMap;
        private Texture2D _roughnessMap;
        private Texture2D _mask;
        private Texture2D _normalMap;
        private Texture2D _metallicMap;

        public bool IsTransparent = false;

        public bool HasShadow = true;

        public bool HasDiffuse = false;
        public bool HasRoughness = false;
        public bool HasMask = false;
        public bool HasNormal = false;
        public bool HasMetallic = false;

        public int MaterialType = 0;

        public Vector3 DiffuseColor;

        public float Roughness = 0.5f;

        public float Metallic = 0;
        public float EmissiveStrength = 0;


        public Texture2D AlbedoMap
        {
            get { return _albedoMap; }
            set
            {
                if (value == null) return; 
                _albedoMap = value;
                HasDiffuse = true;
            }
        }

        public Texture2D RoughnessMap
        {
            get { return _roughnessMap; }
            set
            {
                if (value == null) return; 
                _roughnessMap = value;
                HasRoughness = true;
            }
        }

        public Texture2D MetallicMap
        {
            get { return _metallicMap; }
            set
            {
                if (value == null) return; 
                _metallicMap = value;
                HasMetallic = true;
            }
        }

        public Texture2D NormalMap
        {
            get { return _normalMap; }
            set
            {
                if (value == null) return; 
                _normalMap = value;
                HasNormal = true;
            }
        }

        public Texture2D Mask
        {
            get { return _mask; }
            set
            {
                if (value == null) return; 
                _mask = value;
                HasMask = true;
            }
        }

        public void Initialize(Color diffuseColor, float roughness, float metalness, Texture2D albedoMap = null, Texture2D normalMap = null, Texture2D roughnessMap = null, Texture2D metallicMap = null, Texture2D mask = null, int type = 0, float emissiveStrength = 0)
        {
            DiffuseColor = diffuseColor.ToVector3();
            Roughness = roughness;
            Metallic = metalness;

            AlbedoMap = albedoMap;
            NormalMap = normalMap;
            RoughnessMap = roughnessMap;
            MetallicMap = metallicMap;
            Mask = mask;
            MaterialType = type;

            if (emissiveStrength > 0)
            {
                MaterialType = 3;
                EmissiveStrength = emissiveStrength;
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


        public bool Equals(MaterialEffect b)
        {
            if (b==null) return false;

            if (HasDiffuse != b.HasDiffuse) return false;

            if (HasRoughness != b.HasRoughness) return false;

            if (IsTransparent != b.IsTransparent) return false;

            if (HasMask != b.HasMask) return false;

            if (HasNormal != b.HasNormal) return false;

            if (HasShadow != b.HasShadow) return false;

            if (Vector3.DistanceSquared(DiffuseColor, b.DiffuseColor) > 0.01f) return false;

            if (AlbedoMap != b.AlbedoMap) return false;

            if (MaterialType != b.MaterialType) return false;

            if (Math.Abs(Roughness - b.Roughness) > 0.01f) return false;

            if (Math.Abs(Metallic - b.Metallic) > 0.01f) return false;

            if (AlbedoMap != b.AlbedoMap) return false;

            if (NormalMap != b.NormalMap) return false;

            return true;
        }
    }
}
