using System;
using System.Collections.Generic;
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

                for (var index = 0; index < vertices.Length; index++)
                {
                    vertices[index] = Vector3.Transform(vertices[index], entity.WorldTransform.World);
                }

                int baseIndex = 0;
                for (var i = 0; i < triangles.Length; i++, baseIndex += 3)
                {
                    triangles[i].a = vertices[indices[baseIndex]];
                    triangles[i].b = vertices[indices[baseIndex + 1]];
                    triangles[i].c = vertices[indices[baseIndex + 2]];
                    //normal
                    triangles[i].n = Vector3.Cross(triangles[i].c - triangles[i].a, triangles[i].b - triangles[i].a);
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


            if (volumeTex.NeedsUpdate && Input.WasKeyPressed(Keys.J) && generateTris!=null && generateTris.IsCompleted)
            {
                setup = true;
                generateTask = Task.Factory.StartNew(() =>
                {
                    volumeTex.NeedsUpdate = false;

                    int stepssize = 100;
                    int xi = 0;
                    int yi = 0;
                    int zi = 0;

                    int xsteps = (int) (volumeTex.SizeX * 2 / stepssize) + 1;
                    int ysteps = (int) (volumeTex.SizeY * 2 / stepssize) + 1;
                    int zsteps = (int) (volumeTex.SizeZ * 2 / stepssize) + 1;

                    texture = new Texture2D(graphics, xsteps * zsteps, ysteps, false, SurfaceFormat.Single);

                    volumeTex.Resolution = new Vector3(xsteps, ysteps, zsteps);

                    float[] data = new float[xsteps * ysteps * zsteps];

                    float x;
                    float y;
                    float z;

                    for (x = (int) -volumeTex.SizeX, xi = 0; x <= volumeTex.SizeX; x += stepssize, xi++)
                    {
                        for (y = (int) -volumeTex.SizeY, yi = 0; y <= volumeTex.SizeY; y += stepssize, yi++)
                        {
                            for (z = (int) -volumeTex.SizeZ, zi = 0; z <= volumeTex.SizeZ; z += stepssize, zi++)
                            {
                                Vector3 position = volumeTex.Position + new Vector3(x, y, z);
                                float color = ComputeSDF(position);

                                if (xi == 1) color = 1;
                                //points.Add(new SamplePoint(position, color));

                                data[toTexCoords(xi, yi, zi, xsteps, zsteps)] = color;

                                GameStats.sdf_load = (xi + (yi + zi / (float)zsteps) / (float)ysteps) / (float)xsteps;
                            }
                        }

                    }

                    texture.SetData(data);

                    string path = "sponza_sdf.sdff";

                    //Store
                    DataStream.SaveImageData(data, xsteps, ysteps, zsteps, path);

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
            }


            foreach (var sample in points)
            {
                manager.AddOctahedron(sample.p, Vector4.One * sample.sdf);
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

            float min = 10000;

            foreach (var tri in triangles)
            {
                Vector3 a = tri.a;
                Vector3 b = tri.b;
                Vector3 c = tri.c;
                Vector3 ba = b - a;
                Vector3 pa = p - a;
                Vector3 cb = c - b;
                Vector3 pb = p - b;
                Vector3 ac = a - c;
                Vector3 pc = p - c;
                Vector3 nor = Vector3.Cross(ba, ac);
                
                float value = (Math.Sign(Vector3.Dot(Vector3.Cross(ba, nor), pa)) + 
                               Math.Sign(Vector3.Dot(Vector3.Cross(cb, nor), pb)) +
                               Math.Sign(Vector3.Dot(Vector3.Cross(ac, nor), pc)) < 2.0f
                               ? 
                               Math.Min(Math.Min(
                               dot2(ba * saturate(Vector3.Dot(ba, pa) / dot2(ba)) - pa),
                               dot2(cb * saturate(Vector3.Dot(cb, pb) / dot2(cb)) - pb)),
                               dot2(ac * saturate(Vector3.Dot(ac, pc) / dot2(ac)) - pc))
                               : 
                               Vector3.Dot(nor, pa) * Vector3.Dot(nor, pa) / dot2(nor));
                float sign = Math.Sign(Vector3.Dot(pa, tri.n));

                if (value < Math.Abs(min))
                {
                    min = value * sign;
                }
            }

            return min;
        }
    }
}
