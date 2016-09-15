using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
    public class SpotLight : PointLight
    {
        public Vector3 Direction;

        public RenderTarget2D RenderTargetShadowMap;
        public RenderTargetBinding[] RenderTargetShadowMapBinding = new RenderTargetBinding[1];

        public SpotLight(Vector3 position, float radius, Color color, float intensity, Vector3 direction, bool drawShadow) : base()
        {
            Position = position;
            Radius = radius;
            Color = color;
            Intensity = intensity;
            Direction = direction;
            DrawShadow = drawShadow;
        }

        public void ApplyShader()
        {
            if (RenderTargetShadowMap != null)
            {
                Shaders.deferredSpotLightShadowed.Passes[0].Apply();
                Shaders.deferredSpotLightParameterShadowMap.SetValue(RenderTargetShadowMap);
            }
            else
            {
                Shaders.deferredSpotLightUnshadowed.Passes[0].Apply();
            }
        }
    }
}
