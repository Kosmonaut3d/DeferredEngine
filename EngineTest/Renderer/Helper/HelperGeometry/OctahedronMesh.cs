using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.Helper
{
    class OctahedronMesh
    {
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;

        public OctahedronMesh(GraphicsDevice graphics)
        {
            VertexPosition[] vertexBufferTemp = new VertexPosition[6];

            vertexBufferTemp[0] = new VertexPosition(new Vector3(0, 0, -1));
            vertexBufferTemp[1] = new VertexPosition(new Vector3(0, -1, 0));
            vertexBufferTemp[2] = new VertexPosition(new Vector3(1, 0, 0));
            vertexBufferTemp[3] = new VertexPosition(new Vector3(0, 1, 0));
            vertexBufferTemp[4] = new VertexPosition(new Vector3(-1, 0, 0));
            vertexBufferTemp[5] = new VertexPosition(new Vector3(0, 0, 1));

            short[] indexBufferTemp = new short[] { 2, 0, 1, 1 , 0, 4, 4, 0, 3, 3, 0 , 2, 5 , 2, 1, 5, 1, 4, 5, 4, 3, 5, 3,2 };

            _vertexBuffer = new VertexBuffer(graphics, VertexPosition.VertexDeclaration, 6, BufferUsage.WriteOnly);
            _indexBuffer = new IndexBuffer(graphics, IndexElementSize.SixteenBits, 24, BufferUsage.WriteOnly);

            _vertexBuffer.SetData(vertexBufferTemp);
            _indexBuffer.SetData(indexBufferTemp);
        }

        public VertexBuffer GetVertexBuffer()
        {
            return _vertexBuffer;
        }

        public IndexBuffer GetIndexBuffer()
        {
            return _indexBuffer;
        }
    }
}
