using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{
    public static class Shaders
    {
        //Depth Reconstruction
        public static Effect ReconstructDepth;
        public static EffectParameter ReconstructDepthParameter_DepthMap;
        public static EffectParameter ReconstructDepthParameter_FrustumCorners;
        public static EffectParameter ReconstructDepthParameter_FarClip;
        public static EffectParameter ReconstructDepthParameter_Projection;

        //Id Generator
        public static Effect IdRenderEffect;
        public static EffectParameter IdRenderEffectParameterWorld;
        public static EffectParameter IdRenderEffectParameterWorldViewProj;
        public static EffectParameter IdRenderEffectParameterColorId;
        public static EffectParameter IdRenderEffectParameterOutlineSize;

        public static EffectPass IdRenderEffectDrawId;
        public static EffectPass IdRenderEffectDrawOutline;

        //Billboard Renderer

        public static Effect BillboardEffect;
        public static EffectParameter BillboardEffectParameter_WorldViewProj;
        public static EffectParameter BillboardEffectParameter_WorldView;
        public static EffectParameter BillboardEffectParameter_AspectRatio;
        public static EffectParameter BillboardEffectParameter_FarClip;
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
        public static EffectParameter TemporalAntiAliasingEffect_FrustumCorners;
        public static EffectParameter TemporalAntiAliasingEffect_Threshold;

        //test shadow

        public static Effect TestShadowEffect;

        public static EffectParameter TestShadowEffect_DepthMap;
        public static EffectParameter TestShadowEffect_AccumulationMap;
        public static EffectParameter TestShadowEffect_UpdateMap;
        public static EffectParameter TestShadowEffect_CurrentToPrevious;
        public static EffectParameter TestShadowEffect_Resolution;
        public static EffectParameter TestShadowEffect_FrustumCorners;
        public static EffectParameter TestShadowEffect_Threshold;

        //Vignette and CA

        public static Effect PostProcessing;

        public static EffectParameter PostProcessingParameter_ScreenTexture;
        public static EffectParameter PostProcessingParameter_ChromaticAbberationStrength;
        public static EffectParameter PostProcessingParameter_SCurveStrength;
        public static EffectParameter PostProcessingParameter_WhitePoint;
        public static EffectParameter PostProcessingParameter_Exposure;

        public static EffectTechnique PostProcessingTechnique_Vignette;
        public static EffectTechnique PostProcessingTechnique_Base;
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
        public static EffectParameter ScreenSpaceReflectionParameter_Projection;
        public static EffectParameter ScreenSpaceReflectionParameter_Resolution;
        public static EffectParameter ScreenSpaceReflectionParameter_Time;
        public static EffectParameter ScreenSpaceReflectionParameter_FrustumCorners;
        public static EffectParameter ScreenSpaceReflectionParameter_FarClip;
        public static EffectTechnique ScreenSpaceReflectionTechnique_Default;
        public static EffectTechnique ScreenSpaceReflectionTechnique_Taa;
        public static EffectParameter ScreenSpaceReflectionParameter_NoiseMap;

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
        public static EffectParameter ScreenSpaceEffectParameter_FrustumCorners;

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

        public static EffectParameter GBufferEffectParameter_WorldView;
        public static EffectParameter GBufferEffectParameter_WorldViewProj;
        public static EffectParameter GBufferEffectParameter_WorldViewIT;
        public static EffectParameter GBufferEffectParameter_Camera;
        public static EffectParameter GBufferEffectParameter_FarClip;

        public static EffectParameter GBufferEffectParameter_Material_Metallic;
        public static EffectParameter GBufferEffectParameter_Material_MetallicMap;
        public static EffectParameter GBufferEffectParameter_Material_DiffuseColor;
        public static EffectParameter GBufferEffectParameter_Material_Roughness;
        public static EffectParameter GBufferEffectParameter_Material_MaskMap;
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
        public static EffectParameter DeferredComposeEffectParameter_LinearMap;
        public static EffectParameter DeferredComposeEffectParameter_UseSSAO;

        public static EffectTechnique DeferredComposeTechnique_NonLinear;
        public static EffectTechnique DeferredComposeTechnique_Linear;
        //public static EffectTechnique DeferredComposeTechnique_Unlinearize;

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
        public static EffectParameter deferredDirectionalLightParameterFrustumCorners;
        public static EffectParameter deferredDirectionalLightParameterCameraPosition;
        public static EffectParameter deferredDirectionalLightParameterInverseViewProjection;
        public static EffectParameter deferredDirectionalLightParameterLightViewProjection;
        public static EffectParameter deferredDirectionalLightParameterLightView;
        public static EffectParameter deferredDirectionalLightParameterLightFarClip;

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
        public static EffectTechnique deferredPointLightWriteStencil;
        public static EffectParameter deferredPointLightParameterResolution;
        public static EffectParameter deferredPointLightParameterShadowMap;
        public static EffectParameter deferredPointLightParameterLightViewProjectionPositiveX;
        public static EffectParameter deferredPointLightParameterLightViewProjectionNegativeX;
        public static EffectParameter deferredPointLightParameterLightViewProjectionPositiveY;
        public static EffectParameter deferredPointLightParameterLightViewProjectionNegativeY;
        public static EffectParameter deferredPointLightParameterLightViewProjectionPositiveZ;
        public static EffectParameter deferredPointLightParameterLightViewProjectionNegativeZ;

        public static EffectParameter deferredPointLightParameter_WorldView;
        public static EffectParameter deferredPointLightParameter_WorldViewProjection;
        public static EffectParameter deferredPointLightParameter_InverseView;

        public static EffectParameter deferredPointLightParameter_LightPosition;
        public static EffectParameter deferredPointLightParameter_LightColor;
        public static EffectParameter deferredPointLightParameter_LightRadius;
        public static EffectParameter deferredPointLightParameter_LightIntensity;
        public static EffectParameter deferredPointLightParameter_ShadowMapSize;
        public static EffectParameter deferredPointLightParameter_Inside;
        public static EffectParameter deferredPointLightParameter_Time;
        public static EffectParameter deferredPointLightParameter_FarClip;
        public static EffectParameter deferredPointLightParameter_LightVolumeDensity;

        public static EffectParameter deferredPointLightParameter_NoiseMap;
        public static EffectParameter deferredPointLightParameter_AlbedoMap;
        public static EffectParameter deferredPointLightParameter_NormalMap;
        public static EffectParameter deferredPointLightParameter_DepthMap;

        //DeferredEnvironment
        
        public static Effect deferredEnvironment;
        public static EffectParameter deferredEnvironmentParameter_AlbedoMap;
        public static EffectParameter deferredEnvironmentParameter_NormalMap;
        public static EffectParameter deferredEnvironmentParameter_SSRMap;
        public static EffectParameter deferredEnvironmentParameter_FrustumCorners;
        public static EffectParameter deferredEnvironmentParameter_ReflectionCubeMap;
        public static EffectParameter deferredEnvironmentParameter_Resolution;
        public static EffectParameter deferredEnvironmentParameterTransposeView;

        //SHADOW MAPPING

        public static Effect virtualShadowMappingEffect;
        public static EffectParameter virtualShadowMappingEffectParameter_WorldViewProj;
        public static EffectParameter virtualShadowMappingEffectParameter_WorldView;
        public static EffectParameter virtualShadowMappingEffectParameter_FarClip;
        public static EffectParameter virtualShadowMappingEffectParameter_SizeBias;
        public static EffectTechnique virtualShadowMappingEffect_Technique_Linear;
        public static EffectTechnique virtualShadowMappingEffect_Technique_ZW;


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
            //Depth reconstr
            ReconstructDepth = content.Load<Effect>("Shaders/ScreenSpace/ReconstructDepth");
            ReconstructDepthParameter_DepthMap = ReconstructDepth.Parameters["DepthMap"];
            ReconstructDepthParameter_Projection = ReconstructDepth.Parameters["Projection"];
            ReconstructDepthParameter_FarClip = ReconstructDepth.Parameters["FarClip"];
            ReconstructDepthParameter_FrustumCorners = ReconstructDepth.Parameters["FrustumCorners"];

            //Editor

            IdRenderEffect = content.Load<Effect>("Shaders/Editor/IdRender");
            IdRenderEffectParameterWorldViewProj = IdRenderEffect.Parameters["WorldViewProj"];
            IdRenderEffectParameterColorId = IdRenderEffect.Parameters["ColorId"];
            IdRenderEffectParameterOutlineSize = IdRenderEffect.Parameters["OutlineSize"];
            IdRenderEffectParameterWorld = IdRenderEffect.Parameters["World"];

            IdRenderEffectDrawId = IdRenderEffect.Techniques["DrawId"].Passes[0];
            IdRenderEffectDrawOutline = IdRenderEffect.Techniques["DrawOutline"].Passes[0];

            BillboardEffect = content.Load<Effect>("Shaders/Editor/BillboardEffect");
            BillboardEffectParameter_WorldViewProj = BillboardEffect.Parameters["WorldViewProj"];
            BillboardEffectParameter_WorldView = BillboardEffect.Parameters["WorldView"];
            BillboardEffectParameter_AspectRatio = BillboardEffect.Parameters["AspectRatio"];
            BillboardEffectParameter_FarClip = BillboardEffect.Parameters["FarClip"];
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
            TemporalAntiAliasingEffect_FrustumCorners = TemporalAntiAliasingEffect.Parameters["FrustumCorners"];
            TemporalAntiAliasingEffect_Threshold = TemporalAntiAliasingEffect.Parameters["Threshold"];


            //TEST shadow
            TestShadowEffect = content.Load<Effect>("Shaders/Shadow/testshadow");

            TestShadowEffect_AccumulationMap = TestShadowEffect.Parameters["AccumulationMap"];
            TestShadowEffect_DepthMap = TestShadowEffect.Parameters["DepthMap"];
            TestShadowEffect_CurrentToPrevious = TestShadowEffect.Parameters["CurrentToPrevious"];
            TestShadowEffect_Resolution = TestShadowEffect.Parameters["Resolution"];
            TestShadowEffect_FrustumCorners = TestShadowEffect.Parameters["FrustumCorners"];

            //Post

            PostProcessing = content.Load<Effect>("Shaders/PostProcessing/PostProcessing");

            PostProcessingParameter_ChromaticAbberationStrength =
                PostProcessing.Parameters["ChromaticAbberationStrength"];
            PostProcessingParameter_SCurveStrength = PostProcessing.Parameters["SCurveStrength"];
            PostProcessingParameter_WhitePoint = PostProcessing.Parameters["WhitePoint"];
            PostProcessingParameter_Exposure = PostProcessing.Parameters["Exposure"];

            PostProcessingParameter_ScreenTexture = PostProcessing.Parameters["ScreenTexture"];
            PostProcessingTechnique_Vignette = PostProcessing.Techniques["Vignette"];
            PostProcessingTechnique_VignetteChroma = PostProcessing.Techniques["VignetteChroma"];
            PostProcessingTechnique_Base = PostProcessing.Techniques["BaseChroma"];

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
            ScreenSpaceReflectionParameter_Resolution = ScreenSpaceReflectionEffect.Parameters["resolution"];
            ScreenSpaceReflectionParameter_Projection = ScreenSpaceReflectionEffect.Parameters["Projection"];
            ScreenSpaceReflectionParameter_Time = ScreenSpaceReflectionEffect.Parameters["Time"];
            ScreenSpaceReflectionParameter_FrustumCorners = ScreenSpaceReflectionEffect.Parameters["FrustumCorners"];
            ScreenSpaceReflectionParameter_FarClip = ScreenSpaceReflectionEffect.Parameters["FarClip"];
            ScreenSpaceReflectionParameter_NoiseMap = ScreenSpaceReflectionEffect.Parameters["NoiseMap"];

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
            ScreenSpaceEffectParameter_FrustumCorners = ScreenSpaceEffect.Parameters["FrustumCorners"];

            ScreenSpaceEffectTechnique_SSAO = ScreenSpaceEffect.Techniques["SSAO"];
            ScreenSpaceEffectTechnique_BlurHorizontal = ScreenSpaceEffect.Techniques["BilateralHorizontal"];
            ScreenSpaceEffectTechnique_BlurVertical = ScreenSpaceEffect.Techniques["BilateralVertical"];

            //Blur
            GaussianBlurEffect = content.Load<Effect>("Shaders/ScreenSpace/GaussianBlur");
            GaussianBlurEffectParameter_InverseResolution = GaussianBlurEffect.Parameters["InverseResolution"];
            GaussianBlurEffectParameter_TargetMap = GaussianBlurEffect.Parameters["TargetMap"];

            //Gbuffer
            GBufferEffect = content.Load<Effect>("Shaders/GbufferSetup/GBuffer");

            GBufferEffectParameter_WorldView = GBufferEffect.Parameters["WorldView"];
            GBufferEffectParameter_WorldViewProj = GBufferEffect.Parameters["WorldViewProj"];
            GBufferEffectParameter_WorldViewIT = GBufferEffect.Parameters["WorldViewIT"];
            GBufferEffectParameter_Camera = GBufferEffect.Parameters["Camera"];
            GBufferEffectParameter_FarClip = GBufferEffect.Parameters["FarClip"];

            GBufferEffectParameter_Material_Metallic = GBufferEffect.Parameters["Metallic"];
            GBufferEffectParameter_Material_MetallicMap = GBufferEffect.Parameters["MetallicMap"];
            GBufferEffectParameter_Material_DiffuseColor = GBufferEffect.Parameters["DiffuseColor"];
            GBufferEffectParameter_Material_Roughness = GBufferEffect.Parameters["Roughness"];

            GBufferEffectParameter_Material_MaskMap = GBufferEffect.Parameters["Mask"];
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
            DeferredComposeEffectParameter_LinearMap = DeferredCompose.Parameters["LinearMap"];
            DeferredComposeEffectParameter_SSRMap = DeferredCompose.Parameters["SSRMap"];
            DeferredComposeEffectParameter_UseSSAO = DeferredCompose.Parameters["useSSAO"];

            DeferredComposeTechnique_NonLinear = DeferredCompose.Techniques["TechniqueNonLinear"];
            DeferredComposeTechnique_Linear = DeferredCompose.Techniques["TechniqueLinear"];
            //DeferredComposeTechnique_Unlinearize = DeferredCompose.Techniques["Unlinearize"];

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
            deferredDirectionalLightParameterFrustumCorners = deferredDirectionalLight.Parameters["FrustumCorners"];
            deferredDirectionalLightParameterCameraPosition = deferredDirectionalLight.Parameters["cameraPosition"];
            deferredDirectionalLightParameterInverseViewProjection = deferredDirectionalLight.Parameters["InvertViewProjection"];
            deferredDirectionalLightParameterLightViewProjection = deferredDirectionalLight.Parameters["LightViewProjection"];
            deferredDirectionalLightParameterLightView = deferredDirectionalLight.Parameters["LightView"];
            deferredDirectionalLightParameterLightFarClip = deferredDirectionalLight.Parameters["LightFarClip"];

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
            deferredPointLightWriteStencil = deferredPointLight.Techniques["WriteStencilMask"];

            deferredPointLightParameterShadowMap = deferredPointLight.Parameters["shadowMap"];

            deferredPointLightParameterShadowMap = deferredPointLight.Parameters["shadowCubeMap"];
            deferredPointLightParameterLightViewProjectionPositiveX = deferredPointLight.Parameters["LightViewProjectionPositiveX"];
            deferredPointLightParameterLightViewProjectionNegativeX = deferredPointLight.Parameters["LightViewProjectionNegativeX"];
            deferredPointLightParameterLightViewProjectionPositiveY = deferredPointLight.Parameters["LightViewProjectionPositiveY"];
            deferredPointLightParameterLightViewProjectionNegativeY = deferredPointLight.Parameters["LightViewProjectionNegativeY"];
            deferredPointLightParameterLightViewProjectionPositiveZ = deferredPointLight.Parameters["LightViewProjectionPositiveZ"];
            deferredPointLightParameterLightViewProjectionNegativeZ = deferredPointLight.Parameters["LightViewProjectionNegativeZ"];

            deferredPointLightParameterResolution = deferredPointLight.Parameters["Resolution"];
            deferredPointLightParameter_WorldView = deferredPointLight.Parameters["WorldView"];
            deferredPointLightParameter_WorldViewProjection = deferredPointLight.Parameters["WorldViewProj"];
            deferredPointLightParameter_InverseView = deferredPointLight.Parameters["InverseView"];
            
            deferredPointLightParameter_LightPosition = deferredPointLight.Parameters["lightPosition"];
            deferredPointLightParameter_LightColor = deferredPointLight.Parameters["lightColor"];
            deferredPointLightParameter_LightRadius = deferredPointLight.Parameters["lightRadius"];
            deferredPointLightParameter_LightIntensity = deferredPointLight.Parameters["lightIntensity"];
            deferredPointLightParameter_ShadowMapSize = deferredPointLight.Parameters["ShadowMapSize"];
            deferredPointLightParameter_Inside = deferredPointLight.Parameters["inside"];
            deferredPointLightParameter_Time = deferredPointLight.Parameters["Time"];
            deferredPointLightParameter_FarClip = deferredPointLight.Parameters["FarClip"];
            deferredPointLightParameter_LightVolumeDensity =
                deferredPointLight.Parameters["lightVolumeDensity"];
            deferredPointLightParameter_NoiseMap = deferredPointLight.Parameters["NoiseMap"];
            deferredPointLightParameter_AlbedoMap = deferredPointLight.Parameters["AlbedoMap"];
            deferredPointLightParameter_NormalMap = deferredPointLight.Parameters["NormalMap"];
            deferredPointLightParameter_DepthMap = deferredPointLight.Parameters["DepthMap"];

            //Environment
            deferredEnvironment = content.Load<Effect>("Shaders/Deferred/DeferredEnvironmentMap");
            deferredEnvironmentParameter_AlbedoMap = deferredEnvironment.Parameters["AlbedoMap"];
            deferredEnvironmentParameter_NormalMap = deferredEnvironment.Parameters["NormalMap"];
            deferredEnvironmentParameter_FrustumCorners = deferredEnvironment.Parameters["FrustumCorners"];
            deferredEnvironmentParameter_SSRMap = deferredEnvironment.Parameters["ReflectionMap"];
            deferredEnvironmentParameter_ReflectionCubeMap = deferredEnvironment.Parameters["ReflectionCubeMap"];
            deferredEnvironmentParameter_Resolution = deferredEnvironment.Parameters["Resolution"];
            deferredEnvironmentParameterTransposeView = deferredEnvironment.Parameters["TransposeView"];

            //VSM

            virtualShadowMappingEffect = content.Load<Effect>("Shaders/Shadow/VirtualShadowMapsGenerate");
            virtualShadowMappingEffectParameter_WorldViewProj = virtualShadowMappingEffect.Parameters["WorldViewProj"];
            virtualShadowMappingEffectParameter_WorldView= virtualShadowMappingEffect.Parameters["WorldView"];
            virtualShadowMappingEffectParameter_FarClip = virtualShadowMappingEffect.Parameters["FarClip"];
            virtualShadowMappingEffectParameter_SizeBias = virtualShadowMappingEffect.Parameters["SizeBias"];

            virtualShadowMappingEffect_Technique_ZW = virtualShadowMappingEffect.Techniques["DrawDepthZW"];
            virtualShadowMappingEffect_Technique_Linear = virtualShadowMappingEffect.Techniques["DrawDepthLinear"];
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
