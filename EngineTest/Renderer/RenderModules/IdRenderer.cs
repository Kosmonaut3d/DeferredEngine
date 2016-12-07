using System;
using System.Collections.Generic;
using EngineTest.Entities.Editor;
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

        private BillboardBuffer _billboardBuffer;
        private Assets _assets;

        public void Initialize(GraphicsDevice graphicsDevice, BillboardBuffer billboardBuffer, Assets assets)
        {
            _graphicsDevice = graphicsDevice;
            _billboardBuffer = billboardBuffer;
            _assets = assets;
        }

        public void Draw(MeshMaterialLibrary meshMat, List<PointLightSource> pointLights, List<DirectionalLightSource> dirLights, Matrix viewProjection, Matrix view, EditorLogic.EditorSendData editorData, bool mouseMoved)
        {
            if (editorData.GizmoTransformationMode)
            {
                _graphicsDevice.SetRenderTarget(_idRenderTarget2D);
                _graphicsDevice.Clear(Color.Black);
                return;
            }

            if (mouseMoved)
            {
                DrawIds(meshMat, pointLights, dirLights, viewProjection, view, editorData);
                DrawOutlines(meshMat, viewProjection, true, HoveredId, editorData, mouseMoved);
            }
            else
            {
                DrawOutlines(meshMat, viewProjection, false, HoveredId, editorData, mouseMoved);
            }
        }

        public void DrawIds(MeshMaterialLibrary meshMat, List<PointLightSource> pointLights, List<DirectionalLightSource> dirLights, Matrix viewProjection, Matrix view, EditorLogic.EditorSendData editorData)
        {
            
            _graphicsDevice.SetRenderTarget(_idRenderTarget2D);
            _graphicsDevice.BlendState = BlendState.Opaque;
            
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            meshMat.Draw(MeshMaterialLibrary.RenderType.idRender, _graphicsDevice, viewProjection, false, false);

            //Now onto the billboards
            DrawBillboards(pointLights, dirLights, viewProjection, view);

            //Now onto the gizmos
            DrawGizmos(viewProjection, editorData, _assets);
            
            Rectangle sourceRectangle =
            new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 1, 1);

            Color[] retrievedColor = new Color[1];

            _idRenderTarget2D.GetData<Color>(0, sourceRectangle, retrievedColor, 0, 1);

            HoveredId = IdGenerator.GetIdFromColor(retrievedColor[0]);
        }

        public void DrawBillboards(List<PointLightSource> lights, List<DirectionalLightSource> dirLights, Matrix staticViewProjection, Matrix view)
        {
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.SetVertexBuffer(_billboardBuffer.VBuffer);
            _graphicsDevice.Indices = (_billboardBuffer.IBuffer);

            Shaders.BillboardEffectParameter_Texture.SetValue(_assets.Icon_Light);

            Shaders.BillboardEffect.CurrentTechnique = Shaders.BillboardEffectTechnique_Id;

            foreach (var light in lights)
            {
                Matrix world = Matrix.CreateTranslation(light.Position);
                Shaders.BillboardEffectParameter_WorldViewProj.SetValue(world * staticViewProjection);
                Shaders.BillboardEffectParameter_WorldView.SetValue(world * view);
                Shaders.BillboardEffectParameter_IdColor.SetValue(IdGenerator.GetColorFromId(light.Id).ToVector3());
                Shaders.BillboardEffect.CurrentTechnique.Passes[0].Apply();

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            }

            foreach (var light in dirLights)
            {
                Matrix world = Matrix.CreateTranslation(light.Position);
                Shaders.BillboardEffectParameter_WorldViewProj.SetValue(world * staticViewProjection);
                Shaders.BillboardEffectParameter_WorldView.SetValue(world * view);
                Shaders.BillboardEffectParameter_IdColor.SetValue(IdGenerator.GetColorFromId(light.Id).ToVector3());
                Shaders.BillboardEffect.CurrentTechnique.Passes[0].Apply();

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            }
        }

        public void DrawBillboardsSelection(List<PointLightSource> lights, Matrix staticViewProjection, Matrix view, int selectedId, int hoveredId)
        {
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.SetVertexBuffer(_billboardBuffer.VBuffer);
            _graphicsDevice.Indices = (_billboardBuffer.IBuffer);

            Shaders.BillboardEffectParameter_Texture.SetValue(_assets.Icon_Light);

            Shaders.BillboardEffect.CurrentTechnique = Shaders.BillboardEffectTechnique_Id;

            foreach (var light in lights)
            {
                if (light.Id != selectedId && light.Id != hoveredId) continue;

                Shaders.BillboardEffectParameter_IdColor.SetValue(
                    light.Id == hoveredId ? Color.Gray.ToVector3() : Color.DarkOrange.ToVector3());
                Matrix world = Matrix.CreateTranslation(light.Position);
                Shaders.BillboardEffectParameter_WorldViewProj.SetValue(world * staticViewProjection);
                Shaders.BillboardEffectParameter_WorldView.SetValue(world * view);
                Shaders.BillboardEffect.CurrentTechnique.Passes[0].Apply();

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            }
        }

        public void DrawGizmos(Matrix staticViewProjection, EditorLogic.EditorSendData editorData, Assets _assets)
        {
            if (editorData.SelectedObjectId == 0) return;

            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.BlendState = BlendState.Opaque;

            Vector3 position = editorData.SelectedObjectPosition;

            //Z
            DrawArrow(position, 0, 0, 0, 0.5f, new Color(1,0,0), staticViewProjection, _assets);
            DrawArrow(position, -Math.PI / 2, 0, 0, 0.5f, new Color(2, 0, 0), staticViewProjection, _assets);
            DrawArrow(position, 0, Math.PI / 2, 0, 0.5f, new Color(3, 0, 0), staticViewProjection, _assets);

            DrawArrow(position, Math.PI, 0, 0, 0.5f, new Color(1, 0, 0), staticViewProjection, _assets);
            DrawArrow(position, Math.PI / 2, 0, 0, 0.5f, new Color(2, 0, 0), staticViewProjection, _assets);
            DrawArrow(position, 0, -Math.PI / 2, 0, 0.5f, new Color(3, 0, 0), staticViewProjection, _assets);

        }

        private void DrawArrow(Vector3 Position, double AngleX, double AngleY, double AngleZ, float Scale, Color color, Matrix staticViewProjection, Assets _assets)
        {
            Matrix Rotation = Matrix.CreateRotationX((float)AngleX) * Matrix.CreateRotationY((float)AngleY) *
                               Matrix.CreateRotationZ((float)AngleZ);
            Matrix ScaleMatrix = Matrix.CreateScale(0.75f, 0.75f, Scale * 1.5f);
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
