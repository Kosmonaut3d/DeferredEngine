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

        //Emissive

        public static Effect EmissiveEffect;
        public static EffectParameter EmissiveEffectParameter_ViewProj;
        public static EffectParameter EmissiveEffectParameter_WorldViewProj;
        public static EffectParameter EmissiveEffectParameter_InvertViewProj;
        public static EffectParameter EmissiveEffectParameter_World;
        public static EffectParameter EmissiveEffectParameter_Origin;
        public static EffectParameter EmissiveEffectParameter_Size;
        public static EffectParameter EmissiveEffectParameter_EmissiveColor;
        public static EffectParameter EmissiveEffectParameter_EmissiveStrength;
        public static EffectParameter EmissiveEffectParameter_CameraPosition;

        public static EffectParameter EmissiveEffectParameter_Resolution;

        public static EffectParameter EmissiveEffectParameter_DepthMap;
        public static EffectParameter EmissiveEffectParameter_EmissiveMap;
        public static EffectParameter EmissiveEffectParameter_NormalMap;

        public static EffectTechnique EmissiveEffectTechnique_DrawEmissiveBuffer;
        public static EffectTechnique EmissiveEffectTechnique_DrawEmissiveDiffuseEffect;
        public static EffectTechnique EmissiveEffectTechnique_DrawEmissiveSpecularEffect;
        //Second Screen Space Effect

        public static Effect ScreenSpaceEffect2;
        public static EffectParameter ScreenSpaceEffect2Parameter_DepthMap;
        public static EffectParameter ScreenSpaceEffect2Parameter_TargetMap;
        public static EffectParameter ScreenSpaceEffect2Parameter_NormalMap;
        public static EffectParameter ScreenSpaceEffect2Parameter_ViewProjection;
        public static EffectParameter ScreenSpaceEffect2Parameter_InverseViewProjection;

        //Screen Space Effect

        public static Effect ScreenSpaceEffect;
        public static EffectParameter ScreenSpaceEffectParameter_SSAOMap;
        public static EffectParameter ScreenSpaceEffectParameter_NormalMap;
        public static EffectParameter ScreenSpaceEffectParameter_DepthMap;
        public static EffectParameter ScreenSpaceEffectParameter_CameraPosition;
        public static EffectParameter ScreenSpaceEffectParameter_InverseViewProjection;
        public static EffectParameter ScreenSpaceEffectParameter_Projection;
        public static EffectParameter ScreenSpaceEffectParameter_ViewProjection;

        public static EffectParameter ScreenSpaceEffect_FalloffMin;
        public static EffectParameter ScreenSpaceEffect_FalloffMax;
        public static EffectParameter ScreenSpaceEffect_Samples;
        public static EffectParameter ScreenSpaceEffect_Strength;
        public static EffectParameter ScreenSpaceEffect_SampleRadius;
        public static EffectParameter ScreenSpaceEffectParameter_InverseResolution;

        public static EffectTechnique ScreenSpaceEffectTechnique_SSAO;
        public static EffectTechnique ScreenSpaceEffectTechnique_BlurHorizontal;
        public static EffectTechnique ScreenSpaceEffectTechnique_BlurVertical;

        //ClearGBuffer
        public static Effect ClearGBufferEffect;

        //GBuffer
        public static Effect GBufferEffect;
        public static EffectParameter GBufferEffectParameter_World;
        public static EffectParameter GBufferEffectParameter_WorldViewProj;
        public static EffectParameter GBufferEffectParameter_View;

        public static EffectParameter GBufferEffectParameter_Material_Metallic;
        public static EffectParameter GBufferEffectParameter_Material_MetallicMap;
        public static EffectParameter GBufferEffectParameter_Material_DiffuseColor;
        public static EffectParameter GBufferEffectParameter_Material_Roughness;
        public static EffectParameter GBufferEffectParameter_Material_Mask;
        public static EffectParameter GBufferEffectParameter_Material_Texture;
        public static EffectParameter GBufferEffectParameter_Material_NormalMap;
        public static EffectParameter GBufferEffectParameter_Material_Specular;
        public static EffectParameter GBufferEffectParameter_Material_MaterialType;

        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecularNormalMask;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureNormalMask;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecularMask;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureMask;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecularNormalMetallic;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecularNormal;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureNormal;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecular;
        public static EffectTechnique GBufferEffectTechniques_DrawTexture;
        public static EffectTechnique GBufferEffectTechniques_DrawBasic;

        //COMPOSE

        public static Effect DeferredCompose;
        public static EffectParameter DeferredComposeEffectParameter_ColorMap;
        public static EffectParameter DeferredComposeEffectParameter_diffuseLightMap;
        public static EffectParameter DeferredComposeEffectParameter_specularLightMap;
        public static EffectParameter DeferredComposeEffectParameter_skullMap;
        public static EffectParameter DeferredComposeEffectParameter_SSAOMap;

        //Deferred Light
        public static Effect deferredSpotLight;
        public static EffectTechnique deferredSpotLightUnshadowed;
        public static EffectTechnique deferredSpotLightShadowed;
        public static EffectParameter deferredSpotLightParameterShadowMap;

        public static Effect deferredPointLight;
        public static EffectTechnique deferredPointLightUnshadowed;
        public static EffectTechnique deferredPointLightShadowed;
        public static EffectParameter deferredPointLightParameterResolution;
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

        //DeferredEnvironment


        public static Effect deferredEnvironment;
        public static EffectParameter deferredEnvironmentParameter_AlbedoMap;
        public static EffectParameter deferredEnvironmentParameter_NormalMap;
        public static EffectParameter deferredEnvironmentParameter_DepthMap;
        public static EffectParameter deferredEnvironmentParameterCameraPosition;
        public static EffectParameter deferredEnvironmentParameterInverseViewProjection;

        //SHADOW MAPPING

        public static Effect virtualShadowMappingEffect;
        public static EffectParameter virtualShadowMappingEffectParameter_WorldViewProj;

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
            //Emissive Effect
            EmissiveEffect = content.Load<Effect>("Shaders/Emissive/EmissiveDraw");
            EmissiveEffectParameter_World = EmissiveEffect.Parameters["World"];
            EmissiveEffectParameter_ViewProj = EmissiveEffect.Parameters["ViewProjection"];
            EmissiveEffectParameter_WorldViewProj = EmissiveEffect.Parameters["WorldViewProj"];
            EmissiveEffectParameter_InvertViewProj = EmissiveEffect.Parameters["InvertViewProjection"];
            EmissiveEffectParameter_Origin = EmissiveEffect.Parameters["Origin"];
            EmissiveEffectParameter_CameraPosition = EmissiveEffect.Parameters["CameraPosition"];
            EmissiveEffectParameter_Size = EmissiveEffect.Parameters["Size"];
            EmissiveEffectParameter_NormalMap = EmissiveEffect.Parameters["NormalMap"];
            EmissiveEffectParameter_DepthMap = EmissiveEffect.Parameters["DepthMap"];
            EmissiveEffectParameter_EmissiveMap = EmissiveEffect.Parameters["EmissiveMap"];
            EmissiveEffectParameter_Resolution = EmissiveEffect.Parameters["Resolution"];
            EmissiveEffectParameter_EmissiveColor = EmissiveEffect.Parameters["EmissiveColor"];
            EmissiveEffectParameter_EmissiveStrength = EmissiveEffect.Parameters["EmissiveStrength"];

            EmissiveEffectTechnique_DrawEmissiveBuffer = EmissiveEffect.Techniques["DrawEmissiveBuffer"];
            EmissiveEffectTechnique_DrawEmissiveSpecularEffect = EmissiveEffect.Techniques["DrawEmissiveSpecularEffect"];
            EmissiveEffectTechnique_DrawEmissiveDiffuseEffect = EmissiveEffect.Techniques["DrawEmissiveDiffuseEffect"];
            //Screen Space Effect 2
            ScreenSpaceEffect2 = content.Load<Effect>("Shaders/ScreenSpaceEffect2");

            ScreenSpaceEffect2Parameter_DepthMap = ScreenSpaceEffect2.Parameters["DepthMap"];
            ScreenSpaceEffect2Parameter_NormalMap = ScreenSpaceEffect2.Parameters["NormalMap"];
            ScreenSpaceEffect2Parameter_TargetMap = ScreenSpaceEffect2.Parameters["TargetMap"];
            ScreenSpaceEffect2Parameter_ViewProjection = ScreenSpaceEffect2.Parameters["ViewProjection"];
            ScreenSpaceEffect2Parameter_InverseViewProjection = ScreenSpaceEffect2.Parameters["InverseViewProjection"];

            //Screen Space Effect
            ScreenSpaceEffect = content.Load<Effect>("Shaders/ScreenSpaceEffect");

            ScreenSpaceEffectParameter_SSAOMap = ScreenSpaceEffect.Parameters["SSAOMap"];
            ScreenSpaceEffectParameter_NormalMap = ScreenSpaceEffect.Parameters["NormalMap"];
            ScreenSpaceEffectParameter_DepthMap = ScreenSpaceEffect.Parameters["DepthMap"];
            ScreenSpaceEffectParameter_CameraPosition = ScreenSpaceEffect.Parameters["CameraPosition"];
            ScreenSpaceEffectParameter_InverseViewProjection = ScreenSpaceEffect.Parameters["InverseViewProjection"];
            ScreenSpaceEffectParameter_Projection = ScreenSpaceEffect.Parameters["Projection"];
            ScreenSpaceEffectParameter_ViewProjection = ScreenSpaceEffect.Parameters["ViewProjection"];

            ScreenSpaceEffect_FalloffMin = ScreenSpaceEffect.Parameters["FalloffMin"];
            ScreenSpaceEffect_FalloffMax = ScreenSpaceEffect.Parameters["FalloffMax"];
            ScreenSpaceEffect_Samples = ScreenSpaceEffect.Parameters["Samples"];
            ScreenSpaceEffect_Strength = ScreenSpaceEffect.Parameters["Strength"];
            ScreenSpaceEffect_SampleRadius = ScreenSpaceEffect.Parameters["SampleRadius"];
            ScreenSpaceEffectParameter_InverseResolution = ScreenSpaceEffect.Parameters["InverseResolution"];

            ScreenSpaceEffectTechnique_SSAO = ScreenSpaceEffect.Techniques["SSAO"];
            ScreenSpaceEffectTechnique_BlurHorizontal = ScreenSpaceEffect.Techniques["BilateralHorizontal"];
            ScreenSpaceEffectTechnique_BlurVertical = ScreenSpaceEffect.Techniques["BilateralVertical"];

            ClearGBufferEffect = content.Load<Effect>("Shaders/ClearGBuffer");

            //Gbuffer
            GBufferEffect = content.Load<Effect>("Shaders/GBuffer");

            GBufferEffectParameter_World = GBufferEffect.Parameters["World"];
            GBufferEffectParameter_WorldViewProj = GBufferEffect.Parameters["WorldViewProj"];
            GBufferEffectParameter_View = GBufferEffect.Parameters["View"];

            GBufferEffectParameter_Material_Metallic = GBufferEffect.Parameters["Metallic"];
            GBufferEffectParameter_Material_MetallicMap = GBufferEffect.Parameters["MetallicMap"];
            GBufferEffectParameter_Material_DiffuseColor = GBufferEffect.Parameters["DiffuseColor"];
            GBufferEffectParameter_Material_Roughness = GBufferEffect.Parameters["Roughness"];

            GBufferEffectParameter_Material_Mask = GBufferEffect.Parameters["Mask"];
            GBufferEffectParameter_Material_Texture = GBufferEffect.Parameters["Texture"];
            GBufferEffectParameter_Material_NormalMap = GBufferEffect.Parameters["NormalMap"];
            GBufferEffectParameter_Material_Specular = GBufferEffect.Parameters["RoughnessMap"];

            GBufferEffectParameter_Material_MaterialType = GBufferEffect.Parameters["MaterialType"];

            //Techniques

            GBufferEffectTechniques_DrawTextureSpecularNormalMask = GBufferEffect.Techniques["DrawTextureSpecularNormalMask"];
            GBufferEffectTechniques_DrawTextureNormalMask = GBufferEffect.Techniques["DrawTextureNormalMask"];
            GBufferEffectTechniques_DrawTextureSpecularMask = GBufferEffect.Techniques["DrawTextureSpecularMask"];
            GBufferEffectTechniques_DrawTextureMask = GBufferEffect.Techniques["DrawTextureMask"];
            GBufferEffectTechniques_DrawTextureSpecularNormalMetallic = GBufferEffect.Techniques["DrawTextureSpecularNormalMetallic"];
            GBufferEffectTechniques_DrawTextureSpecularNormal = GBufferEffect.Techniques["DrawTextureSpecularNormal"];
            GBufferEffectTechniques_DrawTextureNormal = GBufferEffect.Techniques["DrawTextureNormal"];
            GBufferEffectTechniques_DrawTextureSpecular = GBufferEffect.Techniques["DrawTextureSpecular"];
            GBufferEffectTechniques_DrawTexture = GBufferEffect.Techniques["DrawTexture"];
            GBufferEffectTechniques_DrawBasic = GBufferEffect.Techniques["DrawBasic"];


            //DeferredCompose

            DeferredCompose = content.Load<Effect>("Shaders/DeferredCompose");

            DeferredComposeEffectParameter_ColorMap = DeferredCompose.Parameters["colorMap"];
            DeferredComposeEffectParameter_diffuseLightMap = DeferredCompose.Parameters["diffuseLightMap"];
            DeferredComposeEffectParameter_specularLightMap = DeferredCompose.Parameters["specularLightMap"];
            DeferredComposeEffectParameter_skullMap = DeferredCompose.Parameters["skull"];
            DeferredComposeEffectParameter_SSAOMap = DeferredCompose.Parameters["SSAOMap"];


            //DeferredLights

            deferredSpotLight = content.Load<Effect>("Shaders/DeferredSpotLight");

            deferredSpotLightUnshadowed = deferredSpotLight.Techniques["Unshadowed"];
            deferredSpotLightShadowed = deferredSpotLight.Techniques["Shadowed"];

            deferredSpotLightParameterShadowMap = deferredSpotLight.Parameters["shadowMap"];

            //PL
            deferredPointLight = content.Load<Effect>("Shaders/DeferredPointLight");

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

            deferredPointLightParameterResolution = deferredPointLight.Parameters["Resolution"];
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

            //Environment
            deferredEnvironment = content.Load<Effect>("Shaders/DeferredEnvironmentMap");
            deferredEnvironmentParameter_AlbedoMap = deferredEnvironment.Parameters["AlbedoMap"];
            deferredEnvironmentParameter_NormalMap = deferredEnvironment.Parameters["NormalMap"];
            deferredEnvironmentParameter_DepthMap = deferredEnvironment.Parameters["DepthMap"];

            deferredEnvironmentParameterCameraPosition = deferredEnvironment.Parameters["cameraPosition"];
            deferredEnvironmentParameterInverseViewProjection = deferredEnvironment.Parameters["InvertViewProjection"];

            //VSM

            virtualShadowMappingEffect = content.Load<Effect>("Shaders/VirtualShadowMapsGenerate");
            virtualShadowMappingEffectParameter_WorldViewProj = virtualShadowMappingEffect.Parameters["WorldViewProj"];

            //SSReflections
            SSReflectionEffect = content.Load<Effect>("Shaders/SSReflectionEffect");

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
