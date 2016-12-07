using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
    public class SpotLightSource : PointLightSource
    {
        public Vector3 Direction;

        public RenderTarget2D RenderTargetShadowMap;
        public RenderTargetBinding[] RenderTargetShadowMapBinding = new RenderTargetBinding[1];
        public Matrix LightViewProjection;

        public SpotLightSource(Vector3 position, float radius, Color color, float intensity, Vector3 direction, bool drawShadow) : base()
        {
            Position = position;
            Radius = radius;
            Color = color;
            Intensity = intensity;
            Direction = direction;
            DrawShadow = drawShadow;
        }

        public override void ApplyShader(Matrix InverseView)
        {
            throw new NotImplementedException();

            //if (RenderTargetShadowMap != null)
            //{
            //    Shaders.deferredSpotLightParameterShadowMap.SetValue(RenderTargetShadowMap);
            //    Shaders.deferredSpotLightShadowed.Passes[0].Apply();
            //}
            //else
            //{
            //    Shaders.deferredSpotLightUnshadowed.Passes[0].Apply();
            //}
        }
    }
}
