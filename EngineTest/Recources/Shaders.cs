using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{
    //As suggested here http://community.monogame.net/t/deferred-engine-playground-download/8180/283?u=kosmonautgames
    // by bettina4you

    public static class Globals
    {
        public static ContentManager content;
    }

    public static class Shaders
    {
        //A static file which contains all shaders
        //Born out of need for quick thoughtless shader building
        //I am working on making seperate shading modules instead and will slowly shorten this one.
        //Depth Reconstruction
        public static Effect ReconstructDepth = Globals.content.Load<Effect>("Shaders/ScreenSpace/ReconstructDepth");
        public static EffectParameter ReconstructDepthParameter_DepthMap = ReconstructDepth.Parameters["DepthMap"];
        public static EffectParameter ReconstructDepthParameter_Projection = ReconstructDepth.Parameters["Projection"];
        public static EffectParameter ReconstructDepthParameter_FarClip = ReconstructDepth.Parameters["FarClip"];
        public static EffectParameter ReconstructDepthParameter_FrustumCorners = ReconstructDepth.Parameters["FrustumCorners"];

        //Id Generator
        public static Effect IdRenderEffect = Globals.content.Load<Effect>("Shaders/Editor/IdRender");
        public static EffectParameter IdRenderEffectParameterWorldViewProj = IdRenderEffect.Parameters["WorldViewProj"];
        public static EffectParameter IdRenderEffectParameterColorId = IdRenderEffect.Parameters["ColorId"];
        public static EffectParameter IdRenderEffectParameterOutlineSize = IdRenderEffect.Parameters["OutlineSize"];
        public static EffectParameter IdRenderEffectParameterWorld = IdRenderEffect.Parameters["World"];

        public static EffectPass IdRenderEffectDrawId = IdRenderEffect.Techniques["DrawId"].Passes[0];
        public static EffectPass IdRenderEffectDrawOutline = IdRenderEffect.Techniques["DrawOutline"].Passes[0];

        //Billboard Renderer
        public static Effect BillboardEffect = Globals.content.Load<Effect>("Shaders/Editor/BillboardEffect");

        public static EffectParameter BillboardEffectParameter_WorldViewProj = BillboardEffect.Parameters["WorldViewProj"];
        public static EffectParameter BillboardEffectParameter_WorldView = BillboardEffect.Parameters["WorldView"];
        public static EffectParameter BillboardEffectParameter_AspectRatio = BillboardEffect.Parameters["AspectRatio"];
        public static EffectParameter BillboardEffectParameter_FarClip = BillboardEffect.Parameters["FarClip"];
        public static EffectParameter BillboardEffectParameter_Texture = BillboardEffect.Parameters["Texture"];
        public static EffectParameter BillboardEffectParameter_DepthMap = BillboardEffect.Parameters["DepthMap"];
        public static EffectParameter BillboardEffectParameter_IdColor = BillboardEffect.Parameters["IdColor"];

        public static EffectTechnique BillboardEffectTechnique_Billboard = BillboardEffect.Techniques["Billboard"];
        public static EffectTechnique BillboardEffectTechnique_Id = BillboardEffect.Techniques["Id"];

        //Lines
        public static Effect LineEffect = Globals.content.Load<Effect>("Shaders/Editor/LineEffect");
        public static EffectParameter LineEffectParameter_WorldViewProj = LineEffect.Parameters["WorldViewProj"];

        //Temporal AntiAliasing


        //Vignette and CA
        public static Effect PostProcessing = Globals.content.Load<Effect>("shaders/postprocessing/postprocessing");
        public static EffectParameter PostProcessingParameter_ScreenTexture = PostProcessing.Parameters["ScreenTexture"];
        public static EffectParameter PostProcessingParameter_ChromaticAbberationStrength = PostProcessing.Parameters["ChromaticAbberationStrength"];
        public static EffectParameter PostProcessingParameter_SCurveStrength = PostProcessing.Parameters["SCurveStrength"];
        public static EffectParameter PostProcessingParameter_WhitePoint = PostProcessing.Parameters["WhitePoint"];
        public static EffectParameter PostProcessingParameter_PowExposure = PostProcessing.Parameters["PowExposure"];
        public static EffectTechnique PostProcessingTechnique_VignetteChroma = PostProcessing.Techniques["VignetteChroma"];
        public static EffectTechnique PostProcessingTechnique_Base = PostProcessing.Techniques["Base"];


        //Hologram Effect
        public static Effect HologramEffect = Globals.content.Load<Effect>("Shaders/Hologram/HologramEffect");
        public static EffectParameter HologramEffectParameter_World = HologramEffect.Parameters["World"];
        public static EffectParameter HologramEffectParameter_WorldViewProj = HologramEffect.Parameters["WorldViewProj"];

        //Emissive Effect
        public static Effect EmissiveEffect = Globals.content.Load<Effect>("Shaders/Emissive/EmissiveDraw");
        public static EffectParameter EmissiveEffectParameter_World = EmissiveEffect.Parameters["World"];
        public static EffectParameter EmissiveEffectParameter_ViewProj = EmissiveEffect.Parameters["ViewProjection"];
        public static EffectParameter EmissiveEffectParameter_WorldViewProj = EmissiveEffect.Parameters["WorldViewProj"];
        public static EffectParameter EmissiveEffectParameter_InvertViewProj = EmissiveEffect.Parameters["InvertViewProjection"];
        public static EffectParameter EmissiveEffectParameter_Origin = EmissiveEffect.Parameters["Origin"];
        public static EffectParameter EmissiveEffectParameter_CameraPosition = EmissiveEffect.Parameters["CameraPosition"];
        public static EffectParameter EmissiveEffectParameter_Size = EmissiveEffect.Parameters["Size"];
        public static EffectParameter EmissiveEffectParameter_NormalMap = EmissiveEffect.Parameters["NormalMap"];
        public static EffectParameter EmissiveEffectParameter_DepthMap = EmissiveEffect.Parameters["DepthMap"];
        public static EffectParameter EmissiveEffectParameter_EmissiveMap = EmissiveEffect.Parameters["EmissiveMap"];
        public static EffectParameter EmissiveEffectParameter_Resolution = EmissiveEffect.Parameters["Resolution"];
        public static EffectParameter EmissiveEffectParameter_EmissiveColor = EmissiveEffect.Parameters["EmissiveColor"];
        public static EffectParameter EmissiveEffectParameter_EmissiveStrength = EmissiveEffect.Parameters["EmissiveStrength"];
        public static EffectParameter EmissiveEffectParameter_Time = EmissiveEffect.Parameters["Time"];

        public static EffectTechnique EmissiveEffectTechnique_DrawEmissiveBuffer = EmissiveEffect.Techniques["DrawEmissiveBuffer"];
        public static EffectTechnique EmissiveEffectTechnique_DrawEmissiveSpecularEffect = EmissiveEffect.Techniques["DrawEmissiveSpecularEffect"];
        public static EffectTechnique EmissiveEffectTechnique_DrawEmissiveDiffuseEffect = EmissiveEffect.Techniques["DrawEmissiveDiffuseEffect"];


        //ScreenSpaceReflection Effect

        public static Effect ScreenSpaceReflectionEffect = Globals.content.Load<Effect>("Shaders/ScreenSpace/ScreenSpaceReflections");

        public static EffectParameter ScreenSpaceReflectionParameter_DepthMap = ScreenSpaceReflectionEffect.Parameters["DepthMap"];
        public static EffectParameter ScreenSpaceReflectionParameter_NormalMap = ScreenSpaceReflectionEffect.Parameters["NormalMap"];
        public static EffectParameter ScreenSpaceReflectionParameter_TargetMap = ScreenSpaceReflectionEffect.Parameters["TargetMap"];
        public static EffectParameter ScreenSpaceReflectionParameter_Resolution = ScreenSpaceReflectionEffect.Parameters["resolution"];
        public static EffectParameter ScreenSpaceReflectionParameter_Projection = ScreenSpaceReflectionEffect.Parameters["Projection"];
        public static EffectParameter ScreenSpaceReflectionParameter_Time = ScreenSpaceReflectionEffect.Parameters["Time"];
        public static EffectParameter ScreenSpaceReflectionParameter_FrustumCorners = ScreenSpaceReflectionEffect.Parameters["FrustumCorners"];
        public static EffectParameter ScreenSpaceReflectionParameter_FarClip = ScreenSpaceReflectionEffect.Parameters["FarClip"];
        public static EffectParameter ScreenSpaceReflectionParameter_NoiseMap = ScreenSpaceReflectionEffect.Parameters["NoiseMap"];

        public static EffectTechnique ScreenSpaceReflectionTechnique_Default = ScreenSpaceReflectionEffect.Techniques["Default"];
        public static EffectTechnique ScreenSpaceReflectionTechnique_Taa = ScreenSpaceReflectionEffect.Techniques["TAA"];


        //Screen Space Ambient Occlusion Effect

        public static Effect ScreenSpaceEffect = Globals.content.Load<Effect>("Shaders/ScreenSpace/ScreenSpaceAO");

        public static EffectParameter ScreenSpaceEffectParameter_SSAOMap = ScreenSpaceEffect.Parameters["SSAOMap"];
        public static EffectParameter ScreenSpaceEffectParameter_NormalMap = ScreenSpaceEffect.Parameters["NormalMap"];
        public static EffectParameter ScreenSpaceEffectParameter_DepthMap = ScreenSpaceEffect.Parameters["DepthMap"];
        public static EffectParameter ScreenSpaceEffectParameter_CameraPosition = ScreenSpaceEffect.Parameters["CameraPosition"];
        public static EffectParameter ScreenSpaceEffectParameter_InverseViewProjection = ScreenSpaceEffect.Parameters["InverseViewProjection"];
        public static EffectParameter ScreenSpaceEffectParameter_Projection = ScreenSpaceEffect.Parameters["Projection"];
        public static EffectParameter ScreenSpaceEffectParameter_ViewProjection = ScreenSpaceEffect.Parameters["ViewProjection"];

        public static EffectParameter ScreenSpaceEffect_FalloffMin = ScreenSpaceEffect.Parameters["FalloffMin"];
        public static EffectParameter ScreenSpaceEffect_FalloffMax = ScreenSpaceEffect.Parameters["FalloffMax"];
        public static EffectParameter ScreenSpaceEffect_Samples = ScreenSpaceEffect.Parameters["Samples"];
        public static EffectParameter ScreenSpaceEffect_Strength = ScreenSpaceEffect.Parameters["Strength"];
        public static EffectParameter ScreenSpaceEffect_SampleRadius = ScreenSpaceEffect.Parameters["SampleRadius"];
        public static EffectParameter ScreenSpaceEffectParameter_InverseResolution = ScreenSpaceEffect.Parameters["InverseResolution"];
        public static EffectParameter ScreenSpaceEffectParameter_FrustumCorners = ScreenSpaceEffect.Parameters["FrustumCorners"];

        public static EffectTechnique ScreenSpaceEffectTechnique_SSAO = ScreenSpaceEffect.Techniques["SSAO"];
        public static EffectTechnique ScreenSpaceEffectTechnique_BlurHorizontal = ScreenSpaceEffect.Techniques["BilateralHorizontal"];
        public static EffectTechnique ScreenSpaceEffectTechnique_BlurVertical = ScreenSpaceEffect.Techniques["BilateralVertical"];


        //Gaussian Blur
        public static Effect GaussianBlurEffect = Globals.content.Load<Effect>("Shaders/ScreenSpace/GaussianBlur");
        public static EffectParameter GaussianBlurEffectParameter_InverseResolution = GaussianBlurEffect.Parameters["InverseResolution"];
        public static EffectParameter GaussianBlurEffectParameter_TargetMap = GaussianBlurEffect.Parameters["TargetMap"];


        //DeferredCompose

        public static Effect DeferredCompose = Globals.content.Load<Effect>("Shaders/Deferred/DeferredCompose");

        public static EffectParameter DeferredComposeEffectParameter_ColorMap = DeferredCompose.Parameters["colorMap"];
        public static EffectParameter DeferredComposeEffectParameter_diffuseLightMap = DeferredCompose.Parameters["diffuseLightMap"];
        public static EffectParameter DeferredComposeEffectParameter_specularLightMap = DeferredCompose.Parameters["specularLightMap"];
        public static EffectParameter DeferredComposeEffectParameter_volumeLightMap = DeferredCompose.Parameters["volumeLightMap"];
        public static EffectParameter DeferredComposeEffectParameter_HologramMap = DeferredCompose.Parameters["HologramMap"];
        public static EffectParameter DeferredComposeEffectParameter_SSAOMap = DeferredCompose.Parameters["SSAOMap"];
        public static EffectParameter DeferredComposeEffectParameter_LinearMap = DeferredCompose.Parameters["LinearMap"];
        public static EffectParameter DeferredComposeEffectParameter_SSRMap = DeferredCompose.Parameters["SSRMap"];
        public static EffectParameter DeferredComposeEffectParameter_UseSSAO = DeferredCompose.Parameters["useSSAO"];

        public static EffectTechnique DeferredComposeTechnique_NonLinear = DeferredCompose.Techniques["TechniqueNonLinear"];
        public static EffectTechnique DeferredComposeTechnique_Linear = DeferredCompose.Techniques["TechniqueLinear"];

        //DeferredClear
        public static Effect DeferredClear = Globals.content.Load<Effect>("Shaders/Deferred/DeferredClear");

        //Directional light

        public static Effect deferredDirectionalLight = Globals.content.Load<Effect>("Shaders/Deferred/DeferredDirectionalLight");

        public static EffectTechnique deferredDirectionalLightUnshadowed = deferredDirectionalLight.Techniques["Unshadowed"];
        public static EffectTechnique deferredDirectionalLightSSShadowed = deferredDirectionalLight.Techniques["SSShadowed"];
        public static EffectTechnique deferredDirectionalLightShadowed = deferredDirectionalLight.Techniques["Shadowed"];
        public static EffectTechnique deferredDirectionalLightShadowOnly = deferredDirectionalLight.Techniques["ShadowOnly"];

        public static EffectParameter deferredDirectionalLightParameterViewProjection = deferredDirectionalLight.Parameters["ViewProjection"];
        public static EffectParameter deferredDirectionalLightParameterFrustumCorners = deferredDirectionalLight.Parameters["FrustumCorners"];
        public static EffectParameter deferredDirectionalLightParameterCameraPosition = deferredDirectionalLight.Parameters["cameraPosition"];
        public static EffectParameter deferredDirectionalLightParameterInverseViewProjection = deferredDirectionalLight.Parameters["InvertViewProjection"];
        public static EffectParameter deferredDirectionalLightParameterLightViewProjection = deferredDirectionalLight.Parameters["LightViewProjection"];
        public static EffectParameter deferredDirectionalLightParameterLightView = deferredDirectionalLight.Parameters["LightView"];
        public static EffectParameter deferredDirectionalLightParameterLightFarClip = deferredDirectionalLight.Parameters["LightFarClip"];

        public static EffectParameter deferredDirectionalLightParameter_LightColor = deferredDirectionalLight.Parameters["lightColor"];
        public static EffectParameter deferredDirectionalLightParameter_LightIntensity = deferredDirectionalLight.Parameters["lightIntensity"];
        public static EffectParameter deferredDirectionalLightParameter_LightDirection = deferredDirectionalLight.Parameters["LightVector"];
        public static EffectParameter deferredDirectionalLightParameter_ShadowFiltering = deferredDirectionalLight.Parameters["ShadowFiltering"];
        public static EffectParameter deferredDirectionalLightParameter_ShadowMapSize = deferredDirectionalLight.Parameters["ShadowMapSize"];

        public static EffectParameter deferredDirectionalLightParameter_AlbedoMap = deferredDirectionalLight.Parameters["AlbedoMap"];
        public static EffectParameter deferredDirectionalLightParameter_NormalMap = deferredDirectionalLight.Parameters["NormalMap"];
        public static EffectParameter deferredDirectionalLightParameter_DepthMap = deferredDirectionalLight.Parameters["DepthMap"];
        public static EffectParameter deferredDirectionalLightParameter_ShadowMap = deferredDirectionalLight.Parameters["ShadowMap"];
        public static EffectParameter deferredDirectionalLightParameter_SSShadowMap = deferredDirectionalLight.Parameters["SSShadowMap"];


        //Point Light
        public static Effect deferredPointLight = Globals.content.Load<Effect>("Shaders/Deferred/DeferredPointLight");

        public static EffectTechnique deferredPointLightUnshadowed = deferredPointLight.Techniques["Unshadowed"];
        public static EffectTechnique deferredPointLightUnshadowedVolumetric = deferredPointLight.Techniques["UnshadowedVolume"];
        public static EffectTechnique deferredPointLightShadowed = deferredPointLight.Techniques["Shadowed"];
        public static EffectTechnique deferredPointLightShadowedVolumetric = deferredPointLight.Techniques["ShadowedVolume"];
        public static EffectTechnique deferredPointLightWriteStencil = deferredPointLight.Techniques["WriteStencilMask"];

        public static EffectParameter deferredPointLightParameterShadowMap = deferredPointLight.Parameters["ShadowMap"];

        public static EffectParameter deferredPointLightParameterResolution = deferredPointLight.Parameters["Resolution"];
        public static EffectParameter deferredPointLightParameter_WorldView = deferredPointLight.Parameters["WorldView"];
        public static EffectParameter deferredPointLightParameter_WorldViewProjection = deferredPointLight.Parameters["WorldViewProj"];
        public static EffectParameter deferredPointLightParameter_InverseView = deferredPointLight.Parameters["InverseView"];

        public static EffectParameter deferredPointLightParameter_LightPosition = deferredPointLight.Parameters["lightPosition"];
        public static EffectParameter deferredPointLightParameter_LightColor = deferredPointLight.Parameters["lightColor"];
        public static EffectParameter deferredPointLightParameter_LightRadius = deferredPointLight.Parameters["lightRadius"];
        public static EffectParameter deferredPointLightParameter_LightIntensity = deferredPointLight.Parameters["lightIntensity"];
        public static EffectParameter deferredPointLightParameter_ShadowMapSize = deferredPointLight.Parameters["ShadowMapSize"];
        public static EffectParameter deferredPointLightParameter_ShadowMapRadius = deferredPointLight.Parameters["ShadowMapRadius"];
        public static EffectParameter deferredPointLightParameter_Inside = deferredPointLight.Parameters["inside"];
        public static EffectParameter deferredPointLightParameter_Time = deferredPointLight.Parameters["Time"];
        public static EffectParameter deferredPointLightParameter_FarClip = deferredPointLight.Parameters["FarClip"];
        public static EffectParameter deferredPointLightParameter_LightVolumeDensity = deferredPointLight.Parameters["lightVolumeDensity"];

        public static EffectParameter deferredPointLightParameter_NoiseMap = deferredPointLight.Parameters["NoiseMap"];
        public static EffectParameter deferredPointLightParameter_AlbedoMap = deferredPointLight.Parameters["AlbedoMap"];
        public static EffectParameter deferredPointLightParameter_NormalMap = deferredPointLight.Parameters["NormalMap"];
        public static EffectParameter deferredPointLightParameter_DepthMap = deferredPointLight.Parameters["DepthMap"];


        public static void Load(ContentManager content)
        {
        }
    }

}
