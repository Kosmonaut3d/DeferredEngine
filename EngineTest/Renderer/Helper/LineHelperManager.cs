using System.Collections.Generic;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.Helper
{
    public static class LineHelperManager
    {
        private static readonly List<LineHelper> Lines = new List<LineHelper>();
        //private static VertexBuffer _vbuffer;
        //private static IndexBuffer _ibuffer;

        private static int _tempVertsPoolLength = 100;
        public static VertexPositionColor[] TempVertsPool = new VertexPositionColor[_tempVertsPoolLength];
        private static int _tempVertsPoolIndex;
        private static int _tempVertsPoolOverCount;

        public static VertexPositionColor GetVertexPositionColor(Vector3 point, Color color)
        {
            if (_tempVertsPoolIndex < _tempVertsPoolLength - 3) //Buffer
            {
                TempVertsPool[_tempVertsPoolIndex].Position = point;
                TempVertsPool[_tempVertsPoolIndex].Color = color;
                _tempVertsPoolIndex++;
                return TempVertsPool[_tempVertsPoolIndex - 1];
            }
            _tempVertsPoolOverCount++;
            return new VertexPositionColor(point, color);
        }

        private static void AdjustTempVertsPoolSize()
        {
            if (_tempVertsPoolOverCount > 0)
            {
                _tempVertsPoolLength += _tempVertsPoolOverCount;
                TempVertsPool = new VertexPositionColor[_tempVertsPoolLength];
            }

            _tempVertsPoolOverCount = 0;
            _tempVertsPoolIndex = 0;
        }

        public static void AddLineStartEnd(Vector3 start, Vector3 end, short timer)
        {
            LineHelper lineHelper = new LineHelper(start, end,timer);
            Lines.Add(lineHelper);
        }

        public static void AddLineStartDir(Vector3 start, Vector3 dir, short timer)
        {
            LineHelper lineHelper = new LineHelper(start, start+dir,timer);   
            Lines.Add(lineHelper);
        }

        public static void AddLineStartEnd(Vector3 start, Vector3 end, short timer, Color startColor, Color endColor)
        {
            LineHelper lineHelper = new LineHelper(start, end, timer, startColor, endColor);
            Lines.Add(lineHelper);
        }

        public static void AddLineStartDir(Vector3 start, Vector3 dir, short timer, Color startColor, Color endColor)
        {
            LineHelper lineHelper = new LineHelper(start, start + dir, timer, startColor, endColor);
            Lines.Add(lineHelper);
        }

        public static void Draw(GraphicsDevice graphicsDevice, Matrix viewProjection)
        {
            if (!GameSettings.d_drawlines) return;

            Shaders.LineEffectParameter_WorldViewProj.SetValue(viewProjection);


        //    _vbuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), Lines.Count*2, BufferUsage.WriteOnly);
        //    _ibuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, Lines.Count*2, BufferUsage.WriteOnly);
            for (int i = 0; i < Lines.Count; i++)
            {
                LineHelper line = Lines[i];
                if (line != null)
                {
                        Shaders.LineEffect.CurrentTechnique.Passes[0].Apply();

                        //Gather
                        graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, line.Verts, 0, 2, LineHelper.Indices,
                            0,
                            1);
                    
                    line.Timer--;
                    if (line.Timer <= 0)
                    {
                        Lines.RemoveAt(i);
                        i--;
                    }
                }
                else
                {
                    Lines.RemoveAt(i);
                }

            }
            AdjustTempVertsPoolSize();
        }

        public static void CreateBoundingBoxLines(BoundingFrustum boundingFrustumShadow)
        {
            Vector3[] vertices = boundingFrustumShadow.GetCorners();
            AddLineStartEnd(vertices[0], vertices[1], 1);
            AddLineStartEnd(vertices[1], vertices[2], 1);
            AddLineStartEnd(vertices[2], vertices[3], 1);
            AddLineStartEnd(vertices[3], vertices[0], 1);

            AddLineStartEnd(vertices[0], vertices[4], 1);
            AddLineStartEnd(vertices[1], vertices[5], 1);
            AddLineStartEnd(vertices[2], vertices[6], 1);
            AddLineStartEnd(vertices[3], vertices[7], 1);

            AddLineStartEnd(vertices[4], vertices[5], 1);
            AddLineStartEnd(vertices[5], vertices[6], 1);
            AddLineStartEnd(vertices[6], vertices[7], 1);
            AddLineStartEnd(vertices[7], vertices[4], 1);
        }
    }
}
