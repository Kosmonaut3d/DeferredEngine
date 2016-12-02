using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
    public class DirectionalLight
    {
        public Color Color;
        public float Intensity;
        private Vector3 _direction;
        private Vector3 _position;
        public bool HasChanged;

        public bool DrawShadows;
        public float ShadowSize;
        public float ShadowDepth;
        public int ShadowResolution;
        public bool StaticShadow;

        public RenderTarget2D ShadowMap;
        public Matrix ShadowViewProjection;

        public ShadowFilteringTypes ShadowFiltering;
        public bool ScreenSpaceShadowBlur;

        public Matrix LightViewProjection;

        public enum ShadowFilteringTypes
        {
            PCF, SoftPCF3x, SoftPCF5x, Poisson, VSM
        };

        /// <summary>
        /// Create a Directional light, shadows are optional
        /// </summary>
        /// <param name="color"></param>
        /// <param name="intensity"></param>
        /// <param name="direction"></param>
        /// <param name="drawShadows"></param>
        /// <param name="shadowSize"></param>
        /// <param name="shadowDepth"></param>
        /// <param name="shadowResolution"></param>
        public DirectionalLight(Color color, float intensity, Vector3 direction,Vector3 position = default(Vector3), bool drawShadows = false, float shadowSize = 100, float shadowDepth = 100, int shadowResolution = 512, ShadowFilteringTypes shadowFiltering = ShadowFilteringTypes.Poisson, bool screenspaceshadowblur = false, bool staticshadows = false)
        {
            Color = color;
            Intensity = intensity;

            Vector3 normalizedDirection = direction;
            normalizedDirection.Normalize();
            Direction = normalizedDirection;

            DrawShadows = drawShadows;

            DrawShadows = drawShadows;
            ShadowSize = shadowSize;
            ShadowDepth = shadowDepth;
            ShadowResolution = shadowResolution;
            StaticShadow = staticshadows;

            ScreenSpaceShadowBlur = screenspaceshadowblur;

            ShadowFiltering = shadowFiltering;

            Position = position;
        }

        public Vector3 Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                HasChanged = true;
            }
        }

        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                HasChanged = true;
            }
        }

        public virtual void ApplyShader()
        {
            if (DrawShadows)
            {
                //Shaders.deferredDirectionalLightParameterLightViewProjection.SetValue(LightViewProjection);
                //Shaders.deferredDirectionalLightParameter_ShadowMap.SetValue(shadowMap);
                if (ScreenSpaceShadowBlur)
                {
                    Shaders.deferredDirectionalLightParameterLightViewProjection.SetValue(LightViewProjection);
                    Shaders.deferredDirectionalLightParameter_ShadowFiltering.SetValue((int)ShadowFiltering);
                    Shaders.deferredDirectionalLightSSShadowed.Passes[0].Apply();  
                }
                else
                {
                    Shaders.deferredDirectionalLightParameterLightViewProjection.SetValue(LightViewProjection);
                    Shaders.deferredDirectionalLightParameter_ShadowMap.SetValue(ShadowMap);
                    Shaders.deferredDirectionalLightParameter_ShadowFiltering.SetValue((int)ShadowFiltering);
                    Shaders.deferredDirectionalLightParameter_ShadowMapSize.SetValue((float)ShadowResolution);
                    Shaders.deferredDirectionalLightShadowed.Passes[0].Apply();   
                }
            }
            else
            {
                Shaders.deferredDirectionalLightUnshadowed.Passes[0].Apply();  
            }
        }
    }
}
