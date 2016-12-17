using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.Constraints.TwoEntity.Motors;
using EngineTest.Recources;
using EngineTest.Recources.GUI;
using EngineTest.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Renderer.RenderModules
{
    public class GUIRenderer
    {
        private GraphicsDevice _graphicsDevice;
        private QuadRenderer _quadRenderer;
        private SpriteBatch _spriteBatch;

        public Vector2 Resolution;

        private Effect _guiEffect;
        private EffectParameter _guiEffectParameters_Color;

        private Color _guiEffectColor;
        private EffectPass _guiEffectPass_Flat;

        public Color GuiEffectColor
        {
            get
            {
                return _guiEffectColor;
            }

            set
            {
                if (_guiEffectColor != value)
                {
                    _guiEffectColor = value;
                    _guiEffectParameters_Color.SetValue(value.ToVector3());
                }
            }
        }

        public void Initialize(GraphicsDevice graphicsDevice, QuadRenderer quadRenderer = null)
        {
            _graphicsDevice = graphicsDevice;
            _quadRenderer = quadRenderer ?? new QuadRenderer();
            _spriteBatch = new SpriteBatch(graphicsDevice);

            Resolution = new Vector2(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight);
        }

        public void Load(ContentManager content)
        {
            _guiEffect = content.Load<Effect>("Shaders/Graphical User Interface/GUIEffect");
            _guiEffectPass_Flat = _guiEffect.Techniques["Flat"].Passes[0];

            _guiEffectParameters_Color = _guiEffect.Parameters["Color"];

            GuiEffectColor = Color.White;
        }
        
        public void Draw(GUICanvas canvas)
        {
            if (!GameSettings.ui_DrawUI) return;

            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;

            //_guiEffectPass_Flat.Apply();
            _spriteBatch.Begin();
            canvas.Draw(this, Vector2.Zero);
            _spriteBatch.End();
        }

        public void DrawQuad(Vector2 pos, Vector2 dim, Color color)
        {
            //Vector2 v1, v2;
            //GuiEffectColor = color;
            //CalculateCoordinates(pos.X, pos.Y, dim.X, dim.Y, Resolution, out v1, out v2);

            //_guiEffectPass_Flat.Apply();
            //_quadRenderer.RenderQuad(_graphicsDevice, v1, v2);
            _spriteBatch.Draw(Assets.BaseTex, RectangleFromVectors(pos, dim), color);
            
        }

        private Rectangle RectangleFromVectors(Vector2 pos, Vector2 dim)
        {
            return new Rectangle((int) pos.X, (int) pos.Y, (int) dim.X, (int) dim.Y);
        }

        public void DrawText(Vector2 position, StringBuilder text, SpriteFont textFont, Color textColor)
        {
            _spriteBatch.DrawString(textFont, text, position, textColor);
        }

        public void CalculateCoordinates(float x, float y, float w, float h, Vector2 resolution, out Vector2 v1, out Vector2 v2)
        {
            v1 = new Vector2(x, y) / resolution;
            v2 = new Vector2(x + w, y + h) / resolution;

            //Transform into VPS
            v1 = v1 * 2 - Vector2.One;
            v1.Y = -v1.Y;

            v2 = v2 * 2 - Vector2.One;
            v2.Y = -v2.Y;
        }

    }
}
