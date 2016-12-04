using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
    public static class Shaders
    {
        //Id Generator
        public static Effect IdRenderEffect;
        public static EffectParameter IdRenderEffectParameterWorld;
        public static EffectParameter IdRenderEffectParameterWorldViewProj;
        public static EffectParameter IdRenderEffectParameterColorId;

        public static EffectPass IdRenderEffectDrawId;
        public static EffectPass IdRenderEffectDrawOutline;

        //Billboard Renderer

        public static Effect BillboardEffect;
        public static EffectParameter BillboardEffectParameter_WorldViewProj;
        public static EffectParameter BillboardEffectParameter_AspectRatio;
        public static EffectParameter BillboardEffectParameter_Texture;
        public static EffectParameter BillboardEffectParameter_DepthMap;
        public static EffectParameter BillboardEffectParameter_IdColor;

        public static EffectTechnique BillboardEffectTechnique_Billboard;
        public static EffectTechnique BillboardEffectTechnique_Id;

        //Lines
        public static Effect LineEffect;
        public static EffectParameter LineEffectParameter_WorldViewProj;

        //Temporal AntiAliasing

        public static Effect TemporalAntiAliasingEffect;

        public static EffectParameter TemporalAntiAliasingEffect_DepthMap;
        public static EffectParameter TemporalAntiAliasingEffect_AccumulationMap;
        public static EffectParameter TemporalAntiAliasingEffect_UpdateMap;
        public static EffectParameter TemporalAntiAliasingEffect_CurrentToPrevious;
        public static EffectParameter TemporalAntiAliasingEffect_Resolution;

        //Vignette and CA

        public static Effect PostProcessing;

        public static EffectParameter PostProcessingParameter_ScreenTexture;
        public static EffectParameter PostProcessingParameter_ChromaticAbberationStrength;
        public static EffectParameter PostProcessingParameter_SCurveStrength;

        public static EffectTechnique PostProcessingTechnique_Vignette;
        public static EffectTechnique PostProcessingTechnique_VignetteChroma;

        //Hologram

        public static Effect HologramEffect;
        public static EffectParameter HologramEffectParameter_WorldViewProj;
        public static EffectParameter HologramEffectParameter_World;

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
        public static EffectParameter EmissiveEffectParameter_Time;

        public static EffectParameter EmissiveEffectParameter_Resolution;

        public static EffectParameter EmissiveEffectParameter_DepthMap;
        public static EffectParameter EmissiveEffectParameter_EmissiveMap;
        public static EffectParameter EmissiveEffectParameter_NormalMap;

        public static EffectTechnique EmissiveEffectTechnique_DrawEmissiveBuffer;
        public static EffectTechnique EmissiveEffectTechnique_DrawEmissiveDiffuseEffect;
        public static EffectTechnique EmissiveEffectTechnique_DrawEmissiveSpecularEffect;
        //Second Screen Space Effect

        public static Effect ScreenSpaceReflectionEffect;
        public static EffectParameter ScreenSpaceReflectionParameter_DepthMap;
        public static EffectParameter ScreenSpaceReflectionParameter_TargetMap;
        public static EffectParameter ScreenSpaceReflectionParameter_NormalMap;
        public static EffectParameter ScreenSpaceReflectionParameter_ViewProjection;
        public static EffectParameter ScreenSpaceReflectionParameter_InverseViewProjection;
        public static EffectParameter ScreenSpaceReflectionParameter_CameraPosition;
        public static EffectParameter ScreenSpaceReflectionParameter_Resolution;
        public static EffectParameter ScreenSpaceReflectionParameter_Time;

        public static EffectTechnique ScreenSpaceReflectionTechnique_Default;
        public static EffectTechnique ScreenSpaceReflectionTechnique_Taa;

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

        //Gaussian Blur
        public static Effect GaussianBlurEffect;
        public static EffectParameter GaussianBlurEffectParameter_InverseResolution;
        public static EffectParameter GaussianBlurEffectParameter_TargetMap;

        //ClearGBuffer
        public static Effect ClearGBufferEffect;

        //GBuffer
        public static Effect GBufferEffect;

        public static EffectParameter GBufferEffectParameter_World;
        public static EffectParameter GBufferEffectParameter_WorldViewProj;
        public static EffectParameter GBufferEffectParameter_View;
        public static EffectParameter GBufferEffectParameter_Camera;

        public static EffectParameter GBufferEffectParameter_Material_Metallic;
        public static EffectParameter GBufferEffectParameter_Material_MetallicMap;
        public static EffectParameter GBufferEffectParameter_Material_DiffuseColor;
        public static EffectParameter GBufferEffectParameter_Material_Roughness;
        public static EffectParameter GBufferEffectParameter_Material_Mask;
        public static EffectParameter GBufferEffectParameter_Material_Texture;
        public static EffectParameter GBufferEffectParameter_Material_NormalMap;
        public static EffectParameter GBufferEffectParameter_Material_DisplacementMap;
        public static EffectParameter GBufferEffectParameter_Material_RoughnessMap;
        public static EffectParameter GBufferEffectParameter_Material_MaterialType;

        public static EffectTechnique GBufferEffectTechniques_DrawTextureDisplacement;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecularNormalMask;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureNormalMask;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecularMask;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureMask;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecularNormalMetallic;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecularNormal;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureNormal;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecular;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecularMetallic;
        public static EffectTechnique GBufferEffectTechniques_DrawTexture;
        public static EffectTechnique GBufferEffectTechniques_DrawBasic;

        //COMPOSE

        public static Effect DeferredCompose;
        public static EffectParameter DeferredComposeEffectParameter_ColorMap;
        public static EffectParameter DeferredComposeEffectParameter_diffuseLightMap;
        public static EffectParameter DeferredComposeEffectParameter_specularLightMap;
        public static EffectParameter DeferredComposeEffectParameter_volumeLightMap;
        public static EffectParameter DeferredComposeEffectParameter_HologramMap;
        public static EffectParameter DeferredComposeEffectParameter_SSAOMap;
        public static EffectParameter DeferredComposeEffectParameter_SSRMap;

        public static EffectTechnique DeferredComposeTechnique_1;
        public static EffectTechnique DeferredComposeTechnique_SSR;

        //Deferred Light
        //public static Effect deferredSpotLight;
        //public static EffectTechnique deferredSpotLightUnshadowed;
        //public static EffectTechnique deferredSpotLightShadowed;
        //public static EffectParameter deferredSpotLightParameterShadowMap;

        //Directional light

        public static Effect deferredDirectionalLight;
        public static EffectTechnique deferredDirectionalLightUnshadowed;
        public static EffectTechnique deferredDirectionalLightSSShadowed;
        public static EffectTechnique deferredDirectionalLightShadowed;
        public static EffectTechnique deferredDirectionalLightShadowOnly;

        public static EffectParameter deferredDirectionalLightParameterViewProjection;
        public static EffectParameter deferredDirectionalLightParameterCameraPosition;
        public static EffectParameter deferredDirectionalLightParameterInverseViewProjection;
        public static EffectParameter deferredDirectionalLightParameterLightViewProjection;

        public static EffectParameter deferredDirectionalLightParameter_LightColor;
        public static EffectParameter deferredDirectionalLightParameter_LightDirection;
        public static EffectParameter deferredDirectionalLightParameter_LightIntensity;

        public static EffectParameter deferredDirectionalLightParameter_ShadowFiltering;
        public static EffectParameter deferredDirectionalLightParameter_ShadowMapSize;
        
        public static EffectParameter deferredDirectionalLightParameter_AlbedoMap;
        public static EffectParameter deferredDirectionalLightParameter_NormalMap;
        public static EffectParameter deferredDirectionalLightParameter_DepthMap;
        public static EffectParameter deferredDirectionalLightParameter_ShadowMap;
        public static EffectParameter deferredDirectionalLightParameter_SSShadowMap;

        //Point Light
        public static Effect deferredPointLight;
        public static EffectTechnique deferredPointLightUnshadowed;
        public static EffectTechnique deferredPointLightUnshadowedVolumetric;
        public static EffectTechnique deferredPointLightShadowed;
        public static EffectTechnique deferredPointLightShadowedVolumetric;
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
        public static EffectParameter deferredPointLightParameter_Time;
        public static EffectParameter deferredPointLightParameter_LightVolumeDensity;

        public static EffectParameter deferredPointLightParameter_AlbedoMap;
        public static EffectParameter deferredPointLightParameter_NormalMap;
        public static EffectParameter deferredPointLightParameter_DepthMap;

        public static EffectParameter deferredPointLightParameter_LightPositionVS;
        public static EffectParameter deferredPointLightParameter_LightPositionTexCoord;
        //DeferredEnvironment


        public static Effect deferredEnvironment;
        public static EffectParameter deferredEnvironmentParameter_AlbedoMap;
        public static EffectParameter deferredEnvironmentParameter_NormalMap;
        public static EffectParameter deferredEnvironmentParameter_SSRMap;
        public static EffectParameter deferredEnvironmentParameter_DepthMap;
        public static EffectParameter deferredEnvironmentParameterCameraPosition;
        public static EffectParameter deferredEnvironmentParameterInverseViewProjection;

        //SHADOW MAPPING

        public static Effect virtualShadowMappingEffect;
        public static EffectParameter virtualShadowMappingEffectParameter_WorldViewProj;
        public static EffectTechnique virtualShadowMappingEffect_Technique_Depth;
        public static EffectTechnique virtualShadowMappingEffect_Technique_VSM;
        


        //SSR

        //public static Effect SSReflectionEffect;
        //public static EffectParameter SSReflectionEffectParameter_CameraPosition;
        //public static EffectParameter SSReflectionEffectParameter_InvertViewProjection;
        //public static EffectParameter SSReflectionEffectParameter_Projection;
        //public static EffectParameter SSReflectionEffectParameter_ViewProjection;
        //public static EffectParameter SSReflectionEffectParameter_DepthMap;
        //public static EffectParameter SSReflectionEffectParameter_NormalMap;
        //public static EffectParameter SSReflectionEffectParameter_AlbedoMap;
        //public static EffectParameter SSReflectionEffectParameter_Resolution;

        public static void Load(ContentManager content)
        {
            //Editor

            IdRenderEffect = content.Load<Effect>("Shaders/Editor/IdRender");
            IdRenderEffectParameterWorldViewProj = IdRenderEffect.Parameters["WorldViewProj"];
            IdRenderEffectParameterColorId = IdRenderEffect.Parameters["ColorId"];
            IdRenderEffectParameterWorld = IdRenderEffect.Parameters["World"];

            IdRenderEffectDrawId = IdRenderEffect.Techniques["DrawId"].Passes[0];
            IdRenderEffectDrawOutline = IdRenderEffect.Techniques["DrawOutline"].Passes[0];

            BillboardEffect = content.Load<Effect>("Shaders/Editor/BillboardEffect");
            BillboardEffectParameter_WorldViewProj = BillboardEffect.Parameters["WorldViewProj"];
            BillboardEffectParameter_AspectRatio = BillboardEffect.Parameters["AspectRatio"];
            BillboardEffectParameter_Texture = BillboardEffect.Parameters["Texture"];
            BillboardEffectParameter_DepthMap = BillboardEffect.Parameters["DepthMap"];
            BillboardEffectParameter_IdColor = BillboardEffect.Parameters["IdColor"];

            BillboardEffectTechnique_Billboard = BillboardEffect.Techniques["Billboard"];
            BillboardEffectTechnique_Id = BillboardEffect.Techniques["Id"];

            LineEffect = content.Load<Effect>("Shaders/Editor/LineEffect");
            LineEffectParameter_WorldViewProj = LineEffect.Parameters["WorldViewProj"];

            //TAA

            TemporalAntiAliasingEffect = content.Load<Effect>("Shaders/TemporalAntiAliasing/TemporalAntiAliasing");

            TemporalAntiAliasingEffect_AccumulationMap = TemporalAntiAliasingEffect.Parameters["AccumulationMap"];
            TemporalAntiAliasingEffect_UpdateMap = TemporalAntiAliasingEffect.Parameters["UpdateMap"];
            TemporalAntiAliasingEffect_DepthMap = TemporalAntiAliasingEffect.Parameters["DepthMap"];
            TemporalAntiAliasingEffect_CurrentToPrevious = TemporalAntiAliasingEffect.Parameters["CurrentToPrevious"];
            TemporalAntiAliasingEffect_Resolution = TemporalAntiAliasingEffect.Parameters["Resolution"];
            //Post

            PostProcessing = content.Load<Effect>("Shaders/PostProcessing/PostProcessing");

            PostProcessingParameter_ChromaticAbberationStrength =
                PostProcessing.Parameters["ChromaticAbberationStrength"];
            PostProcessingParameter_SCurveStrength = PostProcessing.Parameters["SCurveStrength"];
            PostProcessingParameter_ScreenTexture = PostProcessing.Parameters["ScreenTexture"];
            PostProcessingTechnique_Vignette = PostProcessing.Techniques["Vignette"];
            PostProcessingTechnique_VignetteChroma = PostProcessing.Techniques["VignetteChroma"];

            //Hologram Effect
            HologramEffect = content.Load<Effect>("Shaders/Hologram/HologramEffect");
            HologramEffectParameter_World = HologramEffect.Parameters["World"];
            HologramEffectParameter_WorldViewProj = HologramEffect.Parameters["WorldViewProj"];

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
            EmissiveEffectParameter_Time = EmissiveEffect.Parameters["Time"];

            EmissiveEffectTechnique_DrawEmissiveBuffer = EmissiveEffect.Techniques["DrawEmissiveBuffer"];
            EmissiveEffectTechnique_DrawEmissiveSpecularEffect = EmissiveEffect.Techniques["DrawEmissiveSpecularEffect"];
            EmissiveEffectTechnique_DrawEmissiveDiffuseEffect = EmissiveEffect.Techniques["DrawEmissiveDiffuseEffect"];
            //Screen Space Effect 2
            ScreenSpaceReflectionEffect = content.Load<Effect>("Shaders/ScreenSpace/ScreenSpaceEffect2");

            ScreenSpaceReflectionParameter_DepthMap = ScreenSpaceReflectionEffect.Parameters["DepthMap"];
            ScreenSpaceReflectionParameter_NormalMap = ScreenSpaceReflectionEffect.Parameters["NormalMap"];
            ScreenSpaceReflectionParameter_TargetMap = ScreenSpaceReflectionEffect.Parameters["TargetMap"];
            ScreenSpaceReflectionParameter_CameraPosition = ScreenSpaceReflectionEffect.Parameters["CameraPosition"];
            ScreenSpaceReflectionParameter_Resolution = ScreenSpaceReflectionEffect.Parameters["resolution"];
            ScreenSpaceReflectionParameter_ViewProjection = ScreenSpaceReflectionEffect.Parameters["ViewProjection"];
            ScreenSpaceReflectionParameter_InverseViewProjection = ScreenSpaceReflectionEffect.Parameters["InverseViewProjection"];
            ScreenSpaceReflectionParameter_Time = ScreenSpaceReflectionEffect.Parameters["Time"];

            ScreenSpaceReflectionTechnique_Default = ScreenSpaceReflectionEffect.Techniques["Default"];
            ScreenSpaceReflectionTechnique_Taa = ScreenSpaceReflectionEffect.Techniques["TAA"];

            //Screen Space Effect
            ScreenSpaceEffect = content.Load<Effect>("Shaders/ScreenSpace/ScreenSpaceEffect");

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

            //Blur
            GaussianBlurEffect = content.Load<Effect>("Shaders/ScreenSpace/GaussianBlur");
            GaussianBlurEffectParameter_InverseResolution = GaussianBlurEffect.Parameters["InverseResolution"];
            GaussianBlurEffectParameter_TargetMap = GaussianBlurEffect.Parameters["TargetMap"];

            //Gbuffer
            GBufferEffect = content.Load<Effect>("Shaders/GbufferSetup/GBuffer");

            GBufferEffectParameter_World = GBufferEffect.Parameters["World"];
            GBufferEffectParameter_WorldViewProj = GBufferEffect.Parameters["WorldViewProj"];
            GBufferEffectParameter_View = GBufferEffect.Parameters["View"];
            GBufferEffectParameter_Camera = GBufferEffect.Parameters["Camera"];

            GBufferEffectParameter_Material_Metallic = GBufferEffect.Parameters["Metallic"];
            GBufferEffectParameter_Material_MetallicMap = GBufferEffect.Parameters["MetallicMap"];
            GBufferEffectParameter_Material_DiffuseColor = GBufferEffect.Parameters["DiffuseColor"];
            GBufferEffectParameter_Material_Roughness = GBufferEffect.Parameters["Roughness"];

            GBufferEffectParameter_Material_Mask = GBufferEffect.Parameters["Mask"];
            GBufferEffectParameter_Material_Texture = GBufferEffect.Parameters["Texture"];
            GBufferEffectParameter_Material_NormalMap = GBufferEffect.Parameters["NormalMap"];
            GBufferEffectParameter_Material_RoughnessMap = GBufferEffect.Parameters["RoughnessMap"];
            GBufferEffectParameter_Material_DisplacementMap = GBufferEffect.Parameters["DisplacementMap"];

            GBufferEffectParameter_Material_MaterialType = GBufferEffect.Parameters["MaterialType"];

            ClearGBufferEffect = content.Load<Effect>("Shaders/GbufferSetup/ClearGBuffer");

            //Techniques

            GBufferEffectTechniques_DrawTextureDisplacement = GBufferEffect.Techniques["DrawTextureDisplacement"];
            GBufferEffectTechniques_DrawTextureSpecularNormalMask = GBufferEffect.Techniques["DrawTextureSpecularNormalMask"];
            GBufferEffectTechniques_DrawTextureNormalMask = GBufferEffect.Techniques["DrawTextureNormalMask"];
            GBufferEffectTechniques_DrawTextureSpecularMask = GBufferEffect.Techniques["DrawTextureSpecularMask"];
            GBufferEffectTechniques_DrawTextureMask = GBufferEffect.Techniques["DrawTextureMask"];
            GBufferEffectTechniques_DrawTextureSpecularNormalMetallic = GBufferEffect.Techniques["DrawTextureSpecularNormalMetallic"];
            GBufferEffectTechniques_DrawTextureSpecularNormal = GBufferEffect.Techniques["DrawTextureSpecularNormal"];
            GBufferEffectTechniques_DrawTextureNormal = GBufferEffect.Techniques["DrawTextureNormal"];
            GBufferEffectTechniques_DrawTextureSpecular = GBufferEffect.Techniques["DrawTextureSpecular"];
            GBufferEffectTechniques_DrawTextureSpecularMetallic = GBufferEffect.Techniques["DrawTextureSpecularMetallic"];
            GBufferEffectTechniques_DrawTexture = GBufferEffect.Techniques["DrawTexture"];
            GBufferEffectTechniques_DrawBasic = GBufferEffect.Techniques["DrawBasic"];


            //DeferredCompose

            DeferredCompose = content.Load<Effect>("Shaders/Deferred/DeferredCompose");

            DeferredComposeEffectParameter_ColorMap = DeferredCompose.Parameters["colorMap"];
            DeferredComposeEffectParameter_diffuseLightMap = DeferredCompose.Parameters["diffuseLightMap"];
            DeferredComposeEffectParameter_specularLightMap = DeferredCompose.Parameters["specularLightMap"];
            DeferredComposeEffectParameter_volumeLightMap = DeferredCompose.Parameters["volumeLightMap"];
            DeferredComposeEffectParameter_HologramMap = DeferredCompose.Parameters["HologramMap"];
            DeferredComposeEffectParameter_SSAOMap = DeferredCompose.Parameters["SSAOMap"];
            DeferredComposeEffectParameter_SSRMap = DeferredCompose.Parameters["SSRMap"];

            DeferredComposeTechnique_1 = DeferredCompose.Techniques["Technique1"];
            DeferredComposeTechnique_SSR = DeferredCompose.Techniques["TechniqueSSR"];
            ////DeferredLights

            //deferredSpotLight = content.Load<Effect>("Shaders/Deferred/DeferredSpotLight");

            //deferredSpotLightUnshadowed = deferredSpotLight.Techniques["Unshadowed"];
            //deferredSpotLightShadowed = deferredSpotLight.Techniques["Shadowed"];

            //deferredSpotLightParameterShadowMap = deferredSpotLight.Parameters["shadowMap"];

            //Directional Light
            deferredDirectionalLight = content.Load<Effect>("Shaders/Deferred/DeferredDirectionalLight");

            deferredDirectionalLightUnshadowed = deferredDirectionalLight.Techniques["Unshadowed"];
            deferredDirectionalLightSSShadowed = deferredDirectionalLight.Techniques["SSShadowed"];
            deferredDirectionalLightShadowed = deferredDirectionalLight.Techniques["Shadowed"];
            deferredDirectionalLightShadowOnly = deferredDirectionalLight.Techniques["ShadowOnly"];

            deferredDirectionalLightParameterViewProjection = deferredDirectionalLight.Parameters["ViewProjection"];
            deferredDirectionalLightParameterCameraPosition = deferredDirectionalLight.Parameters["cameraPosition"];
            deferredDirectionalLightParameterInverseViewProjection = deferredDirectionalLight.Parameters["InvertViewProjection"];
            deferredDirectionalLightParameterLightViewProjection = deferredDirectionalLight.Parameters["LightViewProjection"];

            deferredDirectionalLightParameter_LightColor = deferredDirectionalLight.Parameters["lightColor"];
            deferredDirectionalLightParameter_LightIntensity = deferredDirectionalLight.Parameters["lightIntensity"];
            deferredDirectionalLightParameter_LightDirection = deferredDirectionalLight.Parameters["LightVector"];
            deferredDirectionalLightParameter_ShadowFiltering = deferredDirectionalLight.Parameters["ShadowFiltering"];
            deferredDirectionalLightParameter_ShadowMapSize = deferredDirectionalLight.Parameters["ShadowMapSize"];

            deferredDirectionalLightParameter_AlbedoMap = deferredDirectionalLight.Parameters["AlbedoMap"];
            deferredDirectionalLightParameter_NormalMap = deferredDirectionalLight.Parameters["NormalMap"];
            deferredDirectionalLightParameter_DepthMap = deferredDirectionalLight.Parameters["DepthMap"];
            deferredDirectionalLightParameter_ShadowMap = deferredDirectionalLight.Parameters["ShadowMap"];
            deferredDirectionalLightParameter_SSShadowMap = deferredDirectionalLight.Parameters["SSShadowMap"];

            //PL
            deferredPointLight = content.Load<Effect>("Shaders/Deferred/DeferredPointLight");

            deferredPointLightUnshadowed = deferredPointLight.Techniques["Unshadowed"];
            deferredPointLightUnshadowedVolumetric = deferredPointLight.Techniques["UnshadowedVolume"];
            deferredPointLightShadowed = deferredPointLight.Techniques["Shadowed"];
            deferredPointLightShadowedVolumetric = deferredPointLight.Techniques["ShadowedVolume"];

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
            deferredPointLightParameter_LightPositionVS = deferredPointLight.Parameters["lightPositionVS"];
            deferredPointLightParameter_LightPositionTexCoord = deferredPointLight.Parameters["lightPositionTexCoord"];
            deferredPointLightParameter_Inside = deferredPointLight.Parameters["inside"];
            deferredPointLightParameter_Time = deferredPointLight.Parameters["Time"];
            deferredPointLightParameter_LightVolumeDensity =
                deferredPointLight.Parameters["lightVolumeDensity"];
            deferredPointLightParameter_AlbedoMap = deferredPointLight.Parameters["AlbedoMap"];
            deferredPointLightParameter_NormalMap = deferredPointLight.Parameters["NormalMap"];
            deferredPointLightParameter_DepthMap = deferredPointLight.Parameters["DepthMap"];

            //Environment
            deferredEnvironment = content.Load<Effect>("Shaders/Deferred/DeferredEnvironmentMap");
            deferredEnvironmentParameter_AlbedoMap = deferredEnvironment.Parameters["AlbedoMap"];
            deferredEnvironmentParameter_NormalMap = deferredEnvironment.Parameters["NormalMap"];
            deferredEnvironmentParameter_DepthMap = deferredEnvironment.Parameters["DepthMap"];
            deferredEnvironmentParameter_SSRMap = deferredEnvironment.Parameters["ReflectionMap"];

            deferredEnvironmentParameterCameraPosition = deferredEnvironment.Parameters["cameraPosition"];
            deferredEnvironmentParameterInverseViewProjection = deferredEnvironment.Parameters["InvertViewProjection"];

            //VSM

            virtualShadowMappingEffect = content.Load<Effect>("Shaders/Shadow/VirtualShadowMapsGenerate");
            virtualShadowMappingEffectParameter_WorldViewProj = virtualShadowMappingEffect.Parameters["WorldViewProj"];

            virtualShadowMappingEffect_Technique_VSM = virtualShadowMappingEffect.Techniques["DrawVSM"];
            virtualShadowMappingEffect_Technique_Depth = virtualShadowMappingEffect.Techniques["DrawDepth"];
            //SSReflections
            //SSReflectionEffect = content.Load<Effect>("Shaders/SSReflectionEffect");

            //SSReflectionEffectParameter_InvertViewProjection = SSReflectionEffect.Parameters["InvertViewProjection"];
            //SSReflectionEffectParameter_ViewProjection = SSReflectionEffect.Parameters["ViewProjection"];
            //SSReflectionEffectParameter_Projection = SSReflectionEffect.Parameters["Projection"];
            //SSReflectionEffectParameter_CameraPosition = SSReflectionEffect.Parameters["cameraPosition"];

            //SSReflectionEffectParameter_DepthMap = SSReflectionEffect.Parameters["depthMap"];
            //SSReflectionEffectParameter_NormalMap = SSReflectionEffect.Parameters["normalMap"];
            //SSReflectionEffectParameter_AlbedoMap = SSReflectionEffect.Parameters["albedoMap"];

            //SSReflectionEffectParameter_Resolution = SSReflectionEffect.Parameters["resolution"];
        }
    }
}
