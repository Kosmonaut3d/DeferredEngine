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
        public static Effect deferredSpotLight;
        public static EffectTechnique deferredSpotLightUnshadowed;
        public static EffectTechnique deferredSpotLightShadowed;
        public static EffectParameter deferredSpotLightParameterShadowMap;

        public static void Load(ContentManager content)
        {
            deferredSpotLight = content.Load<Effect>("DeferredSpotLight");

            deferredSpotLightUnshadowed = deferredSpotLight.Techniques["Unshadowed"];
            deferredSpotLightShadowed = deferredSpotLight.Techniques["Shadowed"];

            deferredSpotLightParameterShadowMap = deferredSpotLight.Parameters["shadowMap"];
        }
    }
}
