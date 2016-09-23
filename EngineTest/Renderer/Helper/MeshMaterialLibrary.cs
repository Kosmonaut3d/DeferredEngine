using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineTest.Entities;
using EngineTest.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Renderer.Helper
{
    // Controls all Materials and Meshes, so they are ordered at render time.

    public class MeshMaterialLibrary
    {
        const int InitialLibrarySize = 10;
        public MaterialLibrary[] MaterialLib = new MaterialLibrary[InitialLibrarySize];
        public int Index;

        private bool _previousMode = GameSettings.g_CPU_Culling;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mat">if "null" it will be taken from the model!</param>
        /// <param name="model"></param>
        /// <param name="worldMatrix"></param>
        public void Register(MaterialEffect mat, Model model, TransformMatrix worldMatrix)
        {
            if (model == null) return;

            //if (mat == null)
            //{
            //    throw new NotImplementedException();
            //}

            for (int index = 0; index < model.Meshes.Count; index++)
            {
                var mesh = model.Meshes[index];
                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    ModelMeshPart meshPart = mesh.MeshParts[i];
                    Register(mat, meshPart, worldMatrix, mesh.BoundingSphere);
                }
            }
        }

        public void Register(MaterialEffect mat, ModelMeshPart mesh, TransformMatrix worldMatrix, BoundingSphere boundingSphere) //These should be ordered by likeness, so I don't get opaque -> transparent -> opaque
        {
            bool found = false;

            if (mat == null)
            {
                mat = (MaterialEffect) mesh.Effect;
            }

            //Check if we already have a material like that, if yes put it in there!
            for (var i = 0; i < Index; i++)
            {
                MaterialLibrary matLib = MaterialLib[i];
                if (matLib.HasMaterial(mat))
                {
                    matLib.Register(mesh, worldMatrix, boundingSphere);
                    found = true;
                    break;
                }
            }

            //We have no MatLib for that specific Material yet. Make a new one.
            if (!found)
            {
                MaterialLib[Index] = new MaterialLibrary();
                MaterialLib[Index].SetMaterial(ref mat);
                MaterialLib[Index].Register(mesh, worldMatrix, boundingSphere);
                Index++;
            }

            //If we exceeded our array length, make the array bigger.
            if (Index >= MaterialLib.Length)
            {
                MaterialLibrary[] tempLib = new MaterialLibrary[Index+1];
                MaterialLib.CopyTo(tempLib, 0);
                MaterialLib = tempLib;
            }
        }

        public void DeleteFromRegistry(BasicEntity basicEntity)
        {
            if (basicEntity.Model == null) return; //nothing to delete

            //delete the individual meshes!
            for (int index = 0; index < basicEntity.Model.Meshes.Count; index++)
            {
                var mesh = basicEntity.Model.Meshes[index];
                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    ModelMeshPart meshPart = mesh.MeshParts[i];
                    DeleteFromRegistry(basicEntity.Material, meshPart, basicEntity.WorldTransform);
                }
            }
        }

        private void DeleteFromRegistry(MaterialEffect mat, ModelMeshPart mesh, TransformMatrix worldMatrix)
        {
            for (var i = 0; i < Index; i++)
            {
                MaterialLibrary matLib = MaterialLib[i];
                if (matLib.HasMaterial(mat))
                {
                    if (matLib.DeleteFromRegistry(mesh, worldMatrix))
                    {
                        for (var j = i; j < Index-1; j++)
                        {
                            //slide down one
                            MaterialLib[j] = MaterialLib[j + 1];

                        }
                        Index--;
                        
                    break;
                    }
                }
            }
        }

        /// <summary>
        /// Update whether or not Objects are in the viewFrustum and need to be rendered or not.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="boundingFrustrum"></param>
        /// <param name="hasCameraChanged"></param>
        public void MeshCulling(List<BasicEntity> entities, BoundingFrustum boundingFrustrum, bool hasCameraChanged)
        {
            //Check if the culling mode has changed
            if (_previousMode != GameSettings.g_CPU_Culling)
            {
                if (_previousMode == true)
                {
                    //If we previously did cull and now don't we need to set all the submeshes to render
                    for (int index1 = 0; index1 < Index; index1++)
                    {
                        MaterialLibrary matLib = MaterialLib[index1];
                        for (int i = 0; i < matLib.Index; i++)
                        {
                            MeshLibrary meshLib = matLib.GetMeshLibrary()[i];
                            for (int j = 0; j < meshLib.Rendered.Length; j++)
                            {
                                meshLib.Rendered[j] = _previousMode;
                            }
                        }
                    }

                }
                _previousMode = GameSettings.g_CPU_Culling;
                
            }

            if (!GameSettings.g_CPU_Culling) return;

            //Vector3 RenderBoundingBoxCenter = (RenderBoundingBox.Max + RenderBoundingBox.Min)/2;

            //First change their world value! We only need to do that once though, when we draw shadows!

            for (int index1 = 0; index1 < entities.Count; index1++)
            {
                BasicEntity entity = entities[index1];

                //If both the camera hasn't changed and the Transformation isn't changed we don't need to update the renderstate
                if (!hasCameraChanged && !entity.WorldTransform.HasChanged)
                {
                    continue;
                }

                if(entity.WorldTransform.HasChanged)
                entity.ApplyTransformation();
            }

            //Ok we applied the transformation to all the entities, now update the submesh boundingboxes!
            for (int index1 = 0; index1 < Index; index1++)
            {
                MaterialLibrary matLib = MaterialLib[index1];
                for (int i = 0; i < matLib.Index; i++)
                {
                    MeshLibrary meshLib = matLib.GetMeshLibrary()[i];
                    meshLib.UpdatePositionAndCheckRender(hasCameraChanged, boundingFrustrum);
                }
            }


            //Set Changed to false
            for (int index1 = 0; index1 < entities.Count; index1++)
            {
                BasicEntity entity = entities[index1];
                entity.WorldTransform.HasChanged = false;
            }


        }

        ///// <summary>
        ///// Update whether or not Objects are in the viewFrustum and need to be rendered or not.
        ///// </summary>
        ///// <param name="entities"></param>
        ///// <param name="boundingFrustrum"></param>
        ///// <param name="hasCameraChanged"></param>
        //public void UpdateWorld(List<BasicEntity> entities,  BoundingFrustum boundingFrustrum, bool hasCameraChanged)
        //{

        //    //Vector3 RenderBoundingBoxCenter = (RenderBoundingBox.Max + RenderBoundingBox.Min)/2;

        //    bool isInsiderRenderVolume = false;

        //    //First change their world value! We only need to do that once though, when we draw shadows!
            
        //    for (int index1 = 0; index1 < entities.Count; index1++)
        //    {
        //        BasicEntity entity = entities[index1];

        //        ModelMesh entityModelMeshPart = entity.Model.Meshes[0];

        //        //If both the camera hasn't changed and the Transformation isn't changed we don't need to update the renderstate
        //        if (!hasCameraChanged && !entity.WorldTransform.HasChanged)
        //        {
        //            continue;
        //        }

        //        //Otherwise we set the changed variable to false, since we process the current position
        //        entity.WorldTransform.HasChanged = false;


        //        isInsiderRenderVolume = false;

        //        //First test, check entity origin vs RenderBoundingBox
        //        if (boundingFrustrum.Contains(entity.Position) == ContainmentType.Contains)
        //        {
        //            isInsiderRenderVolume = true;
        //        }

        //        //Check boundingsphere
        //        if (!isInsiderRenderVolume)
        //        {
        //            //Get the geometry center of the model

        //            Vector3 modelCenter = entity.Position - entityModelMeshPart.MeshBoundingSphere.Center * entity.Scale / 2;

        //            //Create RenderSphere!
        //            MeshBoundingSphere renderSphere =
        //                entityModelMeshPart.MeshBoundingSphere.Transform(Matrix.CreateScale(entity.Scale) * Matrix.CreateTranslation(modelCenter));

        //            //Check containment!
        //            if (boundingFrustrum.Intersects(renderSphere))
        //            {
        //                isInsiderRenderVolume = true;
        //            }

        //        }

        //        //We can also take into account screenpos
        //        if (!isInsiderRenderVolume)
        //        {
        //            //Update all parts as well!
        //            entity.WorldTransform.Rendered = false;

        //            entity.SetRenderMode(false);

        //            continue;
        //        }
        //        entity.SetRenderMode(true);

        //        Matrix worldTransform = entity.GetTransformation();
        //        entity.WorldTransform.World = worldTransform;
        //    }
        //}

        public void Draw(bool shadow, GraphicsDevice graphicsDevice, Matrix viewProjection, bool opaque)
        {
            if (opaque)
            {
                graphicsDevice.BlendState = BlendState.Opaque;
                graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }
            else
            {
                graphicsDevice.BlendState = BlendState.NonPremultiplied;
                graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }

            //Count our drawcalls, for debug purposes
            int materialDraws = 0;
            int meshDraws = 0;

            for (int index1 = 0; index1 < Index; index1++)
            {
                MaterialLibrary matLib = MaterialLib[index1];

                if (matLib.Index < 1) continue;

                //if none of this materialtype is drawn continue too!
                bool isUsed = false;

                for (int i = 0; i < matLib.Index; i++)
                {
                    MeshLibrary meshLib = matLib.GetMeshLibrary()[i];
                    for (int index = 0; index < meshLib.Index; index++)
                    {
                        //If it's set to "not rendered" skip
                        for (int j = 0; j < meshLib.Rendered.Length; j++)
                        {
                            if (meshLib.Rendered[j] == true)
                            {
                                isUsed = true;
                                break;
                            }

                        }
                       
                        if(isUsed)
                        break;
                    }
                }

                if (!isUsed) continue;

                //Count the draws of different materials!
                materialDraws++;

                MaterialEffect material = /*GameSettings.DebugDrawUntextured==2 ? Art.DefaultMaterial :*/ matLib.GetMaterial();

                //Check if alpha or opaque!
                if (opaque && material.IsTransparent) continue;
                if (!opaque && !material.IsTransparent) continue;

                Effect shader = Shaders.GBufferEffect;
                //Set the appropriate Shader for the material
                if (shadow)
                {
                    if (material.HasShadow)
                    {
                        //if we have special shadow shaders for the material

                    }
                    else continue;
                }

                //todo: We only need textures for non shadow mapping, right? Not quite actually, for alpha textures we need materials
                if (!shadow)
                {
                    //Set up the right GBuffer shader!

                    Shaders.GBufferEffect.CurrentTechnique = Shaders.GBufferEffect.Techniques["DrawBasic"];

                    Shaders.GBufferEffectParameter_Material_DiffuseColor.SetValue(material.DiffuseColor);
                    Shaders.GBufferEffectParameter_Material_Roughness.SetValue(material.Roughness);
                    Shaders.GBufferEffectParameter_Material_Metalness.SetValue(material.Metalness);
                    Shaders.GBufferEffectParameter_Material_MaterialType.SetValue(material.MaterialType);
                }
                else
                {
                    //throw new NotImplementedException();
                }

                for (int i = 0; i < matLib.Index; i++)
                {
                    MeshLibrary meshLib = matLib.GetMeshLibrary()[i];

                    //Initialize the mesh VB and IB
                    graphicsDevice.SetVertexBuffer(meshLib.GetMesh().VertexBuffer);
                    graphicsDevice.Indices = (meshLib.GetMesh().IndexBuffer);
                    int primitiveCount = meshLib.GetMesh().PrimitiveCount;
                    int vertexOffset = meshLib.GetMesh().VertexOffset;
                    //int vCount = meshLib.GetMesh().NumVertices;
                    int startIndex = meshLib.GetMesh().StartIndex;

                    //Now draw the local meshes!
                    for (int index = 0; index < meshLib.Index; index++)
                    {
               
                        //If it's set to "not rendered" skip
                        //if (!meshLib.GetWorldMatrices()[index].Rendered) continue;
                        if (!meshLib.Rendered[index]) continue;

                        meshDraws ++;

                        Matrix localWorldMatrix = meshLib.GetWorldMatrices()[index].World;
                        if (!shadow)
                        {
                            Shaders.GBufferEffectParameter_World.SetValue(localWorldMatrix);
                            Shaders.GBufferEffectParameter_WorldViewProj.SetValue(localWorldMatrix * viewProjection);
                        }
                        else
                        {
                           
                        }

                        shader.CurrentTechnique.Passes[0].Apply();

                        try
                        {
                            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
                        }
                        catch (Exception)
                        {
                            // do nothing
                        }
                            
                        
                    }

                }

            }

            //Update the drawcalls in our stats
            GameStats.MaterialDraws += materialDraws;
            GameStats.MeshDraws += meshDraws;
        }

    				

    }

    /// <summary>
    /// Library which has a list of meshlibraries that correspond to a material.
    /// </summary>
    public class MaterialLibrary
    {
        private MaterialEffect _material;
        
        //Determines how many different meshes we have per texture. 
        const int InitialLibrarySize = 2;
        private MeshLibrary[] _meshLib = new MeshLibrary[InitialLibrarySize];
        public int Index;

        public void SetMaterial(ref MaterialEffect mat)
        {
            _material = mat;
        }

        public bool HasMaterial(MaterialEffect mat)
        {
            return mat == _material;
        }

        public MaterialEffect GetMaterial()
        {
            return _material;
        }

        public MeshLibrary[] GetMeshLibrary()
        {
            return _meshLib;
        }

        public void Register(ModelMeshPart mesh, TransformMatrix worldMatrix, BoundingSphere boundingSphere)
        {
            bool found = false;
            //Check if we already have a model like that, if yes put it in there!
            for (var i = 0; i < Index; i++)
            {
                MeshLibrary meshLib = _meshLib[i];
                if (meshLib.HasMesh(mesh))
                {
                    meshLib.Register(worldMatrix);
                    found = true;
                    break;
                }
            }

            //We have no Lib yet, make a new one.
            if (!found)
            {
                _meshLib[Index] = new MeshLibrary();
                _meshLib[Index].SetMesh(mesh);
                _meshLib[Index].SetBoundingSphere(boundingSphere);
                _meshLib[Index].Register(worldMatrix);
                Index++;
            }

            //If we exceeded our array length, make the array bigger.
            if (Index >= _meshLib.Length)
            {
                MeshLibrary[] tempLib = new MeshLibrary[Index + 1];
                _meshLib.CopyTo(tempLib, 0);
                _meshLib = tempLib;
            }
        }

        public bool DeleteFromRegistry(ModelMeshPart mesh, TransformMatrix worldMatrix)
        {
            for (var i = 0; i < Index; i++)
            {
                MeshLibrary meshLib = _meshLib[i];
                if (meshLib.HasMesh(mesh))
                {
                    if (meshLib.DeleteFromRegistry(worldMatrix)) //if true, we can delete it from registry
                    {
                        for (var j = i; j < Index-1; j++)
                        {
                            //slide down one
                            _meshLib[j] = _meshLib[j + 1];

                        }
                        Index--;
                    }
                    break;
                }
            }
            if (Index <= 0) return true; //this material is no longer needed.
            return false;
        }
    }

    //The individual model mesh, and a library or different world coordinates basically is what we need
    public class MeshLibrary
    {
        private ModelMeshPart _mesh;
        public BoundingSphere MeshBoundingSphere;

        const int InitialLibrarySize = 4;
        private TransformMatrix[] _worldMatrices = new TransformMatrix[InitialLibrarySize];

        //the local displacement of the boundingsphere!
        private Vector3[] _worldBoundingCenters = new Vector3[InitialLibrarySize];
        //the local mode - either rendered or not!
        public bool[] Rendered = new bool[InitialLibrarySize];

        public int Index;

        public void SetMesh(ModelMeshPart mesh)
        {
            _mesh = mesh;
        }

        public bool HasMesh(ModelMeshPart mesh)
        {
            return mesh == _mesh;
        }

        public ModelMeshPart GetMesh()
        {
            return _mesh;
        }

        public TransformMatrix[] GetWorldMatrices()
        {
            return _worldMatrices;
        }

        //IF a submesh belongs to an entity that has moved we need to update the BoundingBoxWorld Position!
        public void UpdatePositionAndCheckRender(bool cameraHasChanged, BoundingFrustum viewFrustum)
        {
            BoundingSphere sphere = new BoundingSphere(Vector3.Zero, MeshBoundingSphere.Radius);
            for (var i = 0; i < Index; i++)
            {
                TransformMatrix trafoMatrix = _worldMatrices[i];

                if (trafoMatrix.HasChanged)
                {
                    _worldBoundingCenters[i] = trafoMatrix.TransformMatrixSubModel(MeshBoundingSphere.Center);
                }

                //If either the trafomatrix or the camera has changed we need to check visibility
                if (trafoMatrix.HasChanged || cameraHasChanged)
                {
                    sphere.Center = _worldBoundingCenters[i];
                    if (viewFrustum.Contains(sphere)==ContainmentType.Disjoint )
                    {
                        Rendered[i] = false;
                    }
                    else
                    {
                        Rendered[i] = true;
                    }
                }
            }
        }

        //Basically no chance we have the same model already. We should be fine just adding it to the list if we did everything else right.
        public void Register(TransformMatrix world)
        {
            _worldMatrices[Index] = world;
            Rendered[Index] = true;
            _worldBoundingCenters[Index] = world.TransformMatrixSubModel(MeshBoundingSphere.Center);

            Index++;

            //mesh.Effect = Shaders.AmbientEffect; //Just so it has few properties!

            if (Index >= _worldMatrices.Length)
            {
                TransformMatrix[] tempLib = new TransformMatrix[Index + 1];
                _worldMatrices.CopyTo(tempLib, 0);
                _worldMatrices = tempLib;

                Vector3[] tempLib2 = new Vector3[Index + 1];
                _worldBoundingCenters.CopyTo(tempLib2, 0);
                _worldBoundingCenters = tempLib2;

                bool[] tempRendered = new bool[Index + 1];
                Rendered.CopyTo(tempRendered, 0);
                Rendered = tempRendered;
            }
        }

        public bool DeleteFromRegistry(TransformMatrix worldMatrix)
        {
            for (var i = 0; i < Index; i++)
            {
                TransformMatrix trafoMatrix = _worldMatrices[i];

                if (trafoMatrix == worldMatrix)
                {
                    //delete this value!
                    for (var j = i; j < Index - 1; j++)
                    {
                        //slide down one
                        _worldMatrices[j] = _worldMatrices[j + 1];
                        Rendered[j] = Rendered[j + 1];
                        _worldBoundingCenters[j] = _worldBoundingCenters[j + 1];
                    }
                    Index--;
                    break;
                }
            }
            if (Index <= 0) return true; //this meshtype no longer needed!
            return false;
        }

        public void SetBoundingSphere(BoundingSphere boundingSphere)
        {
            MeshBoundingSphere = boundingSphere;
        }
    }
}
