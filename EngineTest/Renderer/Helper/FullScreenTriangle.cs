using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.Helper
{
    public class FullScreenTriangle
    {
        private VertexBuffer vertexBuffer;

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


        //No function in monogame to draw without vb it seems...

        /*
         struct VertexShaderInput
        {
            float2 Position : POSITION0;
        };

        VertexShaderOutput VertexShaderFunction(VertexShaderInput input, uint id:SV_VERTEXID)
        {
        VertexShaderOutput output;
        output.Position = float4(input.Position, 0, 1);
        output.TexCoord.x = (float)(id / 2) * 2.0;
        output.TexCoord.y = 1.0 - (float)(id % 2) * 2.0;

        return output;
        }

        */

        public FullScreenTriangle(GraphicsDevice graphics)
    {
        FullScreenQuadVertex[] vertices = new FullScreenQuadVertex[3];
        vertices[0] = new FullScreenQuadVertex(new Vector2(-1, -1));
        vertices[1] = new FullScreenQuadVertex(new Vector2(-1, 3));
        vertices[2] = new FullScreenQuadVertex(new Vector2(3, -1));

        vertexBuffer = new VertexBuffer(graphics, FullScreenQuadVertex.VertexDeclaration, 3, BufferUsage.WriteOnly);
        vertexBuffer.SetData(vertices);
    }

    public void Draw(GraphicsDevice graphics)
    {
        graphics.SetVertexBuffer(vertexBuffer);
        graphics.Indices = null;

        graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
    }

        public void Dispose()
        {
            vertexBuffer?.Dispose();
        }
    }
}
