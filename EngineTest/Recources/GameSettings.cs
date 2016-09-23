using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineTest.Recources
{
    public static class GameSettings
    {
        public static bool g_SSR = true;
        public static float g_FarPlane = 500;
        public static bool g_supersample = false;
        public static int ShowDisplayInfo = 3;

        public static Renderer.Renderer.RenderModes g_RenderMode = Renderer.Renderer.RenderModes.Deferred;
        public static bool g_CPU_Culling = true;
    }
}
