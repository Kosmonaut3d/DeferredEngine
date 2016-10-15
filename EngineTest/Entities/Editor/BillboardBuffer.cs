using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Entities.Editor
{
    public class BillboardBuffer
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

            iBufferArray[ 0] = (ushort) ( 0);
            iBufferArray[ 1] = (ushort) ( 1);
            iBufferArray[ 2] = (ushort) ( 2);
            iBufferArray[ 3] = (ushort) ( 2);
            iBufferArray[ 4] = (ushort) ( 3);
            iBufferArray[ 5] = (ushort) ( 0);

            VBuffer.SetData(vBufferArray);
            IBuffer.SetData(iBufferArray);
        }
    }
}
