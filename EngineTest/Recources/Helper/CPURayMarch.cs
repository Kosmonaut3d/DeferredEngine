using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineTest.Renderer;
using EngineTest.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;

namespace EngineTest.Recources.Helper
{
    public class CPURayMarch
    {
        private GraphicsDevice _graphicsDevice;

        private Vector3 startPosition;
        private Vector3 startNormal;
        private Vector3 reflectVector;

        List<Vector3> samplePositions = new List<Vector3>();

        public void Initialize(GraphicsDevice graphics)
        {
            _graphicsDevice = graphics;
        }

        public void Calculate(RenderTarget2D depthMap, RenderTarget2D normalMap, Matrix InverseViewProjection, Matrix viewProjection, Camera camera)
        { 

            samplePositions.Clear();
            
            Vector2 texCoord = new Vector2(0.5f, 0.5f); //middle of the screen

            //construct VS

            float depthVal = 1 - SampleFloat(depthMap, texCoord);

            startNormal = decode(SampleHalfFloat4(normalMap, texCoord).Xyz());
                
            Vector4 positionVS = new Vector4();

            positionVS.X = texCoord.X*2 - 1;
            positionVS.Y = -(texCoord.Y*2 - 1);

            positionVS.W = 1.0f;
            positionVS.Z = depthVal;

            Vector4 positionWS = Vector4.Transform(positionVS, InverseViewProjection);

            positionWS /= positionWS.W;

            startPosition = positionWS.Xyz();

            Vector3 incident = Vector3.Normalize(startPosition - camera.Position);

            reflectVector = Vector3.Reflect(incident, startNormal);

            Vector4 samplePositionVS = Vector4.Transform(positionWS + new Vector4(reflectVector, 0), viewProjection);
            samplePositionVS /= samplePositionVS.W;

            Vector4 Offset = (samplePositionVS - positionVS);

            float xMultiplier = 1;
            //Lets go to the end of the screen
            if (Offset.X > 0)
            {
                xMultiplier = (1 - positionVS.X)/Offset.X;
            }
            else
            {
                xMultiplier = (-1 - positionVS.X) / Offset.X;
            }

            int samples = 10;
            for (int i = 0; i < samples; i++)
            {
                Vector4 samplePosition = samplePositionVS + Offset*i*2;

                Vector4 sampleWorld = Vector4.Transform(samplePosition, InverseViewProjection);
                sampleWorld /= sampleWorld.W;

                samplePositions.Add(sampleWorld.Xyz());
            }


        }

        public void Draw()
        {
            LineHelperManager.AddLineStartEnd(startPosition, startPosition+startNormal*10, 1);
            LineHelperManager.AddLineStartEnd(startPosition, startPosition + reflectVector * 10, 1, Color.AliceBlue, Color.Aqua);

            for (int index = 0; index < samplePositions.Count-1; index++)
            {
                Vector3 v0 = samplePositions[index];
                Vector3 v1 = samplePositions[index + 1];
                LineHelperManager.AddLineStartEnd(v0, v1, 1, Color.Yellow, Color.Red);
            }
        }

        private Vector3 decode(Vector3 input)
        {
            return 2.0f * input - Vector3.One;
        }

        private float SampleFloat(Texture2D tex, Vector2 texCoord)
        {

            Rectangle sourceRectangle =
            new Rectangle((int) (texCoord.X * tex.Width), (int) (texCoord.Y * tex.Height),  1, 1);

            float[] retrievedColor = new float[1];

            tex.GetData<float>(0, sourceRectangle, retrievedColor, 0, 1);

            return retrievedColor[0]; //retrievedColor[0].ToVector4();
        }

        private HalfVector4 SampleHalfFloat4(Texture2D tex, Vector2 texCoord)
        {

            Rectangle sourceRectangle =
            new Rectangle((int)(texCoord.X * tex.Width), (int)(texCoord.Y * tex.Height), 1, 1);

            HalfVector4[] retrievedColor = new HalfVector4[1];

            tex.GetData<HalfVector4>(0, sourceRectangle, retrievedColor, 0, 1);

            return retrievedColor[0]; //retrievedColor[0].ToVector4();
        }

    }
}
