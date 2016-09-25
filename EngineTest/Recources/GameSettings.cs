using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineTest.Recources
{
    public static class GameSettings
    {
        public static bool g_SSR = false;
        public static float g_FarPlane = 500;
        public static float g_supersampling = 1;
        public static int ShowDisplayInfo = 3;

        public static Renderer.Renderer.RenderModes g_RenderMode = Renderer.Renderer.RenderModes.Deferred;
        public static bool g_CPU_Culling = true;

        public static bool g_BatchByMaterial = false; //Note this must be activated before the application is started.

        public static bool g_CPU_Sort = true;
        public static bool g_EnvironmentMapping = true;
        public static bool g_EnvironmentMappingEveryFrame = false;
        public static float t_color = 0.25f;

        public static int g_ScreenWidth = 1280;
        public static int g_ScreenHeight = 800;

        private static float _ssao_falloffmin = 0.001f;
        private static float _ssao_falloffmax = 0.002f;
        private static int _ssao_samples = 8;
        private static float _ssao_sampleradius = 0.08f;
        private static float _ssao_strength = 10;
        public static bool ssao_Blur = true;
        private static bool _ssao_active = false;

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
