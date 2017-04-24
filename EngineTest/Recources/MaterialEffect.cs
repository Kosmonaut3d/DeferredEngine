
//#define FORWARDONLY

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DeferredEngine.Recources
{
    public class MaterialEffect : Effect, IEquatable<MaterialEffect>
    {
        private Texture2D _albedoMap;
        private Texture2D _roughnessMap;
        private Texture2D _mask;
        private Texture2D _normalMap;
        private Texture2D _metallicMap;
        private Texture2D _displacementMap;

        public bool IsTransparent = false;

        public bool HasShadow = true;

        public bool HasDiffuse;
        public bool HasRoughnessMap;
        public bool HasMask;
        public bool HasNormalMap;
        public bool HasMetallic;
        public bool HasDisplacement;


        public Vector3 DiffuseColor = Color.Gray.ToVector3();

        private float _roughness = 0.5f;

        public float Metallic;
        public float EmissiveStrength;

        public float Roughness { get { return _roughness;} set { _roughness = Math.Max(value, 0.001f); } }
        
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
                HasRoughnessMap = true;
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
                HasNormalMap = true;
            }
        }

        public Texture2D DisplacementMap
        {
            get { return _displacementMap; }
            set
            {
                if (value == null) return;
                _displacementMap = value;
                HasDisplacement = true;
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
        
        private MaterialTypes _type = MaterialTypes.Basic;
        public int MaterialTypeNumber;
        public bool RenderCClockwise = false;

        public enum MaterialTypes
        {
            Basic = 0,
            Emissive = 3,
            Hologram = 1,
            ProjectHologram = 2,
            SubsurfaceScattering = 4,
            ForwardShaded = 5,
        }

        public MaterialTypes Type
        {
            get { return _type; }
            set
            {
                _type = value;
                MaterialTypeNumber = (int) value;
            }
        }


        public void Initialize(Color diffuseColor, float roughness, float metalness, Texture2D albedoMap = null, Texture2D normalMap = null, Texture2D roughnessMap = null, Texture2D metallicMap = null, Texture2D mask = null, Texture2D displacementMap = null, MaterialTypes type = MaterialTypes.Basic, float emissiveStrength = 0)
        {
            DiffuseColor = diffuseColor.ToVector3();
            Roughness = roughness;
            Metallic = metalness;

            AlbedoMap = albedoMap;
            NormalMap = normalMap;
            RoughnessMap = roughnessMap;
            MetallicMap = metallicMap;
            DisplacementMap = displacementMap;
            Mask = mask;

            Type = type;

#if FORWARDONLY
            Type = MaterialTypes.ForwardShaded;
#endif

            if (emissiveStrength > 0)
            {
                //Type = MaterialTypes.Emissive;
                EmissiveStrength = emissiveStrength;
            }
        }

        public MaterialEffect(Effect cloneSource) : base(cloneSource)
        {
#if FORWARDONLY
            Type = MaterialTypes.ForwardShaded;
#endif
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

            if (HasRoughnessMap != b.HasRoughnessMap) return false;

            if (IsTransparent != b.IsTransparent) return false;

            if (HasMask != b.HasMask) return false;

            if (HasNormalMap != b.HasNormalMap) return false;

            if (HasShadow != b.HasShadow) return false;

            if (HasDisplacement != b.HasDisplacement) return false;

            if (Vector3.DistanceSquared(DiffuseColor, b.DiffuseColor) > 0.01f) return false;

            if (AlbedoMap != b.AlbedoMap) return false;

            if (Type != b.Type) return false;

            if (Math.Abs(Roughness - b.Roughness) > 0.01f) return false;

            if (Math.Abs(Metallic - b.Metallic) > 0.01f) return false;

            if (AlbedoMap != b.AlbedoMap) return false;

            if (NormalMap != b.NormalMap) return false;

            return true;
        }

        public MaterialEffect Clone()
        {
            return new MaterialEffect(this);

        }
    }
}
