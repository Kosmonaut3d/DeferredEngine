using System.Collections.Generic;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.Helper.HelperGeometry
{
    public class LineHelperManager
    {
        private  readonly List<LineHelper> Lines = new List<LineHelper>();
        //private  VertexBuffer _vbuffer;
        //private  IndexBuffer _ibuffer;
        
        private int _tempVertsPoolLength = 100;
        public VertexPositionColor[] TempVertsPool;
        private int _tempVertsPoolIndex;
        private int _tempVertsPoolOverCount;

        public LineHelperManager()
        {
            TempVertsPool = new VertexPositionColor[_tempVertsPoolLength];
        }

        public  VertexPositionColor GetVertexPositionColor(Vector3 point, Color color)
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

        private  void AdjustTempVertsPoolSize()
        {
            if (_tempVertsPoolOverCount > 0)
            {
                _tempVertsPoolLength += _tempVertsPoolOverCount;
                TempVertsPool = new VertexPositionColor[_tempVertsPoolLength];
            }

            _tempVertsPoolOverCount = 0;
            _tempVertsPoolIndex = 0;
        }

        public  void AddLineStartEnd(Vector3 start, Vector3 end, short timer)
        {
            LineHelper lineHelper = new LineHelper(start, end,timer, this);
            Lines.Add(lineHelper);
        }

        public  void AddLineStartDir(Vector3 start, Vector3 dir, short timer)
        {
            LineHelper lineHelper = new LineHelper(start, start+dir,timer, this);   
            Lines.Add(lineHelper);
        }

        public  void AddLineStartEnd(Vector3 start, Vector3 end, short timer, Color startColor, Color endColor)
        {
            LineHelper lineHelper = new LineHelper(start, end, timer, startColor, endColor, this);
            Lines.Add(lineHelper);
        }

        public  void AddLineStartDir(Vector3 start, Vector3 dir, short timer, Color startColor, Color endColor)
        {
            LineHelper lineHelper = new LineHelper(start, start + dir, timer, startColor, endColor, this);
            Lines.Add(lineHelper);
        }

        public  void AddFrustum(BoundingFrustumEx frustum, short timer, Color color)
        {
            Vector3[] corners = frustum.GetCornersNoCopy();
            //Front
            Lines.Add(new LineHelper(corners[0], corners[1], 1, color, color, this));
            Lines.Add(new LineHelper(corners[1], corners[2], 1, color, color, this));
            Lines.Add(new LineHelper(corners[2], corners[3], 1, color, color, this));
            Lines.Add(new LineHelper(corners[3], corners[0], 1, color, color, this));
            //Back
            Lines.Add(new LineHelper(corners[4], corners[5], 1, color, color, this));
            Lines.Add(new LineHelper(corners[5], corners[6], 1, color, color, this));
            Lines.Add(new LineHelper(corners[6], corners[7], 1, color, color, this));
            Lines.Add(new LineHelper(corners[7], corners[4], 1, color, color, this));
            //Between
            Lines.Add(new LineHelper(corners[4], corners[0], 1, color, color, this));
            Lines.Add(new LineHelper(corners[5], corners[1], 1, color, color, this));
            Lines.Add(new LineHelper(corners[6], corners[2], 1, color, color, this));
            Lines.Add(new LineHelper(corners[7], corners[3], 1, color, color, this));
        }

        public  void Draw(GraphicsDevice graphicsDevice, Matrix viewProjection, EffectParameter worldViewProjection, EffectPass vertexColorPass)
        {
            if (!GameSettings.d_drawlines) return;
            
            worldViewProjection.SetValue(viewProjection);

            for (int i = 0; i < Lines.Count; i++)
            {
                LineHelper line = Lines[i];
                if (line != null)
                {
                        vertexColorPass.Apply();

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

        public  void CreateBoundingBoxLines(BoundingFrustum boundingFrustumExShadow)
        {
            Vector3[] vertices = boundingFrustumExShadow.GetCorners();
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

        public void AddBoundingBox(BasicEntity basicEntity)
        {
            Vector3[] vertices = new Vector3[8];
            vertices = basicEntity.BoundingBox.GetCorners();

            //Transform
            for (var index = 0; index < vertices.Length; index++)
            {
                vertices[index] = Vector3.Transform(vertices[index], basicEntity.WorldTransform.World);
            }

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
