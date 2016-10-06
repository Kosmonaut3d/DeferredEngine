using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineTest.Entities;
using EngineTest.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EngineTest.Main
{
    public class EditorLogic
    {
        private int _selectedId = 0;

        private bool gizmoTransformationMode = false;
        private Vector3 gizmoPosition;
        private int gizmoId = 0;
        private GizmoModes gizmoMode = GizmoModes.translation;

        public TransformableObject SelectedObject;

        private GraphicsDevice _graphicsDevice;

        public enum GizmoModes
        {
            translation,
            rotation
        };

        public struct EditorReceivedData
        {
           public int HoveredId;
            public Matrix ViewMatrix;
            public Matrix ProjectionMatrix;
        }

        public struct EditorSendData
        {
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
        public void Update(GameTime gameTime, List<BasicEntity> entities, EditorReceivedData data)
        {
            if (!GameSettings.Editor_enable) return;

            if(Input.WasKeyPressed(Keys.R)) gizmoMode = GizmoModes.rotation;
            if (Input.WasKeyPressed(Keys.T)) gizmoMode = GizmoModes.translation;

            int hoveredId = data.HoveredId;

            if (gizmoTransformationMode)
            {
                if (Input.mouseState.LeftButton == ButtonState.Pressed)
                {
                    GizmoControl(gizmoId, data);
                }
                else gizmoTransformationMode = false;
            }
            else if (Input.WasLMBPressed())
            {
                //Gizmos
                if (hoveredId >= 1 && hoveredId <= 3)
                {
                    gizmoId = hoveredId;
                    GizmoControl(gizmoId, data);
                    return;
                }

                if (hoveredId <= 0)
                {
                    SelectedObject = null;
                    return;
                }
                //Get the selected entity!
                foreach (var VARIABLE in entities)
                {
                    if (VARIABLE.Id == hoveredId)
                    {
                        SelectedObject = VARIABLE;
                        break;
                    }
                }

            }
        }

        private void GizmoControl(int gizmoId, EditorReceivedData data)
        {
            //there must be a selected object for a gizmo

            float x = Input.mouseState.X;
            float y = Input.mouseState.Y;

            Vector3 pos1 =
                _graphicsDevice.Viewport.Unproject(new Vector3(x, y, 0),
                    data.ProjectionMatrix, data.ViewMatrix, Matrix.Identity);
            Vector3 pos2 = _graphicsDevice.Viewport.Unproject(new Vector3(x, y, 1),
                    data.ProjectionMatrix, data.ViewMatrix, Matrix.Identity);

            Ray ray = new Ray(pos1, pos2-pos1);

            Plane plane = new Plane();

            if (gizmoMode == GizmoModes.translation)
            {
                if (gizmoId == 1)
                {
                    plane = new Plane(SelectedObject.Position, SelectedObject.Position + Vector3.UnitZ,
                        SelectedObject.Position + Vector3.UnitY);
                }
                else if (gizmoId == 2)
                {
                    plane = new Plane(SelectedObject.Position, SelectedObject.Position + Vector3.UnitY,
                        SelectedObject.Position + Vector3.UnitZ);
                }
                else if (gizmoId == 3)
                {
                    plane = new Plane(SelectedObject.Position, SelectedObject.Position + Vector3.UnitZ,
                        SelectedObject.Position + Vector3.UnitX);
                }
            }
            else //rotation
            {
                if (gizmoId == 1)
                {
                    plane = new Plane(SelectedObject.Position, SelectedObject.Position + Vector3.UnitX,
                        SelectedObject.Position + Vector3.UnitY);
                }
                else if (gizmoId == 2)
                {
                    plane = new Plane(SelectedObject.Position, SelectedObject.Position + Vector3.UnitX,
                        SelectedObject.Position + Vector3.UnitZ);
                }
                else if (gizmoId == 3)
                {
                    plane = new Plane(SelectedObject.Position, SelectedObject.Position + Vector3.UnitZ,
                        SelectedObject.Position + Vector3.UnitY);
                }
            }

            float? d = ray.Intersects(plane);

            if (d == null) return;

            float f = (float) d;

            Vector3 hitPoint = pos1 + (pos2 - pos1)*f;

            if (gizmoTransformationMode == false)
            {
                gizmoTransformationMode = true;
                gizmoPosition = hitPoint;
                return;
            }
            
            //Get the difference
            Vector3 diff = hitPoint - gizmoPosition;

            if (gizmoMode == GizmoModes.translation)
            {
                diff.Z *= gizmoId == 1 ? 1 : 0;
                diff.Y *= gizmoId == 2 ? 1 : 0;
                diff.X *= gizmoId == 3 ? 1 : 0;

                SelectedObject.Position += diff;
            }
            else
            {
                diff.Z *= gizmoId == 1 ? 0 : 1;
                diff.Y *= gizmoId == 2 ? 0 : 1;
                diff.X *= gizmoId == 3 ? 0 : 1;

                float diffL = diff.X + diff.Y + diff.Z;

                diffL /= 10;

                if (gizmoId == 1) //Z
                {
                    SelectedObject.AngleZ += diffL;
                }
                if (gizmoId == 2) //Z
                {
                    SelectedObject.AngleY += diffL;
                }
                if (gizmoId == 3) //Z
                {
                    SelectedObject.AngleX += diffL;
                }
            }


            gizmoPosition = hitPoint;

        }

        public EditorSendData GetEditorData()
        {
            if (SelectedObject == null)
                return new EditorSendData() {SelectedObjectId = 0, SelectedObjectPosition = Vector3.Zero};
            return new EditorSendData()
            {
                SelectedObjectId = SelectedObject.Id,
                SelectedObjectPosition = SelectedObject.Position,
                GizmoTransformationMode = gizmoTransformationMode,
                GizmoMode =  gizmoMode
            };
        }

    }
}
