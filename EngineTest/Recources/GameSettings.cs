using System;

namespace DeferredEngine.Recources
{
    public static class GameSettings
    {
        public static float g_FarPlane = 500;
        public static float g_supersampling = 1;
        public static int ShowDisplayInfo = 3;

        public static Renderer.Renderer.RenderModes g_RenderMode = Renderer.Renderer.RenderModes.Deferred;
        public static bool g_CPU_Culling = true;

        public static bool g_BatchByMaterial = false; //Note this must be activated before the application is started.

        public static bool d_profiler = false;

        public static bool g_CPU_Sort = true;
        public static bool g_EnvironmentMapping = true;
        public static bool g_EnvironmentMappingEveryFrame = false;

        public static int g_ScreenWidth = 1280;
        public static int g_ScreenHeight = 800;

        public static bool g_EmissiveDraw = true;
        public static bool g_EmissiveDrawDiffuse = true;
        public static bool g_EmissiveDrawSpecular = true;
        public static bool g_EmissiveNoise = false;
        public static float g_EmissiveDrawFOVFactor = 2;

        public static bool p_Physics = false;

        public static float m_defaultRoughness = 0;

        //Whether or not materials' lighting scales with strength
        public static bool g_EmissiveMaterialeSizeStrengthScaling = true;

        private static int _g_EmissiveMaterialSamples = 8;
        public static int g_EmissiveMaterialSamples
        {
            get { return _g_EmissiveMaterialSamples; }
            set
            {
                _g_EmissiveMaterialSamples = value;
                Shaders.EmissiveEffect.Parameters["Samples"].SetValue(_g_EmissiveMaterialSamples);
            }
        }

        public static int g_ShadowForceFiltering = 0; //1 = PCF, 2 3 better PCF  4 = Poisson, 5 = VSM;
        public static bool g_ShadowForceScreenSpace = false;
        public static int g_ShadowBlurBudget = 1;

        private static float _ssao_falloffmin = 0.001f;
        private static float _ssao_falloffmax = 0.03f;
        private static int _ssao_samples = 8;
        private static float _ssao_sampleradius = 30;
        private static float _ssao_strength = 0.5f;
        public static bool ssao_Blur = true;
        private static bool _ssao_active = true;

        // Hologram
        private static bool _g_hologramUseGauss = true;
        public static bool g_HologramUseGauss
        {
            get { return _g_hologramUseGauss;}
            set
            {
                _g_hologramUseGauss = value;
                Shaders.DeferredCompose.Parameters["useGauss"].SetValue(value);
            }
        }

        public static bool g_HologramDraw = true;

        public static bool g_TemporalAntiAliasing = true;
        public static int g_TemporalAntiAliasingJitterMode = 2;
        public static bool g_TemporalAntiAliasingUseTonemap = false;
        public static bool Editor_enable = true;
        public static bool h_DrawLines = true;

        // PostProcessing

        private static float _chromaticAbberationStrength = 0.035f;
        public static float ChromaticAbberationStrength
        {
            get { return _chromaticAbberationStrength; }
            set
            {
                _chromaticAbberationStrength = value;
                Shaders.PostProcessingParameter_ChromaticAbberationStrength.SetValue(_chromaticAbberationStrength);

                if(_chromaticAbberationStrength<=0)
                Shaders.PostProcessing.CurrentTechnique = Shaders.PostProcessingTechnique_Vignette;
                else
                {
                    Shaders.PostProcessing.CurrentTechnique = Shaders.PostProcessingTechnique_VignetteChroma;
                }
            }
        }

        private static float _sCurveStrength = 0.05f;
        public static float SCurveStrength
        {
            get { return _sCurveStrength; }
            set
            {
                _sCurveStrength = value;
                Shaders.PostProcessingParameter_SCurveStrength.SetValue(_sCurveStrength);
            }
        }

        private static float _whitePoint = 1.1f;
        public static float WhitePoint
        {
            get { return _whitePoint; }
            set
            {
                _whitePoint = value;
                Shaders.PostProcessingParameter_WhitePoint.SetValue(_whitePoint);
            }
        }

        private static float _exposure = 0.25f;
        public static float Exposure
        {
            get { return _exposure; }
            set
            {
                _exposure = value;
                Shaders.PostProcessingParameter_Exposure.SetValue((float) Math.Pow(2,_exposure));
            }
        }

        // SSR

        private static bool _g_SSReflection = true;

        public static bool g_SSReflection
        {
            get { return _g_SSReflection;}
            set
            {
                _g_SSReflection = value;
            }
        }

        private static bool _g_SSReflection_FireflyReduction = true;

        public static bool g_SSReflection_FireflyReduction
        {
            get { return _g_SSReflection_FireflyReduction; }
            set
            {
                _g_SSReflection_FireflyReduction = value;
                Shaders.deferredEnvironmentParameter_FireflyReduction.SetValue(value);
            }
        }

        private static float _g_SSReflection_FireflyThreshold = 1.75f;

        public static float g_SSReflection_FireflyThreshold
        {
            get { return _g_SSReflection_FireflyThreshold; }
            set
            {
                _g_SSReflection_FireflyThreshold = value;
                Shaders.deferredEnvironmentParameter_FireflyThreshold.SetValue(_g_SSReflection_FireflyThreshold);
            }
        }


        private static bool _g_Linear = true;
        public static bool g_Linear
        {
            get { return _g_Linear; }
            set
            {
                _g_Linear = value;
                Shaders.DeferredCompose.CurrentTechnique = value
                    ? Shaders.DeferredComposeTechnique_Linear
                    : Shaders.DeferredComposeTechnique_NonLinear;
            }
        }

