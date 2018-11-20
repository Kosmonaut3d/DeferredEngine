using System;
// ReSharper disable InconsistentNaming

namespace DeferredEngine.Recources
{
    public static class GameSettings
    {
        //Default & Display settings
        public static int g_screenwidth = 1280;
        public static int g_screenheight = 720;
        public static bool g_vsync = false;
        public static int g_fixedfps = 0;
        public static int u_showdisplayinfo = 3;
        public static bool p_physics = false;
        public static Renderer.Renderer.RenderModes g_rendermode = Renderer.Renderer.RenderModes.Deferred;

        //Editor
        public static bool e_enableeditor = true;
        public static bool e_drawoutlines = true;
        public static bool e_drawboundingbox = true;

        //UI
        public static bool ui_enabled = true;

        //Renderer

        //debug
        public static bool d_drawlines = true;

        //Default Material
        public static bool d_defaultmaterial = false;
        public static float m_defaultroughness = 0.5f;
        
        //Settings
        public static float g_farplane = 500;
        public static bool g_cpusort = true;
        public static bool g_cpuculling = true;
        public static bool g_batchbymaterial = false; //Note this must be activated before the application is started.

        //Profiler
        public static bool d_profiler = false;

        //Environment mapping
        public static bool g_environmentmapping = true;
        public static bool g_envmapupdateeveryframe = false;
        public static int g_envmapresolution = 1024;

        //Shadow Settings
        public static int g_shadowforcefiltering = 0; //1 = PCF, 2 3 better PCF  4 = Poisson, 5 = VSM;
        public static bool g_shadowforcescreenspace = false;

        //Deferred Decals
        public static bool g_drawdecals = true;

        //Forward pass
        public static bool g_forwardenable = true;

        //Temporal AntiAliasing
        public static bool g_taa = true;
        public static int g_taa_jittermode = 2;
        public static bool g_taa_tonemapped = true;

        //Screen Space Ambient Occlusion

        public static bool g_ssao_blur = true;

        private static bool _g_ssao_draw = true;
        public static bool g_ssao_draw
        {
            get { return _g_ssao_draw; }
            set
            {
                _g_ssao_draw = value;
                Shaders.DeferredCompose.Parameters["useSSAO"].SetValue(_g_ssao_draw);
            }
        }
        
        private static float _g_ssao_falloffmin = 0.001f;
        public static float g_ssao_falloffmin
        {
            get { return _g_ssao_falloffmin; }
            set
            {
                _g_ssao_falloffmin = value;
                Shaders.ScreenSpaceEffect_FalloffMin.SetValue(value);
            }
        }
        
        private static float _g_ssao_falloffmax = 0.03f;
        public static float g_ssao_falloffmax
        {
            get { return _g_ssao_falloffmax; }
            set
            {
                _g_ssao_falloffmax = value;
                Shaders.ScreenSpaceEffect_FalloffMax.SetValue(value);
            }
        }
        
        private static int _g_ssao_samples = 8;
        public static int g_ssao_samples
        {
            get { return _g_ssao_samples; }
            set
            {
                _g_ssao_samples = value;
                Shaders.ScreenSpaceEffect_Samples.SetValue(value);
            }
        }

        private static float _g_ssao_radius = 30;
        public static float g_ssao_radius
        {
            get { return _g_ssao_radius; }
            set
            {
                _g_ssao_radius = value;
                Shaders.ScreenSpaceEffect_SampleRadius.SetValue(value);
            }
        }

        private static float _g_ssao_strength = 0.5f;
        public static float g_ssao_strength
        {
            get { return _g_ssao_strength; }
            set
            {
                _g_ssao_strength = value;
                Shaders.ScreenSpaceEffect_Strength.SetValue(value);
            }
        }



        public static float g_BloomThreshold = 0.0f;

        // Emissive 

        //public static bool g_EmissiveDraw = true;
        //public static bool g_EmissiveDrawDiffuse = true;
        //public static bool g_EmissiveDrawSpecular = true;
        //public static bool g_EmissiveNoise = false;
        //public static float g_EmissiveDrawFOVFactor = 2;

        ////Whether or not materials' lighting scales with strength
        //public static bool g_EmissiveMaterialeSizeStrengthScaling = true;

        //private static int _g_EmissiveMaterialSamples = 8;
        //public static int g_EmissiveMaterialSamples
        //{
        //    get { return _g_EmissiveMaterialSamples; }
        //    set
        //    {
        //        _g_EmissiveMaterialSamples = value;
        //        Shaders.EmissiveEffect.Parameters["Samples"].SetValue(_g_EmissiveMaterialSamples);
        //    }
        //}




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
                Shaders.PostProcessing.CurrentTechnique = Shaders.PostProcessingTechnique_Base;
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

        private static float _exposure = 0.75f;
        public static float Exposure
        {
            get { return _exposure; }
            set
            {
                _exposure = value;
                Shaders.PostProcessingParameter_PowExposure.SetValue((float) Math.Pow(2,_exposure));
            }
        }

        public static bool g_ColorGrading = true;

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
            }
        }

        private static float _g_SSReflection_FireflyThreshold = 1.75f;

        public static float g_SSReflection_FireflyThreshold
        {
            get { return _g_SSReflection_FireflyThreshold; }
            set
            {
                _g_SSReflection_FireflyThreshold = value;
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
        public static bool e_CPURayMarch = false;
        public static bool g_ClearGBuffer = true;
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

        
        public static float ShadowBias = 0.005f;
        public static int sdf_threads = 4;
        public static bool sdf_cpu = false;
        public static bool sdf_draw = true;
        public static bool sdf_drawdistance = false;
        public static bool sdf_debug = false;
        public static bool sdf_subsurface = true;
        public static bool sdf_drawvolume = false;
        public static bool sdf_regenerate;
        public static bool d_drawnothing = false;
        public static bool e_saveBoundingBoxes = true;
        public static bool d_hotreloadshaders = true;

        public static void ApplySettings()
        {
            ApplySSAO();
            
            g_ssao_draw = true;
            g_PostProcessing = true;
            g_taa = true;
            g_environmentmapping = true;

            g_SSReflection = true;
            g_SSReflections_Samples = msamples;
            g_SSReflections_RefinementSamples = ssamples;
            g_SSReflection_FireflyReduction = _g_SSReflection_FireflyReduction;
            g_SSReflection_FireflyThreshold = _g_SSReflection_FireflyThreshold;

            g_Linear = _g_Linear;

            d_defaultmaterial = false;
            SCurveStrength = _sCurveStrength;
            Exposure = _exposure;
            ChromaticAbberationStrength = _chromaticAbberationStrength;
            
        }

        public static void ApplySSAO()
        {
            g_ssao_falloffmax = _g_ssao_falloffmax;
            g_ssao_falloffmin = _g_ssao_falloffmin;
            g_ssao_radius = _g_ssao_radius;
            g_ssao_samples = g_ssao_samples;
            g_ssao_strength = g_ssao_strength;
            g_ssao_draw = _g_ssao_draw;
        }
    }
}
