using System;
using System.Collections.Generic;
using DeferredEngine.Entities;
using DeferredEngine.Logic;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.Helper.HelperGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace DeferredEngine.Recources.Helper
{
    public class CPURayMarch
    {
        private GraphicsDevice _graphicsDevice;

        private Vector3 startPosition;
        private Vector3 startNormal;
        private Vector3 reflectVector;

        readonly List<Vector3> samplePositions = new List<Vector3>();
        readonly List<Vector3> sampleTests = new List<Vector3>();

        public void Initialize(GraphicsDevice graphics)
        {
            _graphicsDevice = graphics;
        }

        Vector3 GetFrustumRay(Vector2 texCoord, Vector3[] currentCorners)
        {
            Vector3 x1 = Vector3.Lerp(currentCorners[0], currentCorners[1], texCoord.X);
            Vector3 x2 = Vector3.Lerp(currentCorners[2], currentCorners[3], texCoord.X);
            Vector3 outV = Vector3.Lerp(x1, x2, texCoord.Y);
            return outV;
        }


        float GetZWDepth(float linDepth, Matrix Projection)
        {
            float z = linDepth * Projection.M33 + Projection.M43;
            float w = linDepth*Projection.M34 + Projection.M44;
            return z/w; //linDepth*Projection.M33/Projection.M34;

            // return 
        }

        float GetLinDepth(float ZWDepth, Matrix Projection)
        {
            return //Projection.M34 / (ZWDepth - Projection.M33);
             ZWDepth/Projection.M33*Projection.M34;
        }

        Vector3 CreateWorldFromUV(Vector3 input, Matrix InverseViewProjection)
        {
            Vector4 positionVS;
            positionVS.X = input.X * 2 - 1;
            positionVS.Y = -(input.Y * 2 - 1);

            positionVS.W = 1.0f;
            positionVS.Z = input.Z;

            Vector4 positionWS = Vector4.Transform(positionVS, InverseViewProjection);
            positionWS /= positionWS.W;
            return positionWS.Xyz();
        }

        public void Calculate(RenderTarget2D depthMap, RenderTarget2D normalMap, Matrix Projection,
            Matrix inverseViewMatrix, Matrix inverseViewProjection, Camera camera, Vector3[] currentCorners)
        {
            Matrix inverseProjection = Matrix.Invert(Projection);
            
            samplePositions.Clear();
            sampleTests.Clear(); 

            Vector2 texCoord = Input.GetMousePositionNormalized();

            float linearDepth = SampleFloat(depthMap, texCoord);

            Vector3 positionVS = GetFrustumRay(texCoord, currentCorners) * linearDepth;
            
            Vector3 normal = decode(SampleHalfFloat4(normalMap, texCoord).Xyz());

            normal.Normalize();

            Vector3 incident = positionVS;
            incident.Normalize();

            Vector3 reflectVector = Vector3.Reflect(incident, normal);

            reflectVector.Normalize();

            //Vector4 startVectorVPS = Vector4.Transform(new Vector4(positionVS, 1), Projection);
            //startVectorVPS /= startVectorVPS.W;

            Vector4 reflectVectorVPS = Vector4.Transform(new Vector4(positionVS + reflectVector, 1), Projection);

            reflectVectorVPS /= reflectVectorVPS.W;

            Vector2 reflectVectorUV = 0.5f*(new Vector2(reflectVectorVPS.X, -reflectVectorVPS.Y) + Vector2.One);
            

            Vector3 rayOriginUV = new Vector3(texCoord, GetZWDepth( positionVS.Z , Projection));

            Vector3 rayUV = new Vector3(reflectVectorUV - texCoord, reflectVectorVPS.Z - rayOriginUV.Z);

            //float testrayUV = GetZWDepth(positionVS.Z + reflectVector.Z, Projection) - rayOriginUV.Z;

            int samples = 10;

            float xMultiplier;
            float yMultiplier;
            if (rayUV.X > 0)
            {
                xMultiplier = (1 - texCoord.X) / rayUV.X;
            }
            else
            {
                xMultiplier = (-texCoord.X) / rayUV.X;
            }

            if (rayUV.Y > 0)
            {
                yMultiplier = (1 - texCoord.Y) / rayUV.Y;
            }
            else
            {
                yMultiplier = (-texCoord.Y) / rayUV.Y;
            }

            float multiplier = Math.Min(xMultiplier, yMultiplier);
            rayUV *= multiplier;
            //rayVS *= multiplier;

            rayUV /= samples;
            //rayVS /= samples;
            
            for (int i = 1; i <= samples; i++)
            {
                //Vector3 rayPositionVS = rayOriginVS + rayVS*i;
                Vector3 rayPositionUV = rayOriginUV + rayUV*i;

                float sampleDepth = SampleFloat(depthMap, new Vector2(rayPositionUV.X, rayPositionUV.Y));

                Vector3 rayHit = GetFrustumRay(new Vector2(rayPositionUV.X, rayPositionUV.Y), currentCorners) * sampleDepth;
                //Vector3 rayHit2 = GetFrustumRay(new Vector2(rayPositionUV.X, rayPositionUV.Y), currentCorners) * 
                //                  GetLinDepth(rayPositionUV.Z, Projection) / -GameSettings.g_farplane;

                
                //Vector4 test3 = Vector4.Transform(new Vector4(rayHit, 1), Projection);
                //test3 /= test3.W;

                //float test4 = GetZWDepth(sampleDepth *-GameSettings.g_farplane, Projection);

                //samplePositions.Add(Vector3.Transform(rayHit2, inverseViewMatrix));
                //sampleTests.Add(Vector3.Transform(rayHit, inverseViewMatrix));
                //if (sampleDepth <= rayPositionUV.Z)
                //{
                //    sample
                //}
            }

            Vector3 positionWS = Vector3.Transform(positionVS, inverseViewMatrix);
            
            //Vector3 startWS = Vector3.Transform(Vector3.Zero, inverseViewMatrix);
            //samplePositions.Add(startWS);
            //samplePositions.Add(positionWS);

            startPosition = positionWS;
            startNormal = Vector3.Transform(normal - positionVS, inverseViewMatrix);

            //samplePositions.Add(positionWS);
            //samplePositions.Add(Vector3.Transform(positionVS + reflectVector*10, inverseViewMatrix));

            //samplePositions.Add(positionWS);
            //samplePositions.Add(Vector3.Transform(positionVS + normal * 10, inverseViewMatrix));
        }

        //samplePositions.Clear();
        //sampleTests.Clear();
        public void CalculateOld(RenderTarget2D depthMap, RenderTarget2D normalMap, Matrix InverseViewProjection, Matrix viewProjection, Camera camera, Vector3[] currentCorners)
        {
            //Vector2 texCoord = new Vector2(0.5f, 0.5f); //middle of the screen

            Vector2 texCoord = Input.GetMousePositionNormalized();
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

            float xMultiplier;
            float yMultiplier;
            //Lets go to the end of the screen
            if (Offset.X > 0)
            {
                xMultiplier = (1 - positionVS.X)/Offset.X;
            }
            else
            {
                xMultiplier = (-1 - positionVS.X) / Offset.X;
            }

            if (Offset.Y > 0)
            {
                yMultiplier = (1 - positionVS.Y) / Offset.Y;
            }
            else
            {
                yMultiplier = (-1 - positionVS.Y) / Offset.Y;
            }

            //what multiplier is smaller?

            float multiplier = xMultiplier < yMultiplier ? xMultiplier : yMultiplier;

            //int samples = 20;

            //Offset *= multiplier / samples;

            Offset *= multiplier;

            //float maxOffset = Math.Max(Math.Abs(Offset.X), Math.Abs(Offset.Y));


            int samples = 10;//(int)(maxOffset*10);

            Offset /= samples;
            
            for (int i = 0; i < samples; i++)
            {
                Vector4 rayPositionVS = samplePositionVS + Offset*i;

                //float2 sampleTexCoord = 0.5f * (float2(samplePositionVS.x, -samplePositionVS.y) + 1);

                Vector2 sampleTexCoord = 0.5f*(new Vector2(rayPositionVS.X , -rayPositionVS.Y ) + Vector2.One);

                float depthValRay = 1 - SampleFloat(depthMap, sampleTexCoord);
                
                Vector4 rayPositionDepthVS = new Vector4(rayPositionVS.X, rayPositionVS.Y, depthValRay, rayPositionVS.W );

                Vector4 rayDepthSampleWS = Vector4.Transform(rayPositionDepthVS, InverseViewProjection);
                rayDepthSampleWS /= rayDepthSampleWS.W;

                sampleTests.Add(rayDepthSampleWS.Xyz());


                Vector4 rayPositionWS = Vector4.Transform(rayPositionVS, InverseViewProjection);
                rayPositionWS /= rayPositionWS.W;

                samplePositions.Add(rayPositionWS.Xyz());


                if (depthValRay < rayPositionVS.Z) break;

            }


        }

        public void Draw()
        {
            if (samplePositions.Count <= 0) return;
            HelperGeometryManager.GetInstance().AddLineStartEnd(startPosition, startPosition+startNormal, 1);
            //LineHelperManager.AddLineStartEnd(startPosition, startPosition + reflectVector * 10, 1, Color.AliceBlue, Color.Aqua);

            for (int index = 0; index < samplePositions.Count-1; index++)
            {
                Vector3 v0 = samplePositions[index];
                Vector3 v1 = samplePositions[index + 1];
                HelperGeometryManager.GetInstance().AddLineStartEnd(v0, v1, 1, Color.Yellow, Color.Red);

                HelperGeometryManager.GetInstance().AddLineStartEnd(v0, sampleTests[index],1, Color.Blue, Color.Violet);
            }

            //if(samplePositions.Count>0)
            //LineHelperManager.AddLineStartEnd(samplePositions[samplePositions.Count-1], sampleTests[samplePositions.Count - 1], 1, Color.Blue, Color.Violet);
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

            tex.GetData(0, sourceRectangle, retrievedColor, 0, 1);

            return retrievedColor[0]; //retrievedColor[0].ToVector4();
        }

        private HalfVector4 SampleHalfFloat4(Texture2D tex, Vector2 texCoord)
        {

            Rectangle sourceRectangle =
            new Rectangle((int)(texCoord.X * tex.Width), (int)(texCoord.Y * tex.Height), 1, 1);

            HalfVector4[] retrievedColor = new HalfVector4[1];

            tex.GetData(0, sourceRectangle, retrievedColor, 0, 1);

            return retrievedColor[0]; //retrievedColor[0].ToVector4();
        }

    }
}
