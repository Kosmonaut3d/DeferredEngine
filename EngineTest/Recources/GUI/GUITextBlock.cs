using System;
using System.Text;
using DeferredEngine.Renderer.RenderModules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources.GUI
{
    /// <summary>
    /// Just a colored block with a text inside
    /// </summary>
    public class GUITextBlock : GUIBlock
    {
        public Color TextColor;
        public SpriteFont TextFont;

        public override Vector2 Dimensions
        {
            get { return _dimensions; }
            set
            {
                _dimensions = value;
                ComputeFontPosition();
            }
        }

        public StringBuilder Text
        {
            get { return _text; }
            set
            {
                _text = value; 
                ComputeFontPosition();
            }
        }
        private StringBuilder _text;

        private Vector2 _fontPosition;
        private Vector2 _dimensions;

        public GUITextBlock(Vector2 position, Vector2 dimensions, String text, SpriteFont font, Color blockColor, Color textColor, int layer = 0, GUICanvas.GUIAlignment alignment = GUICanvas.GUIAlignment.None, Vector2 parentDimensions = default(Vector2)) : base(position,dimensions, blockColor, layer, alignment, parentDimensions)
        {
            _text = new StringBuilder(text);
            TextColor = textColor;
            TextFont = font;
            ComputeFontPosition();
        }

        protected virtual void ComputeFontPosition()
        {
            if (_text == null) return;
            Vector2 fontDim = TextFont.MeasureString(_text);
            _fontPosition = Dimensions/2 - fontDim/2;
        }

        public override void Draw(GUIRenderer guiRenderer, Vector2 parentPosition)
        {
            guiRenderer.DrawQuad(parentPosition + Position, Dimensions, Color);
            guiRenderer.DrawText(parentPosition + Position + _fontPosition, _text, TextFont, TextColor);
        }
        
    }
}