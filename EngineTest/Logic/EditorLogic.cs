using System.Collections.Generic;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using HelperSuite.GUIHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DirectionalLight = DeferredEngine.Entities.DirectionalLight;

namespace DeferredEngine.Logic
{
    public class EditorLogic
    {
        //private int _selectedId = 0;
        
        private bool _gizmoTransformationMode;
        private Vector3 _gizmoPosition;
        private int _gizmoId;
        private GizmoModes _gizmoMode = GizmoModes.Translation;

        public TransformableObject SelectedObject;

        private GraphicsDevice _graphicsDevice;

        private float previousMouseX = 0;
        private float previousMouseY = 0;

        public enum GizmoModes
        {
            Translation,
            Rotation,
            Scale
        }

        public struct EditorReceivedData
        {
           public int HoveredId;
            public Matrix ViewMatrix;
            public Matrix ProjectionMatrix;
        }

        public struct EditorSendData
        {
            public TransformableObject SelectedObject;
            public int SelectedObjectId;
            public Vector3 SelectedObjectPosition;
            public bool GizmoTransformationMode;
            public GizmoModes GizmoMode;
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        /// <summary>
        /// Main Logic for the editor part
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="entities"></param>
        /// <param name="data"></param>
        public void Update(GameTime gameTime, 
            List<BasicEntity> entities, 
            List<Decal> decals, 
            List<PointLight> pointLights, 
            List<DirectionalLight> dirLights, 
            EnvironmentSample envSample, 
            List<DebugEntity> debugEntities,
            EditorReceivedData data, 
            MeshMaterialLibrary meshMaterialLibrary)
        {
            if (!GameSettings.e_enableeditor) return;

            if (!DebugScreen.ConsoleOpen)
            {
                if (Input.WasKeyPressed(Keys.R)) GameStats.e_gizmoMode = GizmoModes.Rotation;
                if (Input.WasKeyPressed(Keys.T)) GameStats.e_gizmoMode = GizmoModes.Translation;
                if (Input.WasKeyPressed(Keys.Z)) GameStats.e_gizmoMode = GizmoModes.Scale;
            }

            _gizmoMode = GameStats.e_gizmoMode;

            int hoveredId = data.HoveredId;

            if (_gizmoTransformationMode)
            {
                if (Input.mouseState.LeftButton == ButtonState.Pressed)
                {
                    GizmoControl(_gizmoId, data);
                }
                else _gizmoTransformationMode = false;
            }
            else if (Input.WasLMBClicked() && !GUIControl.UIWasUsed)
            {
                previousMouseX = Input.mouseState.X;
                previousMouseY = Input.mouseState.Y;

                //Gizmos
                if (hoveredId >= 1 && hoveredId <= 3)
                {
                    _gizmoId = hoveredId;
                    GizmoControl(_gizmoId, data);
                    return;
                }

                if (hoveredId <= 0)
                {
                    SelectedObject = null;
                    return;
                }

                bool foundnew = false;
                //Get the selected entity!
                for (int index = 0; index < entities.Count; index++)
                {
                    var VARIABLE = entities[index];
                    if (VARIABLE.Id == hoveredId)
                    {
                        SelectedObject = VARIABLE;
                        foundnew = true;
                        break;
                    }
                }
                if (foundnew == false)
                {
                    for (int index = 0; index < decals.Count; index++)
                    {
                        Decal decal = decals[index];
                        if (decal.Id == hoveredId)
                        {
                            SelectedObject = decal;
                            break;
                        }
                    }
                    
                    for (int index = 0; index < pointLights.Count; index++)
                    {
                        PointLight pointLight = pointLights[index];
                        if (pointLight.Id == hoveredId)
                        {
                            SelectedObject = pointLight;
                            break;
                        }
                    }

                    for (int index = 0; index < dirLights.Count; index++)
                    {
                        DirectionalLight directionalLight = dirLights[index];
                        if (directionalLight.Id == hoveredId)
                        {
                            SelectedObject = directionalLight;
                            break;
                        }
                    }

                    {
                        if (envSample.Id == hoveredId)
                        {
                            SelectedObject = envSample;
                        }
                    }

                    for (int index = 0; index < debugEntities.Count; index++)
                    {
                        DirectionalLight debugEntity = dirLights[index];
                        if (debugEntity.Id == hoveredId)
                        {
                            SelectedObject = debugEntity;
                            break;
                        }
                    }
                }

            }

            //Controls

            if (Input.WasKeyPressed(Keys.Delete))
            {
                //Find object
                if (SelectedObject is BasicEntity)
                {
                    entities.Remove((BasicEntity) SelectedObject);
                    meshMaterialLibrary.DeleteFromRegistry((BasicEntity) SelectedObject);

                    SelectedObject = null;
                }
                else if (SelectedObject is Decal)
                {
                    decals.Remove((Decal) SelectedObject);

                    SelectedObject = null;
                }
                else if (SelectedObject is PointLight)
                {
                    pointLights.Remove((PointLight) SelectedObject);

                    SelectedObject = null;
                }
                else if (SelectedObject is DirectionalLight)
                {
                    dirLights.Remove((DirectionalLight) SelectedObject);

                    SelectedObject = null;
                }
            }

            if (Input.WasKeyPressed(Keys.Insert) || (Input.keyboardState.IsKeyDown(Keys.LeftControl) && Input.WasKeyPressed(Keys.C)))
            {
                if (SelectedObject is BasicEntity)
                {
                    BasicEntity copy = (BasicEntity)SelectedObject.Clone;
                    copy.RegisterInLibrary(meshMaterialLibrary);
    
                    entities.Add(copy);
                }
                else if (SelectedObject is Decal)
                {
                    Decal copy = (Decal)SelectedObject.Clone;
                    decals.Add(copy);
                }
                else if (SelectedObject is PointLight)
                {
                    PointLight copy = (PointLight)SelectedObject.Clone;
                    pointLights.Add(copy);
                }
                else if (SelectedObject is DirectionalLight)
                {
                    DirectionalLight copy = (DirectionalLight)SelectedObject.Clone;
                    dirLights.Add(copy);
                }
            }
            
        }

