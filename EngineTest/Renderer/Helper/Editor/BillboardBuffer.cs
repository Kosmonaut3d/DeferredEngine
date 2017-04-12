using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.Helper.Editor
{
    public class BillboardBuffer : IDisposable
    {
        public readonly VertexBuffer VBuffer;
        public readonly IndexBuffer IBuffer;

        public BillboardBuffer(Color color, GraphicsDevice graphics)
        {
            VBuffer = new VertexBuffer(graphics, VertexPositionColorTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
            IBuffer = new IndexBuffer(graphics, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);

            var vBufferArray = new VertexPositionColorTexture[4];
            var iBufferArray = new ushort[6];

            vBufferArray[0].Position = Vector3.Zero;
            vBufferArray[0].TextureCoordinate = new Vector2(0, 0);
            vBufferArray[0].Color = color;

            vBufferArray[1].Position = Vector3.Zero;
            vBufferArray[ 1].TextureCoordinate = new Vector2(0, 1);
            vBufferArray[1].Color = color;

            vBufferArray[2].Position = Vector3.Zero;
            vBufferArray[ 2].TextureCoordinate = new Vector2(1, 1);
            vBufferArray[2].Color = color;

            vBufferArray[3].Position = Vector3.Zero;
            vBufferArray[ 3].TextureCoordinate = new Vector2(1, 0);
            vBufferArray[3].Color = color;

            iBufferArray[ 0] = 0;
            iBufferArray[ 1] = 1;
            iBufferArray[ 2] = 2;
            iBufferArray[ 3] = 2;
            iBufferArray[ 4] = 3;
            iBufferArray[ 5] = 0;

            VBuffer.SetData(vBufferArray);
            IBuffer.SetData(iBufferArray);
        }

        public void Dispose()
        {
            VBuffer?.Dispose();
            IBuffer?.Dispose();
        }
    }
}
