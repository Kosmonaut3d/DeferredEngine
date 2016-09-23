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
        //ClearGBuffer
        public static Effect ClearGBufferEffect;

        //GBuffer
        public static Effect GBufferEffect;
        public static EffectParameter GBufferEffectParameter_World;
        public static EffectParameter GBufferEffectParameter_WorldViewProj;
        public static EffectParameter GBufferEffectParameter_View;

        public static EffectParameter GBufferEffectParameter_Material_Metalness;
        public static EffectParameter GBufferEffectParameter_Material_DiffuseColor;
        public static EffectParameter GBufferEffectParameter_Material_Roughness;
        public static EffectParameter GBufferEffectParameter_Material_Mask;
        public static EffectParameter GBufferEffectParameter_Material_Texture;
        public static EffectParameter GBufferEffectParameter_Material_NormalMap;
        public static EffectParameter GBufferEffectParameter_Material_Specular;
        public static EffectParameter GBufferEffectParameter_Material_MaterialType;

        public static Effect DeferredCompose;
        public static EffectParameter DeferredComposeEffectParameter_ColorMap;
        public static EffectParameter DeferredComposeEffectParameter_diffuseLightMap;
        public static EffectParameter DeferredComposeEffectParameter_specularLightMap;
        public static EffectParameter DeferredComposeEffectParameter_skullMap;

        //Deferred Light
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

        public static EffectParameter deferredPointLightParameterViewProjection;
        public static EffectParameter deferredPointLightParameterCameraPosition;
        public static EffectParameter deferredPointLightParameterInverseViewProjection;

        public static EffectParameter deferredPointLightParameter_World;
        public static EffectParameter deferredPointLightParameter_LightPosition;
        public static EffectParameter deferredPointLightParameter_LightColor;
        public static EffectParameter deferredPointLightParameter_LightRadius;
        public static EffectParameter deferredPointLightParameter_LightIntensity;
        public static EffectParameter deferredPointLightParameter_Inside;

        public static EffectParameter deferredPointLightParameter_AlbedoMap;
        public static EffectParameter deferredPointLightParameter_NormalMap;
        public static EffectParameter deferredPointLightParameter_DepthMap;


        //SSR

        public static Effect SSReflectionEffect;
        public static EffectParameter SSReflectionEffectParameter_CameraPosition;
        public static EffectParameter SSReflectionEffectParameter_InvertViewProjection;
        public static EffectParameter SSReflectionEffectParameter_Projection;
        public static EffectParameter SSReflectionEffectParameter_ViewProjection;
        public static EffectParameter SSReflectionEffectParameter_DepthMap;
        public static EffectParameter SSReflectionEffectParameter_NormalMap;
        public static EffectParameter SSReflectionEffectParameter_AlbedoMap;
        public static EffectParameter SSReflectionEffectParameter_Resolution;

        public static void Load(ContentManager content)
        {
            ClearGBufferEffect = content.Load<Effect>("ClearGBuffer");

            //Gbuffer
            GBufferEffect = content.Load<Effect>("GBuffer");

            GBufferEffectParameter_World = GBufferEffect.Parameters["World"];
            GBufferEffectParameter_WorldViewProj = GBufferEffect.Parameters["WorldViewProj"];
            GBufferEffectParameter_View = GBufferEffect.Parameters["View"];

            GBufferEffectParameter_Material_Metalness = GBufferEffect.Parameters["Metalness"];
            GBufferEffectParameter_Material_DiffuseColor = GBufferEffect.Parameters["DiffuseColor"];
            GBufferEffectParameter_Material_Roughness = GBufferEffect.Parameters["Roughness"];

            GBufferEffectParameter_Material_Mask = GBufferEffect.Parameters["Mask"];
            GBufferEffectParameter_Material_Texture = GBufferEffect.Parameters["Texture"];
            GBufferEffectParameter_Material_NormalMap = GBufferEffect.Parameters["NormalMap"];
            GBufferEffectParameter_Material_Specular = GBufferEffect.Parameters["Specular"];

            GBufferEffectParameter_Material_MaterialType = GBufferEffect.Parameters["MaterialType"];

            //DeferredCompose

            DeferredCompose = content.Load<Effect>("DeferredCompose");

            DeferredComposeEffectParameter_ColorMap = DeferredCompose.Parameters["colorMap"];
            DeferredComposeEffectParameter_diffuseLightMap = DeferredCompose.Parameters["diffuseLightMap"];
            DeferredComposeEffectParameter_specularLightMap = DeferredCompose.Parameters["specularLightMap"];
            DeferredComposeEffectParameter_skullMap = DeferredCompose.Parameters["skull"];

            //DeferredLights

            deferredSpotLight = content.Load<Effect>("DeferredSpotLight");

            deferredSpotLightUnshadowed = deferredSpotLight.Techniques["Unshadowed"];
            deferredSpotLightShadowed = deferredSpotLight.Techniques["Shadowed"];

            deferredSpotLightParameterShadowMap = deferredSpotLight.Parameters["shadowMap"];

            //PL
            deferredPointLight = content.Load<Effect>("DeferredPointLight");

            deferredPointLightUnshadowed = deferredPointLight.Techniques["Unshadowed"];
            deferredPointLightShadowed = deferredPointLight.Techniques["Shadowed"];

            deferredPointLightParameterShadowMap = deferredPointLight.Parameters["shadowMap"];

            deferredPointLightParameterShadowMap = deferredPointLight.Parameters["shadowCubeMap"];
            deferredPointLightParameterLightViewProjectionPositiveX = deferredPointLight.Parameters["LightViewProjectionPositiveX"];
            deferredPointLightParameterLightViewProjectionNegativeX = deferredPointLight.Parameters["LightViewProjectionNegativeX"];
            deferredPointLightParameterLightViewProjectionPositiveY = deferredPointLight.Parameters["LightViewProjectionPositiveY"];
            deferredPointLightParameterLightViewProjectionNegativeY = deferredPointLight.Parameters["LightViewProjectionNegativeY"];
            deferredPointLightParameterLightViewProjectionPositiveZ = deferredPointLight.Parameters["LightViewProjectionPositiveZ"];
            deferredPointLightParameterLightViewProjectionNegativeZ = deferredPointLight.Parameters["LightViewProjectionNegativeZ"];

            deferredPointLightParameterViewProjection = deferredPointLight.Parameters["ViewProjection"];
            deferredPointLightParameterCameraPosition = deferredPointLight.Parameters["cameraPosition"];
            deferredPointLightParameterInverseViewProjection = deferredPointLight.Parameters["InvertViewProjection"];

            deferredPointLightParameter_World = deferredPointLight.Parameters["World"];
            deferredPointLightParameter_LightPosition = deferredPointLight.Parameters["lightPosition"];
            deferredPointLightParameter_LightColor = deferredPointLight.Parameters["lightColor"];
            deferredPointLightParameter_LightRadius = deferredPointLight.Parameters["lightRadius"];
            deferredPointLightParameter_LightIntensity = deferredPointLight.Parameters["lightIntensity"];
            deferredPointLightParameter_Inside = deferredPointLight.Parameters["inside"];

            deferredPointLightParameter_AlbedoMap = deferredPointLight.Parameters["AlbedoMap"];
            deferredPointLightParameter_NormalMap = deferredPointLight.Parameters["NormalMap"];
            deferredPointLightParameter_DepthMap = deferredPointLight.Parameters["DepthMap"];


            //SSReflections
            SSReflectionEffect = content.Load<Effect>("SSReflectionEffect");

            SSReflectionEffectParameter_InvertViewProjection = SSReflectionEffect.Parameters["InvertViewProjection"];
            SSReflectionEffectParameter_ViewProjection = SSReflectionEffect.Parameters["ViewProjection"];
            SSReflectionEffectParameter_Projection = SSReflectionEffect.Parameters["Projection"];
            SSReflectionEffectParameter_CameraPosition = SSReflectionEffect.Parameters["cameraPosition"];

            SSReflectionEffectParameter_DepthMap = SSReflectionEffect.Parameters["depthMap"];
            SSReflectionEffectParameter_NormalMap = SSReflectionEffect.Parameters["normalMap"];
            SSReflectionEffectParameter_AlbedoMap = SSReflectionEffect.Parameters["albedoMap"];

            SSReflectionEffectParameter_Resolution = SSReflectionEffect.Parameters["resolution"];
        }
    }
}
