using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineTest.Main;
using EngineTest.Recources;
using EngineTest.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Renderer.RenderModules
{
    public class EditorRender
    {
        public IdRenderer _idRenderer;
        public GraphicsDevice _graphicsDevice;

        private Assets _assets;

        private double mouseMoved;
        private bool mouseMovement = false;

        public void Initialize(GraphicsDevice graphics, Assets assets)
        {
            _graphicsDevice = graphics;

            _idRenderer = new IdRenderer();
            _idRenderer.Initialize(graphics);

            _assets = assets;

        }

        public void Update(GameTime gameTime)
        {
            if (Input.mouseState != Input.mouseLastState)
            {
                //reset the timer!

                mouseMoved = gameTime.TotalGameTime.TotalMilliseconds + 500;
                mouseMovement = true;
            }

            if (mouseMoved < gameTime.TotalGameTime.TotalMilliseconds)
            {
                mouseMovement = false;
            }

        }

        public void SetUpRenderTarget(int width, int height)
        {
            _idRenderer.SetUpRenderTarget(width, height);
        }

        public void DrawIds(MeshMaterialLibrary meshMaterialLibrary, Matrix staticViewProjection, EditorLogic.EditorSendData editorData)
        {
            _idRenderer.Draw(meshMaterialLibrary, staticViewProjection, editorData, mouseMovement, _assets);
        }

        public void DrawEditorElements(MeshMaterialLibrary meshMaterialLibrary, Matrix staticViewProjection, EditorLogic.EditorSendData editorData)
        {
            DrawGizmo(staticViewProjection, editorData);
        }

        public void DrawGizmo(Matrix staticViewProjection, EditorLogic.EditorSendData editorData)
        {
            if (editorData.SelectedObjectId == 0) return;

            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;

            Vector3 position = editorData.SelectedObjectPosition;

            //Z
            DrawArrow(position, Math.PI, 0,0, GetHoveredId()==1 ? 1 : 0.5f, Color.Blue, staticViewProjection); //z 1
            DrawArrow(position, -Math.PI / 2, 0, 0, GetHoveredId()==2 ? 1 : 0.5f, Color.Green, staticViewProjection); //y 2
            DrawArrow(position, 0, Math.PI / 2, 0,GetHoveredId()==3 ? 1 : 0.5f, Color.Red, staticViewProjection); //x 3
        }

        private void DrawArrow(Vector3 Position, double AngleX, double AngleY, double AngleZ, float Scale, Color color, Matrix staticViewProjection)
        {
            Matrix Rotation = Matrix.CreateRotationX((float)AngleX) * Matrix.CreateRotationY((float)AngleY) *
                               Matrix.CreateRotationZ((float)AngleZ);
            Matrix ScaleMatrix = Matrix.CreateScale(Scale);
            Matrix WorldViewProj = ScaleMatrix * Rotation * Matrix.CreateTranslation(Position) * staticViewProjection;

            Shaders.IdRenderEffectParameterWorldViewProj.SetValue(WorldViewProj);
            Shaders.IdRenderEffectParameterColorId.SetValue(color.ToVector4());
            foreach (ModelMesh mesh in _assets.EditorArrow.Meshes)
            {
                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                {
                    Shaders.IdRenderEffectDrawId.Apply();

                    _graphicsDevice.SetVertexBuffer(meshpart.VertexBuffer);
                    _graphicsDevice.Indices = (meshpart.IndexBuffer);
                    int primitiveCount = meshpart.PrimitiveCount;
                    int vertexOffset = meshpart.VertexOffset;
                    int vCount = meshpart.NumVertices;
                    int startIndex = meshpart.StartIndex;

                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex, primitiveCount);
                }
            }
        }

        public RenderTarget2D GetOutlines()
        {
            return _idRenderer.GetRT();
        }

        /// <summary>
        /// Returns the id of the currently hovered object
        /// </summary>
        /// <returns></returns>
        public int GetHoveredId()
        {
            return _idRenderer.HoveredId;
        }
    }
}
