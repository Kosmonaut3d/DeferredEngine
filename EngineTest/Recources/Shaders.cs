using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{
    //As suggested here http://community.monogame.net/t/deferred-engine-playground-download/8180/283?u=kosmonautgames
    //the whole global shaders is shortened to load early without the need for a seperate load function
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
        public static readonly Effect ReconstructDepth = Globals.content.Load<Effect>("Shaders/ScreenSpace/ReconstructDepth");
        public static readonly EffectParameter ReconstructDepthParameter_DepthMap = ReconstructDepth.Parameters["DepthMap"];
        public static readonly EffectParameter ReconstructDepthParameter_Projection = ReconstructDepth.Parameters["Projection"];
        public static readonly EffectParameter ReconstructDepthParameter_FarClip = ReconstructDepth.Parameters["FarClip"];
        public static readonly EffectParameter ReconstructDepthParameter_FrustumCorners = ReconstructDepth.Parameters["FrustumCorners"];

        //Id Generator
        public static readonly Effect IdRenderEffect = Globals.content.Load<Effect>("Shaders/Editor/IdRender");
        public static readonly EffectParameter IdRenderEffectParameterWorldViewProj = IdRenderEffect.Parameters["WorldViewProj"];
        public static readonly EffectParameter IdRenderEffectParameterColorId = IdRenderEffect.Parameters["ColorId"];
        public static readonly EffectParameter IdRenderEffectParameterOutlineSize = IdRenderEffect.Parameters["OutlineSize"];
        public static readonly EffectParameter IdRenderEffectParameterWorld = IdRenderEffect.Parameters["World"];

        public static readonly EffectPass IdRenderEffectDrawId = IdRenderEffect.Techniques["DrawId"].Passes[0];
        public static readonly EffectPass IdRenderEffectDrawOutline = IdRenderEffect.Techniques["DrawOutline"].Passes[0];

        //Billboard Renderer
        public static readonly Effect BillboardEffect = Globals.content.Load<Effect>("Shaders/Editor/BillboardEffect");

        public static readonly EffectParameter BillboardEffectParameter_WorldViewProj = BillboardEffect.Parameters["WorldViewProj"];
        public static readonly EffectParameter BillboardEffectParameter_WorldView = BillboardEffect.Parameters["WorldView"];
        public static readonly EffectParameter BillboardEffectParameter_AspectRatio = BillboardEffect.Parameters["AspectRatio"];
        public static readonly EffectParameter BillboardEffectParameter_FarClip = BillboardEffect.Parameters["FarClip"];
        public static readonly EffectParameter BillboardEffectParameter_Texture = BillboardEffect.Parameters["Texture"];
        public static readonly EffectParameter BillboardEffectParameter_DepthMap = BillboardEffect.Parameters["DepthMap"];
        public static readonly EffectParameter BillboardEffectParameter_IdColor = BillboardEffect.Parameters["IdColor"];

        public static readonly EffectTechnique BillboardEffectTechnique_Billboard = BillboardEffect.Techniques["Billboard"];
        public static readonly EffectTechnique BillboardEffectTechnique_Id = BillboardEffect.Techniques["Id"];


        //Temporal AntiAliasing


        //Vignette and CA
        public static readonly Effect PostProcessing = Globals.content.Load<Effect>("shaders/postprocessing/postprocessing");
        public static readonly EffectParameter PostProcessingParameter_ScreenTexture = PostProcessing.Parameters["ScreenTexture"];
        public static readonly EffectParameter PostProcessingParameter_ChromaticAbberationStrength = PostProcessing.Parameters["ChromaticAbberationStrength"];
        public static readonly EffectParameter PostProcessingParameter_SCurveStrength = PostProcessing.Parameters["SCurveStrength"];
        public static readonly EffectParameter PostProcessingParameter_WhitePoint = PostProcessing.Parameters["WhitePoint"];
        public static readonly EffectParameter PostProcessingParameter_PowExposure = PostProcessing.Parameters["PowExposure"];
        public static readonly EffectTechnique PostProcessingTechnique_VignetteChroma = PostProcessing.Techniques["VignetteChroma"];
        public static readonly EffectTechnique PostProcessingTechnique_Base = PostProcessing.Techniques["Base"];


        //Hologram Effect
        public static readonly Effect HologramEffect = Globals.content.Load<Effect>("Shaders/Hologram/HologramEffect");
        public static readonly EffectParameter HologramEffectParameter_World = HologramEffect.Parameters["World"];
        public static readonly EffectParameter HologramEffectParameter_WorldViewProj = HologramEffect.Parameters["WorldViewProj"];

        //Emissive Effect
        public static readonly Effect EmissiveEffect = Globals.content.Load<Effect>("Shaders/Emissive/EmissiveDraw");
        public static readonly EffectParameter EmissiveEffectParameter_World = EmissiveEffect.Parameters["World"];
        public static readonly EffectParameter EmissiveEffectParameter_ViewProj = EmissiveEffect.Parameters["ViewProjection"];
        public static readonly EffectParameter EmissiveEffectParameter_WorldViewProj = EmissiveEffect.Parameters["WorldViewProj"];
        public static readonly EffectParameter EmissiveEffectParameter_InvertViewProj = EmissiveEffect.Parameters["InvertViewProjection"];
        public static readonly EffectParameter EmissiveEffectParameter_Origin = EmissiveEffect.Parameters["Origin"];
        public static readonly EffectParameter EmissiveEffectParameter_CameraPosition = EmissiveEffect.Parameters["CameraPosition"];
        public static readonly EffectParameter EmissiveEffectParameter_Size = EmissiveEffect.Parameters["Size"];
        public static readonly EffectParameter EmissiveEffectParameter_NormalMap = EmissiveEffect.Parameters["NormalMap"];
        public static readonly EffectParameter EmissiveEffectParameter_DepthMap = EmissiveEffect.Parameters["DepthMap"];
        public static readonly EffectParameter EmissiveEffectParameter_EmissiveMap = EmissiveEffect.Parameters["EmissiveMap"];
        public static readonly EffectParameter EmissiveEffectParameter_Resolution = EmissiveEffect.Parameters["Resolution"];
        public static readonly EffectParameter EmissiveEffectParameter_EmissiveColor = EmissiveEffect.Parameters["EmissiveColor"];
        public static readonly EffectParameter EmissiveEffectParameter_EmissiveStrength = EmissiveEffect.Parameters["EmissiveStrength"];
        public static readonly EffectParameter EmissiveEffectParameter_Time = EmissiveEffect.Parameters["Time"];

        public static readonly EffectTechnique EmissiveEffectTechnique_DrawEmissiveBuffer = EmissiveEffect.Techniques["DrawEmissiveBuffer"];
        public static readonly EffectTechnique EmissiveEffectTechnique_DrawEmissiveSpecularEffect = EmissiveEffect.Techniques["DrawEmissiveSpecularEffect"];
        public static readonly EffectTechnique EmissiveEffectTechnique_DrawEmissiveDiffuseEffect = EmissiveEffect.Techniques["DrawEmissiveDiffuseEffect"];


        //ScreenSpaceReflection Effect

        public static readonly Effect ScreenSpaceReflectionEffect = Globals.content.Load<Effect>("Shaders/ScreenSpace/ScreenSpaceReflections");

        public static readonly EffectParameter ScreenSpaceReflectionParameter_DepthMap = ScreenSpaceReflectionEffect.Parameters["DepthMap"];
        public static readonly EffectParameter ScreenSpaceReflectionParameter_NormalMap = ScreenSpaceReflectionEffect.Parameters["NormalMap"];
        public static readonly EffectParameter ScreenSpaceReflectionParameter_TargetMap = ScreenSpaceReflectionEffect.Parameters["TargetMap"];
        public static readonly EffectParameter ScreenSpaceReflectionParameter_Resolution = ScreenSpaceReflectionEffect.Parameters["resolution"];
        public static readonly EffectParameter ScreenSpaceReflectionParameter_Projection = ScreenSpaceReflectionEffect.Parameters["Projection"];
        public static readonly EffectParameter ScreenSpaceReflectionParameter_Time = ScreenSpaceReflectionEffect.Parameters["Time"];
        public static readonly EffectParameter ScreenSpaceReflectionParameter_FrustumCorners = ScreenSpaceReflectionEffect.Parameters["FrustumCorners"];
        public static readonly EffectParameter ScreenSpaceReflectionParameter_FarClip = ScreenSpaceReflectionEffect.Parameters["FarClip"];
        public static readonly EffectParameter ScreenSpaceReflectionParameter_NoiseMap = ScreenSpaceReflectionEffect.Parameters["NoiseMap"];

        public static readonly EffectTechnique ScreenSpaceReflectionTechnique_Default = ScreenSpaceReflectionEffect.Techniques["Default"];
        public static readonly EffectTechnique ScreenSpaceReflectionTechnique_Taa = ScreenSpaceReflectionEffect.Techniques["TAA"];


        //Screen Space Ambient Occlusion Effect

        public static readonly Effect ScreenSpaceEffect = Globals.content.Load<Effect>("Shaders/ScreenSpace/ScreenSpaceAO");

        public static readonly EffectParameter ScreenSpaceEffectParameter_SSAOMap = ScreenSpaceEffect.Parameters["SSAOMap"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_NormalMap = ScreenSpaceEffect.Parameters["NormalMap"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_DepthMap = ScreenSpaceEffect.Parameters["DepthMap"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_CameraPosition = ScreenSpaceEffect.Parameters["CameraPosition"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_InverseViewProjection = ScreenSpaceEffect.Parameters["InverseViewProjection"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_Projection = ScreenSpaceEffect.Parameters["Projection"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_ViewProjection = ScreenSpaceEffect.Parameters["ViewProjection"];

        public static readonly EffectParameter ScreenSpaceEffect_FalloffMin = ScreenSpaceEffect.Parameters["FalloffMin"];
        public static readonly EffectParameter ScreenSpaceEffect_FalloffMax = ScreenSpaceEffect.Parameters["FalloffMax"];
        public static readonly EffectParameter ScreenSpaceEffect_Samples = ScreenSpaceEffect.Parameters["Samples"];
        public static readonly EffectParameter ScreenSpaceEffect_Strength = ScreenSpaceEffect.Parameters["Strength"];
        public static readonly EffectParameter ScreenSpaceEffect_SampleRadius = ScreenSpaceEffect.Parameters["SampleRadius"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_InverseResolution = ScreenSpaceEffect.Parameters["InverseResolution"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_AspectRatio = ScreenSpaceEffect.Parameters["AspectRatio"];
        public static readonly EffectParameter ScreenSpaceEffectParameter_FrustumCorners = ScreenSpaceEffect.Parameters["FrustumCorners"];

        public static readonly EffectTechnique ScreenSpaceEffectTechnique_SSAO = ScreenSpaceEffect.Techniques["SSAO"];
        public static readonly EffectTechnique ScreenSpaceEffectTechnique_BlurHorizontal = ScreenSpaceEffect.Techniques["BilateralHorizontal"];
        public static readonly EffectTechnique ScreenSpaceEffectTechnique_BlurVertical = ScreenSpaceEffect.Techniques["BilateralVertical"];


        //Gaussian Blur
        public static readonly Effect GaussianBlurEffect = Globals.content.Load<Effect>("Shaders/ScreenSpace/GaussianBlur");
        public static readonly EffectParameter GaussianBlurEffectParameter_InverseResolution = GaussianBlurEffect.Parameters["InverseResolution"];
        public static readonly EffectParameter GaussianBlurEffectParameter_TargetMap = GaussianBlurEffect.Parameters["TargetMap"];


        //DeferredCompose

        public static readonly Effect DeferredCompose = Globals.content.Load<Effect>("Shaders/Deferred/DeferredCompose");

        public static readonly EffectParameter DeferredComposeEffectParameter_ColorMap = DeferredCompose.Parameters["colorMap"];
        public static readonly EffectParameter DeferredComposeEffectParameter_NormalMap = DeferredCompose.Parameters["normalMap"];
        public static readonly EffectParameter DeferredComposeEffectParameter_diffuseLightMap = DeferredCompose.Parameters["diffuseLightMap"];
        public static readonly EffectParameter DeferredComposeEffectParameter_specularLightMap = DeferredCompose.Parameters["specularLightMap"];
        public static readonly EffectParameter DeferredComposeEffectParameter_volumeLightMap = DeferredCompose.Parameters["volumeLightMap"];
        public static readonly EffectParameter DeferredComposeEffectParameter_HologramMap = DeferredCompose.Parameters["HologramMap"];
        public static readonly EffectParameter DeferredComposeEffectParameter_SSAOMap = DeferredCompose.Parameters["SSAOMap"];
        public static readonly EffectParameter DeferredComposeEffectParameter_LinearMap = DeferredCompose.Parameters["LinearMap"];
        public static readonly EffectParameter DeferredComposeEffectParameter_SSRMap = DeferredCompose.Parameters["SSRMap"];
        public static readonly EffectParameter DeferredComposeEffectParameter_UseSSAO = DeferredCompose.Parameters["useSSAO"];

        public static readonly EffectTechnique DeferredComposeTechnique_NonLinear = DeferredCompose.Techniques["TechniqueNonLinear"];
        public static readonly EffectTechnique DeferredComposeTechnique_Linear = DeferredCompose.Techniques["TechniqueLinear"];

        //DeferredClear
        public static readonly Effect DeferredClear = Globals.content.Load<Effect>("Shaders/Deferred/DeferredClear");

        //Directional light

        public static readonly Effect deferredDirectionalLight = Globals.content.Load<Effect>("Shaders/Deferred/DeferredDirectionalLight");

        public static readonly EffectTechnique deferredDirectionalLightUnshadowed = deferredDirectionalLight.Techniques["Unshadowed"];
        public static readonly EffectTechnique deferredDirectionalLightSSShadowed = deferredDirectionalLight.Techniques["SSShadowed"];
        public static readonly EffectTechnique deferredDirectionalLightShadowed = deferredDirectionalLight.Techniques["Shadowed"];
        public static readonly EffectTechnique deferredDirectionalLightShadowOnly = deferredDirectionalLight.Techniques["ShadowOnly"];

        public static readonly EffectParameter deferredDirectionalLightParameterViewProjection = deferredDirectionalLight.Parameters["ViewProjection"];
        public static readonly EffectParameter deferredDirectionalLightParameterFrustumCorners = deferredDirectionalLight.Parameters["FrustumCorners"];
        public static readonly EffectParameter deferredDirectionalLightParameterCameraPosition = deferredDirectionalLight.Parameters["cameraPosition"];
        public static readonly EffectParameter deferredDirectionalLightParameterInverseViewProjection = deferredDirectionalLight.Parameters["InvertViewProjection"];
        public static readonly EffectParameter deferredDirectionalLightParameterLightViewProjection = deferredDirectionalLight.Parameters["LightViewProjection"];
        public static readonly EffectParameter deferredDirectionalLightParameterLightView = deferredDirectionalLight.Parameters["LightView"];
        public static readonly EffectParameter deferredDirectionalLightParameterLightFarClip = deferredDirectionalLight.Parameters["LightFarClip"];

        public static readonly EffectParameter deferredDirectionalLightParameter_LightColor = deferredDirectionalLight.Parameters["lightColor"];
        public static readonly EffectParameter deferredDirectionalLightParameter_LightIntensity = deferredDirectionalLight.Parameters["lightIntensity"];
        public static readonly EffectParameter deferredDirectionalLightParameter_LightDirection = deferredDirectionalLight.Parameters["LightVector"];
        public static readonly EffectParameter deferredDirectionalLightParameter_ShadowFiltering = deferredDirectionalLight.Parameters["ShadowFiltering"];
        public static readonly EffectParameter deferredDirectionalLightParameter_ShadowMapSize = deferredDirectionalLight.Parameters["ShadowMapSize"];

        public static readonly EffectParameter deferredDirectionalLightParameter_AlbedoMap = deferredDirectionalLight.Parameters["AlbedoMap"];
        public static readonly EffectParameter deferredDirectionalLightParameter_NormalMap = deferredDirectionalLight.Parameters["NormalMap"];
        public static readonly EffectParameter deferredDirectionalLightParameter_DepthMap = deferredDirectionalLight.Parameters["DepthMap"];
        public static readonly EffectParameter deferredDirectionalLightParameter_ShadowMap = deferredDirectionalLight.Parameters["ShadowMap"];
        public static readonly EffectParameter deferredDirectionalLightParameter_SSShadowMap = deferredDirectionalLight.Parameters["SSShadowMap"];


        //Point Light


        public static void Load(ContentManager content)
        {
        }
    }

}
