using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules.Signed_Distance_Fields.SDF_Generator
{
    public class SdfGenerator
    {
        private Task generateTask;

        private List<SignedDistanceField> sdfDefinitions = new List<SignedDistanceField>();
        
        public struct Triangle
        {
            public Vector3 a;
            public Vector3 b;
            public Vector3 c;
            public Vector3 n;
            public Vector3 ba;
            public Vector3 cb;
            public Vector3 ac;

        }

        public struct SamplePoint
        {
            public Vector3 p;
            public float sdf;

            public SamplePoint(Vector3 position, float sdf)
            {
                p = position;
                this.sdf = sdf;
            }
        }


        public void GenerateTriangles(Model model, out Triangle[] triangles)
        {
            Vector3[] vertexPositions;
            int[] indices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(model, out vertexPositions, out indices);
            triangles = new Triangle[indices.Length / 3];
            int baseIndex = 0;
            for (var i = 0; i < triangles.Length; i++, baseIndex += 3)
            {
                triangles[i].a = vertexPositions[indices[baseIndex]];
                triangles[i].b = vertexPositions[indices[baseIndex + 1]];
                triangles[i].c = vertexPositions[indices[baseIndex + 2]];
                //normal
                triangles[i].ba = triangles[i].b - triangles[i].a;
                triangles[i].cb = triangles[i].c - triangles[i].b;
                triangles[i].ac = triangles[i].a - triangles[i].c;

                triangles[i].n = Vector3.Cross(triangles[i].ba, triangles[i].ac);
                triangles[i].n.Normalize();
                triangles[i].n *= 0.03f;
            }
        }

        public void GenerateDistanceFields(BasicEntity entity, GraphicsDevice graphics, DistanceFieldRenderModule distanceFieldRenderModule, FullScreenTriangle fullScreenTriangle)
        {
            SignedDistanceField uncomputedSignedDistanceField = entity.SignedDistanceField;
            Model unprocessedModel = entity.Model;

            //Set to false so it won't get covered in future
            uncomputedSignedDistanceField.NeedsToBeGenerated = false;
            uncomputedSignedDistanceField.IsLoaded = false;
            uncomputedSignedDistanceField.SdfTexture?.Dispose();

            //First generate tris
            Triangle[] triangles;
            GenerateTriangles(unprocessedModel, out triangles);

            int xsteps = (int)uncomputedSignedDistanceField.TextureResolution.X;
            int ysteps = (int)uncomputedSignedDistanceField.TextureResolution.Y;
            int zsteps = (int)uncomputedSignedDistanceField.TextureResolution.Z;

            Texture2D output;

            if (!GameSettings.sdf_cpu)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                int maxwidth = 4096; //16384
                int requiredData = triangles.Length * 3;

                int x = maxwidth;//Math.Min(requiredData, maxwidth);
                int y = requiredData / x + 1;

                Vector4[] data = new Vector4[x * y];

                int index = 0;
                for (int i = 0; i < triangles.Length; i++, index += 3)
                {
                    data[index] = new Vector4(triangles[i].a, 0);
                    data[index + 1] = new Vector4(triangles[i].b, 0);
                    data[index + 2] = new Vector4(triangles[i].c, 0);
                }

                //16k

                Texture2D triangleData = new Texture2D(graphics, x, y, false, SurfaceFormat.Vector4);

                triangleData.SetData(data);

                output = distanceFieldRenderModule.CreateSDFTexture(graphics, triangleData, xsteps, ysteps,
                    zsteps, uncomputedSignedDistanceField, fullScreenTriangle, triangles.Length);

                stopwatch.Stop();

                Debug.Write("\nSDF generated in " + stopwatch.ElapsedMilliseconds + "ms on GPU");

                float[] texData = new float[xsteps * ysteps * zsteps];

                output.GetData(texData);

                string path = uncomputedSignedDistanceField.TexturePath;
                DataStream.SaveImageData(texData, xsteps, ysteps, zsteps, path);
                uncomputedSignedDistanceField.TextureResolution = new Vector4(xsteps, ysteps, zsteps, 0);
                uncomputedSignedDistanceField.SdfTexture = output;
                uncomputedSignedDistanceField.IsLoaded = true;

            }
            else
            {
                generateTask = Task.Factory.StartNew(() =>
                {
                    output = new Texture2D(graphics, xsteps * zsteps, ysteps, false, SurfaceFormat.Single);

                    float[] data = new float[xsteps * ysteps * zsteps];

                    Stopwatch stopwatch = Stopwatch.StartNew();

                    int numberOfThreads = GameSettings.sdf_threads;

                    if (numberOfThreads > 1)
                    {
                        Task[] threads = new Task[numberOfThreads - 1];

                        //Make local datas

                        float[][] dataArray = new float[numberOfThreads][];

                        for (int index = 0; index < threads.Length; index++)
                        {
                            int i = index;
                            dataArray[index + 1] = new float[xsteps * ysteps * zsteps];
                            threads[i] = Task.Factory.StartNew(() =>
                            {
                                GenerateData(xsteps, ysteps, zsteps, uncomputedSignedDistanceField,
                                    ref dataArray[i + 1], i + 1,
                                    numberOfThreads, triangles);
                            });
                        }

                        dataArray[0] = data;
                        GenerateData(xsteps, ysteps, zsteps, uncomputedSignedDistanceField, ref dataArray[0], 0,
                            numberOfThreads, triangles);

                        Task.WaitAll(threads);

                        //Something broke?
                        for (int i = 0; i < data.Length; i++)
                        {
                            //data[i] = dataArray[i % numberOfThreads][i];
                            for (int j = 0; j < numberOfThreads; j++)
                            {
                                if (dataArray[j][i] != 0)
                                {
                                    data[i] = dataArray[j][i];
                                    break;
                                }
                            }
                        }

                        for (var index2 = 0; index2 < threads.Length; index2++)
                        {
                            threads[index2].Dispose();
                        }
                    }
                    else
                    {
                        GenerateData(xsteps, ysteps, zsteps, uncomputedSignedDistanceField, ref data, 0,
                            numberOfThreads, triangles);
                    }

                    stopwatch.Stop();

                    Debug.Write("\nSDF generated in " + stopwatch.ElapsedMilliseconds + "ms with " +
                                GameSettings.sdf_threads + " thread(s)");

                    string path = uncomputedSignedDistanceField.TexturePath;
                    DataStream.SaveImageData(data, xsteps, ysteps, zsteps, path);
                    output.SetData(data);
                    uncomputedSignedDistanceField.TextureResolution = new Vector4(xsteps, ysteps, zsteps, 0);
                    uncomputedSignedDistanceField.SdfTexture = output;
                    uncomputedSignedDistanceField.IsLoaded = true;
                });
            }
        }

        private int toTexCoords(int x, int y, int z, int xsteps, int zsteps)
        {
            x += z * xsteps;
            return x + y * xsteps * zsteps;
        }

        
        public void Update(List<BasicEntity> entities, GraphicsDevice graphics, DistanceFieldRenderModule distanceFieldRenderModule, FullScreenTriangle fullScreenTriangle, ref List<SignedDistanceField> sdfDefinitionsOut)
        {
            //First let's check which entities need building, if at all!
            sdfDefinitions.Clear();

            //This should preferably be a list of meshes that are in the scene, instead of a list of entities
            for (var index0 = 0; index0 < entities.Count; index0++)
            {
                BasicEntity entity = entities[index0];


                if (!entity.SignedDistanceField.IsUsed) continue;

                if (entity.SignedDistanceField.NeedsToBeGenerated)
                {
                    GenerateDistanceFields(entity, graphics, distanceFieldRenderModule, fullScreenTriangle);
                }

                bool found = false;
                //Compile a list of all mbbs used right now
                for (var i = 0; i < sdfDefinitions.Count; i++)
                {
                    if (entity.SignedDistanceField == sdfDefinitions[i])
                    {
                        found = true;
                        break;
                    }
                }
                if(!found)
                sdfDefinitions.Add(entity.SignedDistanceField);

            }

            //Now for the model definitions
            for(var i = 0; i < sdfDefinitions.Count; i++)
            {
                if (GameSettings.sdf_regenerate)
                {
                    sdfDefinitions[i].NeedsToBeGenerated = true;
                }
            }

            GameSettings.sdf_regenerate = false;

            sdfDefinitionsOut = sdfDefinitions;
        }

        private void GenerateData(int xsteps, int ysteps, int zsteps, SignedDistanceField volumeTex, ref float[] data, int threadindex, int numberOfThreads, Triangle[] triangles)
        {
            int xi, yi, zi;

            float volumeTexSizeX = volumeTex.VolumeSize.X;
            float volumeTexSizeY = volumeTex.VolumeSize.Y;
            float volumeTexSizeZ = volumeTex.VolumeSize.Z;

            Vector3 offset = new Vector3(volumeTex.Offset.X, volumeTex.Offset.Y, volumeTex.Offset.Z);

            int i = 0;

            for (xi = 0; xi < xsteps; xi++)
            {
                for (yi = 0; yi < ysteps; yi++)
                {
                    for (zi = 0; zi < zsteps; zi++)
                    {
                        //Only do it for the current thread!
                        if (i++ % numberOfThreads != threadindex) continue;
                        
                        Vector3 position = new Vector3(xi * volumeTexSizeX * 2.0f / (xsteps-1) - volumeTexSizeX,
                            yi * volumeTexSizeY * 2.0f / (ysteps-1) - volumeTexSizeY,
                            zi * volumeTexSizeZ * 2.0f / (zsteps -1) - volumeTexSizeZ ) + offset;
                        
                        float color = ComputeSDF(position, triangles);
                        
                        data[toTexCoords(xi, yi, zi, xsteps, zsteps)] = color;

                        if(threadindex==0)
                        GameStats.sdf_load = (xi + (yi + zi / (float)zsteps) / (float)ysteps) / (float)xsteps;
                    }
                }

            }
        }

        private float dot2(Vector3 v)
        {
            return Vector3.Dot(v, v);
        }

        private float saturate(float x)
        {
            return x < 0 ? 0 : x > 1 ? 1 : x;
        }

        private float ComputeSDF(Vector3 p, Triangle[] triangles)
        {
            //Find nearest distance.
            //http://iquilezles.org/www/articles/distfunctions/distfunctions.htm

            float min = 100000;
            
            //Shoot a ray in some direction to check if we are inside the mesh or outside

            Vector3 dir = Vector3.Up;

            int intersections = 0;

            for (var index = 0; index < triangles.Length; index++)
            {
                var tri = triangles[index];
                Vector3 a = tri.a;
                Vector3 b = tri.b;
                Vector3 c = tri.c;
                Vector3 ba = tri.ba;
                Vector3 pa = p - a;
                Vector3 cb = tri.cb;
                Vector3 pb = p - b;
                Vector3 ac = tri.ac;
                Vector3 pc = p - c;
                Vector3 nor = tri.n;

                float value = (Math.Sign(Vector3.Dot(Vector3.Cross(ba, nor), pa)) +
                                Math.Sign(Vector3.Dot(Vector3.Cross(cb, nor), pb)) +
                                Math.Sign(Vector3.Dot(Vector3.Cross(ac, nor), pc)) < 2.0f)
                                ? 
                                Math.Min(Math.Min(
                                dot2(ba * saturate(Vector3.Dot(ba, pa) / dot2(ba)) - pa),
                                dot2(cb * saturate(Vector3.Dot(cb, pb) / dot2(cb)) - pb)),
                                dot2(ac * saturate(Vector3.Dot(ac, pc) / dot2(ac)) - pc))
                                : 
                                Vector3.Dot(nor, pa) * Vector3.Dot(nor, pa) / dot2(nor);

                //intersection
                intersections += RayCast(a, b, c, p, dir);


                //int sign = Math.Sign(Vector3.Dot(pa, nor));

                //value = /*Math.Abs(value)*/value * sign;

                if (Math.Abs(value) < Math.Abs(min))
                {
                    min = value;
                }
            }

            int signum = intersections % 2 == 0 ? 1 : -1;

            return (float) Math.Sqrt(Math.Abs(min)) * signum; /** Math.Sign(min)*/;
        }

        private int RayCast(Vector3 a, Vector3 b, Vector3 c, Vector3 origin, Vector3 dir)
        {
            Vector3 edge1 = b - a;
            Vector3 edge2 = c - a;
            Vector3 pvec = Vector3.Cross(dir, edge2);
            float det = Vector3.Dot(edge1, pvec);

            const float EPSILON = 0.0000001f;

            if (det > -EPSILON && det < EPSILON) return 0;

            float inv_det = 1.0f / det;
            Vector3 tvec = origin - a;
            float u = Vector3.Dot(tvec, pvec) * inv_det;

            if (u < 0 || u > 1) return 0;
            Vector3 qvec = Vector3.Cross(tvec, edge1);
            float v = Vector3.Dot(dir, qvec) * inv_det;
            if (v < 0 || u + v > 1) return 0;

            float t = Vector3.Dot(edge2, qvec) * inv_det;

            if (t > EPSILON) return 1;

            return 0;
        }
    }
}
