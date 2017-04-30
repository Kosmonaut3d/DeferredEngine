using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionShapes.ConvexShapes;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.Helper.HelperGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DeferredEngine.Logic.SDF_Generator
{
    public class SDFGenerator
    {
        public Vector3[] vertices;
        public Triangle[] triangles;
        public List<SamplePoint> points = new List<SamplePoint>();
        private Task generateTask;
        private Texture2D texture;
        private bool setup = false;
        private Task generateTris;

        public struct Triangle
        {
            public Vector3 a;
            public Vector3 b;
            public Vector3 c;
            public Vector3 n;
            public Vector3 ba;
            public Vector3 cb;
            public Vector3 ac;
            public Vector3 nor;

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
        

        public void Generate(BasicEntity entity)
        {
            //Extract all triangles
            //foreach (ModelMesh modelmesh in entity.Model.Meshes)
            //{
            //    foreach (ModelMeshPart part in modelmesh.MeshParts)
            //    {
            //        VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[part.VertexBuffer.VertexCount];
            //        part.VertexBuffer.GetData<VertexPositionNormalTexture>(vertices);

            //        ushort[] drawOrder = new ushort[part.IndexBuffer.IndexCount];
            //        part.IndexBuffer.GetData<ushort>(drawOrder);

            //        foreach (var vertex in vertices)
            //        {
            //            points.Add(vertex.Position);
            //        }
            //    }
            //}

            generateTris = Task.Factory.StartNew(() =>
            {

                int[] indices;
                ModelDataExtractor.GetVerticesAndIndicesFromModel(entity.Model, out vertices, out indices);

                triangles = new Triangle[indices.Length / 3];

                //Create BOunding box
                BoundingBox box = BoundingBox.CreateFromPoints(vertices);

                //Transform to world position first? No

                //for (var index = 0; index < vertices.Length; index++)
                //{
                //    vertices[index] = Vector3.Transform(vertices[index], entity.WorldTransform.World);
                //}

                int baseIndex = 0;
                for (var i = 0; i < triangles.Length; i++, baseIndex += 3)
                {
                    triangles[i].a = vertices[indices[baseIndex]];
                    triangles[i].b = vertices[indices[baseIndex + 1]];
                    triangles[i].c = vertices[indices[baseIndex + 2]];
                    //normal
                    triangles[i].n = Vector3.Cross(triangles[i].c - triangles[i].a, triangles[i].b - triangles[i].a);

                    triangles[i].ba = triangles[i].b - triangles[i].a;
                    triangles[i].cb = triangles[i].c - triangles[i].b;
                    triangles[i].ac = triangles[i].a - triangles[i].c;
                }
            });
            //for (var index = 0; index < points.Length; index++)
            //{
            //    points[index] = Vector3.Transform(points[index], entity.WorldTransform.World);
            //}
        }
        
        private int toTexCoords(int x, int y, int z, int xsteps, int zsteps)
        {
            x += z * xsteps;
            return x + y * xsteps * zsteps;
        }

        public void Update(VolumeTextureEntity volumeTex, GraphicsDevice graphics)
        {
            
            HelperGeometryManager manager = HelperGeometryManager.GetInstance();
            //foreach (var point in points)
            //{
            //    manager.AddOctahedron(point, Vector4.One);
            //}

            //Show normals
            //foreach (var tri in triangles)
            //{
            //    manager.AddLineStartDir(tri.a, tri.n, 1, Color.Red, Color.Blue);
            //}


            if (!DebugScreen.ConsoleOpen && Input.WasKeyPressed(Keys.J) && generateTris!=null && generateTris.IsCompleted)
            {
                setup = true;
                generateTask = Task.Factory.StartNew(() =>
                {
                    volumeTex.NeedsUpdate = false;

                    const float stepssize = 2.5f;

                    int xsteps = (int) (volumeTex.SizeX * 2 / stepssize) + 1;
                    int ysteps = (int) (volumeTex.SizeY * 2 / stepssize) + 1;
                    int zsteps = (int) (volumeTex.SizeZ * 2 / stepssize) + 1;

                    texture?.Dispose();
                    texture = new Texture2D(graphics, xsteps * zsteps, ysteps, false, SurfaceFormat.Single);

                    float[] data = new float[xsteps * ysteps * zsteps];

                    Stopwatch stopwatch = Stopwatch.StartNew();

                    int numberOfThreads = GameSettings.sdf_threads;

                    if (numberOfThreads > 1)
                    {
                        Task[] threads = new Task[numberOfThreads-1];

                        //Make local datas

                        float[][] dataArray = new float[numberOfThreads][];

                        for (int index = 0; index < threads.Length; index++)
                        {
                            int i = index;
                            dataArray[index+1] = new float[xsteps * ysteps * zsteps];
                            threads[i] = Task.Factory.StartNew(() =>
                            {
                                GenerateData(xsteps, ysteps, zsteps, stepssize, volumeTex, ref dataArray[i+1], i+1,
                                    numberOfThreads);
                            });
                        }

                        dataArray[0] = data;
                        GenerateData(xsteps, ysteps, zsteps, stepssize, volumeTex, ref dataArray[0], 0, numberOfThreads);
                        
                        Task.WaitAll(threads);

                        for (var index2 = 0; index2 < threads.Length; index2++)
                        {
                            threads[index2].Dispose();
                        }

                        for (int i = 0; i < data.Length; i++)
                        {
                            data[i] = dataArray[i % numberOfThreads][i];
                        }
                    }
                    else
                    {
                        GenerateData(xsteps, ysteps, zsteps, stepssize, volumeTex, ref data, 0, numberOfThreads);
                    }


                    stopwatch.Stop();

                    Debug.Write("\nSDF generated in "+stopwatch.ElapsedMilliseconds+"ms with "+GameSettings.sdf_threads+" thread(s)");

                    string path = "sponza_sdf.sdff";

                    //Store
                    DataStream.SaveImageData(data, xsteps, ysteps, zsteps, path);

                    texture.SetData(data);

                    volumeTex.Resolution = new Vector3(xsteps, ysteps, zsteps);

                    //Stream stream = File.Create("volumetex");
                    //texture.SaveAsPng(stream, texture.Width, texture.Height);
                    //stream.Dispose();

                    GameStats.sdf_load = 0;
                });
            }

            if (setup && generateTask != null && generateTask.IsCompleted)
            {
                setup = false;
                volumeTex.Texture = texture;
                generateTask.Dispose();
            }


            //foreach (var sample in points)
            //{
            //    manager.AddOctahedron(sample.p, Vector4.One * sample.sdf);
            //}
        }

        private void GenerateData(int xsteps, int ysteps, int zsteps, float stepssize, VolumeTextureEntity volumeTex, ref float[] data, int threadindex, int numberOfThreads)
        {
            float x, y, z;
            int xi, yi, zi;

            float volumeTexSizeX = volumeTex.SizeX;
            float volumeTexSizeY = volumeTex.SizeY;
            float volumeTexSizeZ = volumeTex.SizeZ;

            int i = 0;

            for (x = (int)-volumeTexSizeX, xi = 0; x <= volumeTexSizeX; x += stepssize, xi++)
            {
                for (y = (int)-volumeTexSizeY, yi = 0; y <= volumeTexSizeY; y += stepssize, yi++)
                {
                    for (z = (int)-volumeTexSizeZ, zi = 0; z <= volumeTexSizeZ; z += stepssize, zi++)
                    {
                        //Only do it for the current thread!
                        if (i++ % numberOfThreads != threadindex) continue;

                        Vector3 position = volumeTex.Position + new Vector3(x, y, z);


                        float color = ComputeSDF(position);

                        
                        //float color = 0;//
                        //if (zi % 2 == 1  && xi%2 != yi%2) color = .25f;
                        

                        //points.Add(new SamplePoint(position, color));

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

        private float ComputeSDF(Vector3 p)
        {
            //Find nearest distance.
            //http://iquilezles.org/www/articles/distfunctions/distfunctions.htm

            float min = 100000;

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
                               Math.Sign(Vector3.Dot(Vector3.Cross(ac, nor), pc)) < 2.0f
                    ? Math.Min(Math.Min(
                            dot2(ba * saturate(Vector3.Dot(ba, pa) / dot2(ba)) - pa),
                            dot2(cb * saturate(Vector3.Dot(cb, pb) / dot2(cb)) - pb)),
                        dot2(ac * saturate(Vector3.Dot(ac, pc) / dot2(ac)) - pc))
                    : Vector3.Dot(nor, pa) * Vector3.Dot(nor, pa) / dot2(nor));

                float sign = Math.Sign(Vector3.Dot(pa, nor));

                if (value < Math.Abs(min))
                {
                    min = value * sign;
                }
            }

            return (float) Math.Sqrt(Math.Abs(min)) * Math.Sign(min);
        }
    }
}
