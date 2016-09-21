using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
    public static class Shaders
    {
        public static Effect deferredSpotLight;
        public static EffectTechnique deferredSpotLightUnshadowed;
        public static EffectTechnique deferredSpotLightShadowed;
        public static EffectParameter deferredSpotLightParameterShadowMap;

        public static Effect deferredPointLight;
        public static EffectTechnique deferredPointLightUnshadowed;
        public static EffectTechnique deferredPointLightShadowed;
        public static EffectParameter deferredPointLightParameterShadowMap;
        public static EffectParameter deferredPointLightParameterLightViewProjectionPositiveX;
        public static EffectParameter deferredPointLightParameterLightViewProjectionNegativeX;
        public static EffectParameter deferredPointLightParameterLightViewProjectionPositiveY;
        public static EffectParameter deferredPointLightParameterLightViewProjectionNegativeY;
        public static EffectParameter deferredPointLightParameterLightViewProjectionPositiveZ;
        public static EffectParameter deferredPointLightParameterLightViewProjectionNegativeZ;

        public static Effect ssao;
        public static EffectParameter ssaoCameraPosition;
        public static EffectParameter ssaoInvertViewProjection;
        public static EffectParameter ssaoProjection;
        public static EffectParameter ssaoViewProjection;
        public static EffectParameter ssaoDepthMap;
        public static EffectParameter ssaoNormalMap;
        public static EffectParameter ssaoAlbedoMap;

        public static void Load(ContentManager content)
        {
            deferredSpotLight = content.Load<Effect>("DeferredSpotLight");

            deferredSpotLightUnshadowed = deferredSpotLight.Techniques["Unshadowed"];
            deferredSpotLightShadowed = deferredSpotLight.Techniques["Shadowed"];

            deferredSpotLightParameterShadowMap = deferredSpotLight.Parameters["shadowMap"];

            deferredPointLight = content.Load<Effect>("DeferredPointLight");

            deferredPointLightUnshadowed = deferredPointLight.Techniques["Unshadowed"];
            deferredPointLightShadowed = deferredPointLight.Techniques["Shadowed"];

            deferredPointLightParameterShadowMap = deferredPointLight.Parameters["shadowCubeMap"];
            deferredPointLightParameterLightViewProjectionPositiveX = deferredPointLight.Parameters["LightViewProjectionPositiveX"];
            deferredPointLightParameterLightViewProjectionNegativeX = deferredPointLight.Parameters["LightViewProjectionNegativeX"];
            deferredPointLightParameterLightViewProjectionPositiveY = deferredPointLight.Parameters["LightViewProjectionPositiveY"];
            deferredPointLightParameterLightViewProjectionNegativeY = deferredPointLight.Parameters["LightViewProjectionNegativeY"];
            deferredPointLightParameterLightViewProjectionPositiveZ = deferredPointLight.Parameters["LightViewProjectionPositiveZ"];
            deferredPointLightParameterLightViewProjectionNegativeZ = deferredPointLight.Parameters["LightViewProjectionNegativeZ"];


            ssao = content.Load<Effect>("SSAO");

            ssaoInvertViewProjection = ssao.Parameters["InvertViewProjection"];
            ssaoViewProjection = ssao.Parameters["ViewProjection"];
            ssaoProjection = ssao.Parameters["Projection"];
            ssaoCameraPosition = ssao.Parameters["cameraPosition"];

            ssaoDepthMap = ssao.Parameters["depthMap"];
            ssaoNormalMap = ssao.Parameters["normalMap"];
            ssaoAlbedoMap = ssao.Parameters["albedoMap"];
        }
    }
}
