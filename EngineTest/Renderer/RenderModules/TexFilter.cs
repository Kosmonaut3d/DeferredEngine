using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using BEPUutilities;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace DeferredEngine.Renderer.RenderModules
{
    public class TexFilter
    {
        public Effect texFilterEffect;
        public EffectParameter Texture;

        public Texture2D tex;

        private QuadRenderer _quadRenderer;

        private GraphicsDevice _graphics;
        private SpriteBatch _spriteBatch;

        public void Load(ContentManager content)
        {
            tex = content.Load<Texture2D>("Art/Editor/texStrip");

            texFilterEffect = content.Load<Effect>("Shaders/Test/texFilter");
            Texture = texFilterEffect.Parameters["ScreenTexture"];

            Texture.SetValue(tex);
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _quadRenderer = new QuadRenderer();
            _graphics = graphicsDevice;
            _spriteBatch = new SpriteBatch(_graphics);
        }

        public void Draw(Matrix _viewProjection, Model isosphere, Texture2D tex)
        {
            //_spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, texFilterEffect, null);
            //_spriteBatch.Draw(tex, new Rectangle(0,0,256, 1536), Color.White);
            //_spriteBatch.End();
            ModelMeshPart meshpart = isosphere.Meshes[0].MeshParts[0];

            texFilterEffect.Parameters["WorldViewProj"].SetValue(Matrix.CreateScale(10) * _viewProjection);
            Texture.SetValue(tex);

            _graphics.SetRenderTarget(null);
            _graphics.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphics.BlendState = BlendState.Opaque;

            _graphics.SetVertexBuffer(meshpart.VertexBuffer);
            _graphics.Indices = (meshpart.IndexBuffer);
            int primitiveCount = meshpart.PrimitiveCount;
            int vertexOffset = meshpart.VertexOffset;
            int startIndex = meshpart.StartIndex;

            Shaders.EmissiveEffect.CurrentTechnique.Passes[0].Apply();

            texFilterEffect.CurrentTechnique.Passes[0].Apply();

            _graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset,
                startIndex, primitiveCount);

        }

    }
}