        private static bool _g_SSReflection_Taa = true;
        public static bool g_SSReflectionNoise = true;
        public static bool g_VolumetricLights = true;
        public static bool e_CPURayMarch = true;
        public static bool g_ClearGBuffer = true;
        public static bool d_defaultMaterial;
        public static bool g_PostProcessing = true;

        public static bool g_SSReflectionTaa
        {
            get { return _g_SSReflection_Taa;}
            set
            {
                _g_SSReflection_Taa = value;
                Shaders.ScreenSpaceReflectionEffect.CurrentTechnique = value
                    ? Shaders.ScreenSpaceReflectionTechnique_Taa
                    : Shaders.ScreenSpaceReflectionTechnique_Default;

                if (value) g_SSReflectionNoise = true;
            }
        }

        // Screen Space Ambient Occlusion

        public static bool ssao_Active
        {
            get { return _ssao_active; }
            set
            {
                _ssao_active = value;
                Shaders.DeferredCompose.Parameters["useSSAO"].SetValue(_ssao_active);
            }
        }

        public static float ssao_FalloffMin
        {
            get { return _ssao_falloffmin; }
            set
            {
                _ssao_falloffmin = value;
                Shaders.ScreenSpaceEffect_FalloffMin.SetValue(value);
            }
        }


        public static float ssao_FalloffMax
        {
            get { return _ssao_falloffmax; }
            set
            {
                _ssao_falloffmax = value;
                Shaders.ScreenSpaceEffect_FalloffMax.SetValue(value);
            }
        }


        public static int ssao_Samples
        {
            get { return _ssao_samples; }
            set
            {
                _ssao_samples = value;
                Shaders.ScreenSpaceEffect_Samples.SetValue(value);
            }
        }

        public static float ssao_SampleRadius
        {
            get { return _ssao_sampleradius; }
            set
            {
                _ssao_sampleradius = value;
                Shaders.ScreenSpaceEffect_SampleRadius.SetValue(value);
            }
        }

        public static float ssao_Strength
        {
            get { return _ssao_strength; }
            set
            {
                _ssao_strength = value;
                Shaders.ScreenSpaceEffect_Strength.SetValue(value);
            }
        }

        //5 and 5 are good, 3 and 3 are cheap
        private static int msamples = 3;
        public static int g_SSReflections_Samples
        {
            get { return msamples; }
            set
            {
                msamples = value;
                Shaders.ScreenSpaceReflectionEffect.Parameters["Samples"].SetValue(msamples);
            }
        }

        private static int ssamples = 3;
        public static int g_SSReflections_RefinementSamples
        {
            get { return ssamples; }
            set
            {
                ssamples = value;
                Shaders.ScreenSpaceReflectionEffect.Parameters["SecondarySamples"].SetValue(ssamples);
            }
        }

        private static float minThickness = 70;
        public static float g_SSReflections_MinThickness
        {
            get { return minThickness; }
            set
            {
                minThickness = value;
                Shaders.ScreenSpaceReflectionEffect.Parameters["MinimumThickness"].SetValue(minThickness);
            }
        }

        //private static float _g_TemporalAntiAliasingThreshold = 0.9f;
        public static int g_CubeMapResolution = 512;
        public static bool c_UseStringBuilder = true;
        public static int g_UseDepthStencilLightCulling = 1; //None, Depth, Depth+Stencil
        public static bool g_BloomEnable = true;

        public static float g_BloomRadius1 = 1.0f;
        public static float g_BloomRadius2 = 1.0f;
        public static float g_BloomRadius3 = 2.0f;
        public static float g_BloomRadius4 = 3.0f;
        public static float g_BloomRadius5 = 4.0f;

        public static float g_BloomStrength1 = 0.5f;
        public static float g_BloomStrength2 = 1;
        public static float g_BloomStrength3 = 1;
        public static float g_BloomStrength4 = 1.0f;
        public static float g_BloomStrength5 = 1.0f;

        public static float g_BloomThreshold = 0.0f;
        public static bool ui_DrawUI = true;




        public static float tr = -1;
        public static float ShadowBias = 0.005f;


        //public static float g_TemporalAntiAliasingThreshold
        //{
        //    get
        //    {
        //        return _g_TemporalAntiAliasingThreshold;
        //    }

        //    set
        //    {
        //        if (Math.Abs(_g_TemporalAntiAliasingThreshold - value) > 0.0001f)
        //        {
        //            _g_TemporalAntiAliasingThreshold = value;
        //            Shaders.TemporalAntiAliasingEffect_Threshold.SetValue(_g_TemporalAntiAliasingThreshold);
        //        }
        //    }
        //}

        public static void ApplySettings()
        {
            ApplySSAO();
            
            g_EmissiveDraw = false;
            ssao_Active = true;
            g_PostProcessing = true;
            g_TemporalAntiAliasing = true;
            g_EnvironmentMapping = true;

            g_SSReflection = true;
            g_SSReflections_Samples = msamples;
            g_SSReflections_RefinementSamples = ssamples;
            g_SSReflection_FireflyReduction = _g_SSReflection_FireflyReduction;
            g_SSReflection_FireflyThreshold = _g_SSReflection_FireflyThreshold;

            g_Linear = _g_Linear;

            d_defaultMaterial = false;
            SCurveStrength = _sCurveStrength;
            Exposure = _exposure;
            ChromaticAbberationStrength = _chromaticAbberationStrength;
            
        }

        public static void ApplySSAO()
        {
            ssao_FalloffMax = _ssao_falloffmax;
            ssao_FalloffMin = _ssao_falloffmin;
            ssao_SampleRadius = _ssao_sampleradius;
            ssao_Samples = ssao_Samples;
            ssao_Strength = ssao_Strength;
            ssao_Active = _ssao_active;
        }
    }
}
