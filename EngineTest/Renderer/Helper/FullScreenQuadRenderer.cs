
#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace ShaderPlayground.Helpers
{
    public class FullScreenQuadRenderer
    {
        //buffers for rendering the quad
        private readonly IndexBuffer _indexBuffer;
        private readonly VertexBuffer _vertexBuffer;

        public FullScreenQuadRenderer(GraphicsDevice graphics)
        {
            FullScreenQuadVertex[] vertexBufferTemp = new FullScreenQuadVertex[4];
            vertexBufferTemp[0] = new FullScreenQuadVertex(new Vector2(-1, 1) );
            vertexBufferTemp[1] = new FullScreenQuadVertex(new Vector2(1, 1) );
            vertexBufferTemp[2] = new FullScreenQuadVertex(new Vector2(-1, -1) );
            vertexBufferTemp[3] = new FullScreenQuadVertex(new Vector2(1, -1));
            short[] indexBufferTemp = new short[] { 0, 3, 2, 0, 1, 3 };

            _vertexBuffer = new VertexBuffer(graphics, FullScreenQuadVertex.VertexDeclaration, 4, BufferUsage.WriteOnly);
            _indexBuffer = new IndexBuffer(graphics, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);

            _vertexBuffer.SetData(vertexBufferTemp);
            _indexBuffer.SetData(indexBufferTemp);
        }

        /// <summary>
        /* 
        //FULLSCREENQUAD
        //Pass a v4 position and v2 texcoord from only v2 input
        VertexShaderFSQOutput VertexShaderFSQFunction(VertexShaderFSQInput input)
        {
            VertexShaderOutput output;

            output.Position = float4(input.Position.xy, 1, 1);
            output.TexCoord = input.Position.xy * 0.5f + 0.5f;
            output.TexCoord.y = 1 - output.TexCoord.y;

            return output;
        }
        */
        /// </summary>
        /// <param name="graphicsDevice"></param>
        public void RenderFullscreenQuad(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Indices = _indexBuffer;
            graphicsDevice.SetVertexBuffer(_vertexBuffer);
            graphicsDevice.DrawIndexedPrimitives
                (PrimitiveType.TriangleList, 0,0,4);
        }

        public struct FullScreenQuadVertex
        {
            // Stores the starting position of the particle.
            public Vector2 Position;

            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector2,
                    VertexElementUsage.Position, 0)
            );

            public FullScreenQuadVertex(Vector2 position)
            {
                Position = position;
            }

            public const int SizeInBytes = 8;
        }
    }
}
