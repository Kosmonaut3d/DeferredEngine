﻿using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using BEPUutilities;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace DeferredEngine.Recources.Helper
{
    /// <summary>
    /// Contains helper methods for extracting vertices and indices from XNA models.
    /// </summary>
    public static class ModelDataExtractor
    {
        /// <summary>
        /// Gets an array of vertices and indices from the provided model.
        /// </summary>
        /// <param name="collisionModel">Model to use for the collision shape.</param>
        /// <param name="vertices">Compiled set of vertices from the model.</param>
        /// <param name="indices">Compiled set of indices from the model.</param>
        public static void GetVerticesAndIndicesFromModel(Model collisionModel, out Vector3[] vertices, out int[] indices)
        {
            Microsoft.Xna.Framework.Vector3[] tempVertices;
            Microsoft.Xna.Framework.Vector3[] tempNormals;
            GetVerticesAndIndicesFromModel(collisionModel, out tempVertices, out tempNormals, out indices);
            vertices = MathConverter.Convert(tempVertices);
        }

        /// <summary>
        /// Gets an array of vertices and indices from the provided model.
        /// </summary>
        /// <param name="collisionModel">Model to use for the collision shape.</param>
        /// <param name="vertices">Compiled set of vertices from the model.</param>
        /// <param name="indices">Compiled set of indices from the model.</param>
        public static void GetVerticesAndIndicesFromModel(Model collisionModel, out Microsoft.Xna.Framework.Vector3[] vertices, out Microsoft.Xna.Framework.Vector3[] normals, out int[] indices)
        {
            var verticesList = new List<Microsoft.Xna.Framework.Vector3>();
            var normalList = new List<Microsoft.Xna.Framework.Vector3>();
            var indicesList = new List<int>();
            var transforms = new Matrix[collisionModel.Bones.Count];
            collisionModel.CopyAbsoluteBoneTransformsTo(transforms);

            Matrix transform;
            foreach (ModelMesh mesh in collisionModel.Meshes)
            {
                //if (mesh.ParentBone != null)
                //    transform = transforms[mesh.ParentBone.Index];
                //else
                    transform = Matrix.Identity;
                AddMesh(mesh, transform, verticesList, normalList, indicesList);
            }

            vertices = verticesList.ToArray();
            indices = indicesList.ToArray();
            normals = normalList.ToArray();

        }

        /// <summary>
        /// Adds a mesh's vertices and indices to the given lists.
        /// </summary>
        /// <param name="collisionModelMesh">Model to use for the collision shape.</param>
        /// <param name="transform">Transform to apply to the mesh.</param>
        /// <param name="vertices">List to receive vertices from the mesh.</param>
        /// <param name="indices">List to receive indices from the mesh.</param>
        public static void AddMesh(ModelMesh collisionModelMesh, Matrix transform, List<Microsoft.Xna.Framework.Vector3> vertices, List<Microsoft.Xna.Framework.Vector3> normals, IList<int> indices)
        {
            foreach (ModelMeshPart meshPart in collisionModelMesh.MeshParts)
            {
                int startIndex = vertices.Count;
                ////Grab position data from the mesh part.
                //int stride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                //meshPart.VertexBuffer.GetData(
                //        meshPart.VertexOffset * stride,
                //        meshPartVertices,
                //        0,
                //        meshPart.NumVertices,
                //        stride);

                var meshPartVertices = new VertexPositionNormalTexture[meshPart.NumVertices];
                //Grab position data from the mesh part.
                
               //meshPart.VertexBuffer.GetData(meshPartVertices, 0, meshPart.NumVertices);

                int stride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                meshPart.VertexBuffer.GetData<VertexPositionNormalTexture>(
                        meshPart.VertexOffset * stride,
                        meshPartVertices,
                        0,
                        meshPart.NumVertices,
                        stride);

                //Copy

                var meshPartVerticePosition = new Microsoft.Xna.Framework.Vector3[meshPart.NumVertices];
                var meshPartVerticeNormal = new Microsoft.Xna.Framework.Vector3[meshPart.NumVertices];
                for (int i = 0; i < meshPartVertices.Length; i++)
                {
                    meshPartVerticePosition[i] = meshPartVertices[i].Position;
                    meshPartVerticeNormal[i] = meshPartVertices[i].Normal;
                }


                //Transform it so its vertices are located in the model's space as opposed to mesh part space.
                Microsoft.Xna.Framework.Vector3.Transform(meshPartVerticePosition, ref transform, meshPartVerticePosition);
                vertices.AddRange(meshPartVerticePosition);
                normals.AddRange(meshPartVerticePosition);

                if (meshPart.IndexBuffer.IndexElementSize == IndexElementSize.ThirtyTwoBits)
                {
                    var meshIndices = new int[meshPart.PrimitiveCount * 3];
                    meshPart.IndexBuffer.GetData(meshPart.StartIndex * 4, meshIndices, 0, meshPart.PrimitiveCount * 3);
                    for (int k = 0; k < meshIndices.Length; k++)
                    {
                        indices.Add(startIndex + meshIndices[k]);
                    }
                }
                else
                {
                    var meshIndices = new ushort[meshPart.PrimitiveCount * 3];
                    meshPart.IndexBuffer.GetData(meshPart.StartIndex * 2, meshIndices, 0, meshPart.PrimitiveCount * 3);
                    for (int k = 0; k < meshIndices.Length; k++)
                    {
                        indices.Add(startIndex + meshIndices[k]);
                    }


                }
            }




        }

    }
}
