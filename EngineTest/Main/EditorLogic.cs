using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineTest.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EngineTest.Main
{
    public class EditorLogic
    {
        private int _selectedId = 0;

        private bool gizmoMode = false;
        private Vector3 gizmoPosition;
        private int gizmoId = 0;

        public TransformableObject SelectedObject;

        private GraphicsDevice _graphicsDevice;

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
            int hoveredId = data.HoveredId;

            if (gizmoMode)
            {
                if (Input.mouseState.LeftButton == ButtonState.Pressed)
                {
                    GizmoControl(gizmoId, data);
                }
                else gizmoMode = false;
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

            if(gizmoId == 1)
            {
                plane = new Plane(SelectedObject.Position, SelectedObject.Position + Vector3.UnitZ, SelectedObject.Position + Vector3.UnitY);
            }
            else if (gizmoId == 2)
            {
                plane = new Plane(SelectedObject.Position, SelectedObject.Position + Vector3.UnitY, SelectedObject.Position + Vector3.UnitZ);
            }
            else if (gizmoId == 3)
            {
                plane = new Plane(SelectedObject.Position, SelectedObject.Position + Vector3.UnitZ, SelectedObject.Position + Vector3.UnitX);
            }

            float? d = ray.Intersects(plane);

            if (d == null) return;

            float f = (float) d;

            Vector3 hitPoint = pos1 + (pos2 - pos1)*f;

            if (gizmoMode == false)
            {
                gizmoMode = true;
                gizmoPosition = hitPoint;
                return;
            }
            
            //Get the difference
            Vector3 diff = hitPoint - gizmoPosition;

            diff.Z *= gizmoId == 1 ? 1 : 0;
            diff.Y *= gizmoId == 2 ? 1 : 0;
            diff.X *= gizmoId == 3 ? 1 : 0;

            SelectedObject.Position += diff;

            gizmoPosition = hitPoint;

        }

        public EditorSendData GetEditorData()
        {
            if (SelectedObject == null)
                return new EditorSendData() {SelectedObjectId = 0, SelectedObjectPosition = Vector3.Zero};
            return new EditorSendData()
            {
                SelectedObjectId = SelectedObject.Id,
                SelectedObjectPosition = SelectedObject.Position
            };
        }

    }
}
