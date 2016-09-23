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
    }
}