        private void GizmoControl(int gizmoId, EditorReceivedData data)
        {
            if (SelectedObject == null) return;
            //there must be a selected object for a gizmo

            float x = Input.mouseState.X;
            float y = Input.mouseState.Y;



            if (_gizmoMode == GizmoModes.Translation)
            {

                Vector3 pos1 =
                    _graphicsDevice.Viewport.Unproject(new Vector3(x, y, 0),
                        data.ProjectionMatrix, data.ViewMatrix, Matrix.Identity);
                Vector3 pos2 = _graphicsDevice.Viewport.Unproject(new Vector3(x, y, 1),
                    data.ProjectionMatrix, data.ViewMatrix, Matrix.Identity);

                Ray ray = new Ray(pos1, pos2 - pos1);
                
                Vector3 normal;
                Vector3 binormal;
                Vector3 tangent;

                if (gizmoId == 1)
                {
                    tangent = Vector3.UnitZ;
                    normal = Vector3.UnitZ;
                    binormal = Vector3.UnitY;
                }
                else if (gizmoId == 2)
                {
                    tangent = Vector3.UnitY;
                    normal = Vector3.UnitY;
                    binormal = Vector3.UnitZ;
                }
                else
                {
                    tangent = Vector3.UnitX;
                    normal = Vector3.UnitZ;
                    binormal = Vector3.UnitX;
                }

                if (GameStats.e_LocalTransformation)
                {
                    tangent = Vector3.Transform(tangent, SelectedObject.RotationMatrix);
                    normal = Vector3.Transform(normal, SelectedObject.RotationMatrix);
                    binormal = Vector3.Transform(binormal, SelectedObject.RotationMatrix);
                }

                Plane plane = new Plane(SelectedObject.Position, SelectedObject.Position + normal,
                       SelectedObject.Position + binormal);


                float? d = ray.Intersects(plane);

                if (d == null) return;

                float f = (float) d;

                Vector3 hitPoint = pos1 + (pos2 - pos1)*f;

                if (_gizmoTransformationMode == false)
                {
                    _gizmoTransformationMode = true;
                    _gizmoPosition = hitPoint;
                    return;
                }


                //Get the difference
                Vector3 diff = hitPoint - _gizmoPosition;

                diff = Vector3.Dot(tangent, diff)*tangent;

                //diff.Z *= gizmoId == 1 ? 1 : 0;
                //diff.Y *= gizmoId == 2 ? 1 : 0;
                //diff.X *= gizmoId == 3 ? 1 : 0;

                SelectedObject.Position += diff;
                
                _gizmoPosition = hitPoint;
            }
            else
            {
                if (_gizmoTransformationMode == false)
                {
                    _gizmoTransformationMode = true;
                    return;
                }

                float diffL = x - previousMouseX + y - previousMouseY;
                diffL /= 50;

                if (Input.keyboardState.IsKeyDown(Keys.LeftControl))
                    gizmoId = 4;

                if (_gizmoMode == GizmoModes.Rotation)
                {

                    if (!GameStats.e_LocalTransformation)
                    {
                        if (gizmoId == 1 )
                        {
                            SelectedObject.RotationMatrix = SelectedObject.RotationMatrix*
                                                            Matrix.CreateRotationZ((float) diffL);
                        }
                        if (gizmoId == 2 )
                        {
                            SelectedObject.RotationMatrix = SelectedObject.RotationMatrix*
                                                            Matrix.CreateRotationY((float) diffL);
                        }
                        if (gizmoId == 3)
                        {
                            SelectedObject.RotationMatrix = SelectedObject.RotationMatrix*
                                                            Matrix.CreateRotationX((float) diffL);
                        }
                    }
                    else
                    {
                        if (gizmoId == 1)
                        {
                            SelectedObject.RotationMatrix = Matrix.CreateRotationZ((float) diffL)*
                                                            SelectedObject.RotationMatrix;
                        }
                        if (gizmoId == 2)
                        {
                            SelectedObject.RotationMatrix = Matrix.CreateRotationY((float) diffL)*
                                                            SelectedObject.RotationMatrix;
                        }
                        if (gizmoId == 3)
                        {
                            SelectedObject.RotationMatrix = Matrix.CreateRotationX((float) diffL)*
                                                            SelectedObject.RotationMatrix;
                        }
                    }
                }
                else
                {
                    if (gizmoId == 1 || gizmoId == 4)
                    {
                        SelectedObject.Scale = new Vector3(SelectedObject.Scale.X, SelectedObject.Scale.Y, MathHelper.Max(SelectedObject.Scale.Z + (float) diffL, 0.01f));
                    }
                    if (gizmoId == 2 || gizmoId == 4)
                    {
                        SelectedObject.Scale = new Vector3(SelectedObject.Scale.X,  MathHelper.Max(SelectedObject.Scale.Y + (float)diffL, 0.01f), SelectedObject.Scale.Z);
                    }
                    if (gizmoId == 3 || gizmoId == 4)
                    {
                        SelectedObject.Scale = new Vector3(MathHelper.Max(SelectedObject.Scale.X + (float)diffL, 0.01f), SelectedObject.Scale.Y, SelectedObject.Scale.Z);
                    }
                }


                previousMouseX = x;
                previousMouseY = y;
            }

        }

        public EditorSendData GetEditorData()
        {
            if (SelectedObject == null)
                return new EditorSendData {SelectedObjectId = 0, SelectedObjectPosition = Vector3.Zero};
            return new EditorSendData
            {
                SelectedObject = SelectedObject,
                SelectedObjectId = SelectedObject.Id,
                SelectedObjectPosition = SelectedObject.Position,
                GizmoTransformationMode = _gizmoTransformationMode,
                GizmoMode =  _gizmoMode,
                
            };
        }

    }
}
