using System.Collections.Generic;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.Helper
{
    public class OctahedronHelperManager
    {
        private OctahedronMesh _octahedronMesh;
        private List<Vector3> positions = new List<Vector3>();
        private List<Vector4> colors = new List<Vector4>();
        private Matrix scale = Matrix.CreateScale(.005f);

        public void AddOctahedron(Vector3 position, Vector4 color)
        {
            positions.Add(position);
            colors.Add(color);
        }

        public void Draw(GraphicsDevice graphics, Matrix viewProjection, EffectParameter worldViewProjection, EffectParameter globalColor, EffectPass globalColorPass)
        {
            if(_octahedronMesh ==  null) _octahedronMesh = new OctahedronMesh(graphics);
            
            graphics.SetVertexBuffer(_octahedronMesh.GetVertexBuffer());
            graphics.Indices = _octahedronMesh.GetIndexBuffer();

            for (var index = 0; index < positions.Count; index++)
            {
                
                Matrix wvp = scale * Matrix.CreateTranslation(positions[index]) * viewProjection;

                worldViewProjection.SetValue(wvp);
                globalColor.SetValue(colors[index]);

                globalColorPass.Apply();

                graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 8);
            }

            //Clear

            positions.Clear();
            colors.Clear();
        }

    }
}
