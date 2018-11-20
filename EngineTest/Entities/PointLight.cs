using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.RenderModules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Entities
{
    public class PointLight : TransformableObject
    {
        private Vector3 _position = Vector3.Zero;
        public Matrix WorldMatrix;
        private float _radius;
        private Color _color;
        public Vector3 ColorV3;
        public float Intensity;
        public int ShadowMapRadius = 3;

        public bool HasChanged = true;

        public readonly int ShadowResolution;
        public readonly bool StaticShadows;

        public RenderTarget2D ShadowMap;

        public Matrix LightViewProjectionPositiveX;
        public Matrix LightViewProjectionNegativeX;
        public Matrix LightViewProjectionPositiveY;
        public Matrix LightViewProjectionNegativeY;
        public Matrix LightViewProjectionPositiveZ;
        public Matrix LightViewProjectionNegativeZ;

        public int[] faceBlurCount = {0, 0, 0, 0, 0, 0};

        public Matrix LightViewSpace;
        public Matrix LightWorldViewProj;

        public BoundingSphere BoundingSphere;

        public bool CastShadows;
        public bool CastSDFShadows;
        public int SoftShadowBlurAmount = 0;

        public readonly bool IsVolumetric;
        public readonly float LightVolumeDensity = 1;


        /// <summary>
        /// A point light is a light that shines in all directions
        /// </summary>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        /// <param name="intensity"></param>
        /// <param name="isVolumetric"></param>
        /// <param name="shadowResolution">shadow map resolution per face. Optional</param>
        /// <param name="staticShadow">if set to true the shadows will not update at all. Dynamic shadows in contrast update only when needed.</param>
        /// <param name="castShadows"></param>
        /// <param name="volumeDensity"></param>
        /// <returns></returns>
        public PointLight(Vector3 position, float radius, Color color, float intensity, bool castShadows, bool isVolumetric, int shadowResolution, int softShadowBlurAmount, bool staticShadow, float volumeDensity = 1, bool isEnabled = true)
        {
            BoundingSphere = new BoundingSphere(position, radius);
            Position = position;
            Radius = radius;
            Color = color;
            Intensity = intensity;
            CastShadows = castShadows;
            IsVolumetric = isVolumetric;
            SoftShadowBlurAmount = softShadowBlurAmount;

            ShadowResolution = shadowResolution;
            StaticShadows = staticShadow;
            LightVolumeDensity = volumeDensity;
            IsEnabled = isEnabled;
            Id = IdGenerator.GetNewId();
            Name = GetType().Name + " " + Id;
        }

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                ColorV3 =  (_color.ToVector3().Pow(2.2f));
            }
        }

        public override Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                BoundingSphere.Center = value;
                WorldMatrix = Matrix.CreateScale(Radius * 1.1f) * Matrix.CreateTranslation(Position);
                HasChanged = true;
            }
        }

        public override Vector3 Scale { get; set; }

        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                BoundingSphere.Radius = value;
                WorldMatrix = Matrix.CreateScale(Radius * 1.1f) * Matrix.CreateTranslation(Position);
                HasChanged = true;
            }
        }

        public override int Id { get; set; }
        public override Matrix RotationMatrix { get; set; }

        public sealed override bool IsEnabled
        {
            get;
            set;
        }

        public override TransformableObject Clone
        {
            get { return new PointLight(Position, Radius, Color, Intensity, CastShadows, IsVolumetric, ShadowResolution, SoftShadowBlurAmount, StaticShadows);}
        }

        public override string Name { get; set; }

        protected PointLight()
        {

        }

        public virtual void ApplyShader(Matrix inverseView)
        {
            

        }
    }
    
}
