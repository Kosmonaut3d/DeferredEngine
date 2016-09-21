using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
    public class PointLight
    {
        public Vector3 Position;
        public float Radius;
        public Color Color;
        public float Intensity;

        public RenderTargetCube shadowMapCube;

        public Matrix LightViewProjectionPositiveX;
        public Matrix LightViewProjectionNegativeX;
        public Matrix LightViewProjectionPositiveY;
        public Matrix LightViewProjectionNegativeY;
        public Matrix LightViewProjectionPositiveZ;
        public Matrix LightViewProjectionNegativeZ;

        public bool DrawShadow = false;

        public PointLight(Vector3 position, float radius, Color color, float intensity, bool drawShadow)
        {
            Position = position;
            Radius = radius;
            Color = color;
            Intensity = intensity;
            DrawShadow = drawShadow;
        }

        protected PointLight()
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
                Shaders.deferredPointLightUnshadowed.Passes[0].Apply();
            }
        }
    }
}
