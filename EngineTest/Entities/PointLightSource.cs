using EngineTest.Entities;
using EngineTest.Recources.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
    public class PointLightSource : TransformableObject
    {
        private Vector3 _position;
        public float _radius;
        public Color Color;
        public float Intensity;

        public bool HasChanged = true;

        public int ShadowResolution;
        public bool StaticShadows;

        private int _id;

        public RenderTargetCube shadowMapCube;

        public Matrix LightViewProjectionPositiveX;
        public Matrix LightViewProjectionNegativeX;
        public Matrix LightViewProjectionPositiveY;
        public Matrix LightViewProjectionNegativeY;
        public Matrix LightViewProjectionPositiveZ;
        public Matrix LightViewProjectionNegativeZ;

        public BoundingSphere BoundingSphere;

        public bool DrawShadow = false;
        public bool IsVolumetric;

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
        public PointLightSource(Vector3 position, float radius, Color color, float intensity, bool drawShadow, bool isVolumetric, int shadowResolution, bool staticShadow)
        {
            BoundingSphere = new BoundingSphere(position, radius);
            Position = position;
            Radius = radius;
            Color = color;
            Intensity = intensity;
            DrawShadow = drawShadow;
            IsVolumetric = isVolumetric;

            ShadowResolution = shadowResolution;
            StaticShadows = staticShadow;

            Id = IdGenerator.GetNewId();

        }

        public override Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                BoundingSphere.Center = value;
                HasChanged = true;
            }
        }

        public override int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public override double AngleZ { get; set; }
        public override double AngleX { get; set; }
        public override double AngleY { get; set; }

        public override TransformableObject Clone
        {
            get { return new PointLightSource(Position, Radius, Color, Intensity, DrawShadow, IsVolumetric, ShadowResolution, StaticShadows);}
        }

        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                BoundingSphere.Radius = value;
                HasChanged = true;
            }
        }


        protected PointLightSource()
        {

        }

        public virtual void ApplyShader()
        {
            if (shadowMapCube != null)
            {
                Shaders.deferredPointLightParameterShadowMap.SetValue(shadowMapCube);

                Shaders.deferredPointLightParameterLightViewProjectionPositiveX.SetValue(LightViewProjectionPositiveX);
                Shaders.deferredPointLightParameterLightViewProjectionNegativeX.SetValue(LightViewProjectionNegativeX);

                Shaders.deferredPointLightParameterLightViewProjectionPositiveY.SetValue(LightViewProjectionPositiveY);
                Shaders.deferredPointLightParameterLightViewProjectionNegativeY.SetValue(LightViewProjectionNegativeY);

                Shaders.deferredPointLightParameterLightViewProjectionPositiveZ.SetValue(LightViewProjectionPositiveZ);
                Shaders.deferredPointLightParameterLightViewProjectionNegativeZ.SetValue(LightViewProjectionNegativeZ);

                Shaders.deferredPointLightShadowed.Passes[0].Apply();
            }
            else
            {
                if (IsVolumetric && GameSettings.g_VolumetricLights)
                {
                    Shaders.deferredPointLightUnshadowedVolumetric.Passes[0].Apply();
                }
                else
                {
                    Shaders.deferredPointLightUnshadowed.Passes[0].Apply();
                }
            }
        }
    }
    
}
