using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    public class DecalRenderModule : IDisposable
    {
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBufferCage;
        private IndexBuffer _indexBufferCube;

        private Effect _decalShader;

        private EffectParameter _paramDecalMap;
        private EffectParameter _paramWorldView;
        private EffectParameter _paramWorldViewProj;
        private EffectParameter _paramInverseWorldView;
        private EffectParameter _paramDepthMap;
        private EffectParameter _paramFarClip;

        private EffectPass _decalPass;
        private EffectPass _outlinePass;

        private BlendState _decalBlend;
        private int _shaderIndex;
        private ShaderManager _shaderManagerReference;

        public float FarClip { set {_paramFarClip.SetValue(value);} }
        public Texture2D DepthMap { set { _paramDepthMap.SetValue(value); } }

        public DecalRenderModule(ShaderManager shaderManager, string shaderPath)
        {
            
            Load(shaderManager, shaderPath);
            InitializeShader();
        }

        public void Load(ShaderManager shaderManager, string shaderPath)
        {
            _shaderIndex = shaderManager.AddShader(shaderPath);

            _decalShader = shaderManager.GetShader(_shaderIndex);

            _shaderManagerReference = shaderManager;

        }

        private void InitializeShader()
        {
            _paramDecalMap = _decalShader.Parameters["DecalMap"];
            _paramWorldView = _decalShader.Parameters["WorldView"];
            _paramWorldViewProj = _decalShader.Parameters["WorldViewProj"];
            _paramInverseWorldView = _decalShader.Parameters["InverseWorldView"];
            _paramDepthMap = _decalShader.Parameters["DepthMap"];
            _paramFarClip = _decalShader.Parameters["FarClip"];

            _decalPass = _decalShader.Techniques["Decal"].Passes[0];
            _outlinePass = _decalShader.Techniques["Outline"].Passes[0];
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _decalBlend = new BlendState()
            {
                AlphaDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.Zero,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                ColorSourceBlend = Blend.SourceAlpha,
            };

            VertexPositionColor[] verts = new VertexPositionColor[8];

            Vector3 a = -Vector3.One;
            Vector3 b = Vector3.One;
            Color color = Color.White;
            Color colorUpper = new Color(0,255,0,255);
            verts[0] = new VertexPositionColor(new Vector3(a.X, a.Y, a.Z), color);
            verts[1] = new VertexPositionColor(new Vector3(b.X, a.Y, a.Z), color);
            verts[2] = new VertexPositionColor(new Vector3(a.X, b.Y, a.Z), color);
            verts[3] = new VertexPositionColor(new Vector3(b.X, b.Y, a.Z), color);
            verts[4] = new VertexPositionColor(new Vector3(a.X, a.Y, b.Z), colorUpper);
            verts[5] = new VertexPositionColor(new Vector3(b.X, a.Y, b.Z), colorUpper);
            verts[6] = new VertexPositionColor(new Vector3(a.X, b.Y, b.Z), colorUpper);
            verts[7] = new VertexPositionColor(new Vector3(b.X, b.Y, b.Z), colorUpper);

            short[] Indices = new short[24];

            Indices[0] = 0;
            Indices[1] = 1;
            Indices[2] = 1;
            Indices[3] = 3;
            Indices[4] = 3;
            Indices[5] = 2;
            Indices[6] = 2;
            Indices[7] = 0;

            Indices[8] = 4;
            Indices[9] = 5;
            Indices[10] = 5;
            Indices[11] = 7;
            Indices[12] = 7;
            Indices[13] = 6;
            Indices[14] = 6;
            Indices[15] = 4;

            Indices[16] = 0;
            Indices[17] = 4;
            Indices[18] = 1;
            Indices[19] = 5;
            Indices[20] = 2;
            Indices[21] = 6;
            Indices[22] = 3;
            Indices[23] = 7;

            //short[] Indices2 = new short[36];
            short[] Indices2 = new short[] {0,4,1,
                1,4,5,
                1,5,3,
                3,5,7,
                2,3,7,
                7,6,2,
                2,6,0,
                0,6,4,
                5,4,7,
                7,4,6,
                2,0,1,
                1,3,2 };

            _vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionColor.VertexDeclaration, 8, BufferUsage.WriteOnly);
            _indexBufferCage = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, 24, BufferUsage.WriteOnly);
            _indexBufferCube = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, 36, BufferUsage.WriteOnly);

            _vertexBuffer.SetData(verts);
            _indexBufferCage.SetData(Indices);
            _indexBufferCube.SetData(Indices2);
        }

        public void Draw(GraphicsDevice graphics, List<Decal> decals, Matrix view, Matrix viewProjection, Matrix inverseView)
        {
            CheckForShaderChanges();

            graphics.SetVertexBuffer(_vertexBuffer);
            graphics.Indices = _indexBufferCube;
            graphics.RasterizerState = RasterizerState.CullClockwise;
            graphics.BlendState = _decalBlend;

            for (int index = 0; index < decals.Count; index++)
            {
                Decal decal = decals[index];

                Matrix localMatrix = decal.World;

                _paramDecalMap.SetValue(decal.Texture);
                _paramWorldView.SetValue(localMatrix*view);
                _paramWorldViewProj.SetValue( localMatrix*viewProjection);
                _paramInverseWorldView.SetValue(inverseView * decal.InverseWorld);
                
                _decalPass.Apply();

                graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12);
            }
        }

        public void DrawOutlines(GraphicsDevice graphics, Decal decal, Matrix viewProjection, Matrix view)
        {
            graphics.SetVertexBuffer(_vertexBuffer);
            graphics.Indices = _indexBufferCage;
            
            Matrix localMatrix = decal.World;

            _paramWorldView.SetValue(localMatrix * view);
            _paramWorldViewProj.SetValue(localMatrix * viewProjection);
            
            _outlinePass.Apply();

            graphics.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 12);
            
        }


        private void CheckForShaderChanges()
        {
            if (_shaderManagerReference.GetShaderHasChanged(_shaderIndex))
            {
                _decalShader = _shaderManagerReference.GetShader(_shaderIndex);
                InitializeShader();
            }
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBufferCage?.Dispose();
            _indexBufferCube?.Dispose();
            _decalShader?.Dispose();
            _decalBlend?.Dispose();
        }
    }
}
