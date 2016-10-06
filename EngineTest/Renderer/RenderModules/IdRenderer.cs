using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineTest.Main;
using EngineTest.Recources;
using EngineTest.Recources.Helper;
using EngineTest.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EngineTest.Renderer.RenderModules
{
    public class IdRenderer
    {
        GraphicsDevice _graphicsDevice;

        private RenderTarget2D _idRenderTarget2D;

        public int HoveredId = 0;

        private Vector4 hoveredColor = Color.White.ToVector4();
        private Vector4 selectedColor = Color.Yellow.ToVector4();

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

        }

        public void Draw(MeshMaterialLibrary meshMat, Matrix viewProjection, EditorLogic.EditorSendData editorData, bool mouseMoved, Assets _assets)
        {
            if (editorData.GizmoTransformationMode)
            {
                _graphicsDevice.SetRenderTarget(_idRenderTarget2D);
                _graphicsDevice.Clear(Color.Black);
                return;
            }

            if (mouseMoved)
            {
                DrawIds(meshMat, viewProjection, editorData, _assets);
                DrawOutlines(meshMat, viewProjection, true, HoveredId, editorData, mouseMoved);
            }
            else
            {
                DrawOutlines(meshMat, viewProjection, false, HoveredId, editorData, mouseMoved);
            }
        }

        public void DrawIds(MeshMaterialLibrary meshMat, Matrix viewProjection, EditorLogic.EditorSendData editorData, Assets _assets)
        {
            
            _graphicsDevice.SetRenderTarget(_idRenderTarget2D);
            _graphicsDevice.BlendState = BlendState.Opaque;
            
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            meshMat.Draw(MeshMaterialLibrary.RenderType.idRender, _graphicsDevice, viewProjection, false, false);

            //Now onto the gizmos
            DrawGizmos(viewProjection, editorData, _assets);

            Rectangle sourceRectangle =
            new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 1, 1);

            Color[] retrievedColor = new Color[1];

            _idRenderTarget2D.GetData<Color>(0, sourceRectangle, retrievedColor, 0, 1);

            HoveredId = IdGenerator.GetIdFromColor(retrievedColor[0]);
        }

        public void DrawGizmos(Matrix staticViewProjection, EditorLogic.EditorSendData editorData, Assets _assets)
        {
            if (editorData.SelectedObjectId == 0) return;

            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.BlendState = BlendState.Opaque;

            Vector3 position = editorData.SelectedObjectPosition;

            //Z
            DrawArrow(position, Math.PI, 0, 0, 0.5f, new Color(1,0,0), staticViewProjection, _assets);
            DrawArrow(position, -Math.PI / 2, 0, 0, 0.5f, new Color(2, 0, 0), staticViewProjection, _assets);
            DrawArrow(position, 0, Math.PI / 2, 0, 0.5f, new Color(3, 0, 0), staticViewProjection, _assets);
        }

        private void DrawArrow(Vector3 Position, double AngleX, double AngleY, double AngleZ, float Scale, Color color, Matrix staticViewProjection, Assets _assets)
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

        public void DrawOutlines(MeshMaterialLibrary meshMat, Matrix viewProjection, bool drawAll, int hoveredId, EditorLogic.EditorSendData editorData, bool mouseMoved)
        {
            _graphicsDevice.SetRenderTarget(_idRenderTarget2D);
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            int selectedId = editorData.SelectedObjectId;

            //Selected entity
            if (selectedId != 0)
            {
                if (!drawAll)
                    meshMat.Draw(MeshMaterialLibrary.RenderType.idOutline, _graphicsDevice, viewProjection, false, false,
                        false, selectedId);

                Shaders.IdRenderEffectParameterColorId.SetValue(selectedColor);
                meshMat.Draw(MeshMaterialLibrary.RenderType.idOutline, _graphicsDevice, viewProjection, false, false,
                    outlined: true, outlineId: selectedId);
            }

            if (selectedId != hoveredId && hoveredId!=0 && mouseMoved)
            {
                if (!drawAll) meshMat.Draw(MeshMaterialLibrary.RenderType.idOutline, _graphicsDevice, viewProjection, false, false, false, hoveredId);

                Shaders.IdRenderEffectParameterColorId.SetValue(hoveredColor);
                meshMat.Draw(MeshMaterialLibrary.RenderType.idOutline, _graphicsDevice, viewProjection, false, false, outlined: true, outlineId: hoveredId);
            }
        }

        public RenderTarget2D GetRT()
        {
            return _idRenderTarget2D;
        }


        public void SetUpRenderTarget(int width, int height)
        {
            if(_idRenderTarget2D != null) _idRenderTarget2D.Dispose();

            _idRenderTarget2D = new RenderTarget2D(_graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
        }


    }
}
