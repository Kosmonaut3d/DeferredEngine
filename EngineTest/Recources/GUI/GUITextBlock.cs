using System;
using System.Text;
using EngineTest.Renderer.RenderModules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources.GUI
{
    /// <summary>
    /// Just a colored block with a text inside
    /// </summary>
    public class GUITextBlock : GUIElement
    {
        public Vector2 Position;
        public Vector2 Dimensions;
        public Color Color;
        public StringBuilder Text;
        public Color TextColor;
        public SpriteFont TextFont;

        private Vector2 _fontPosition;

        public GUITextBlock(Vector2 position, Vector2 dimensions, String text, SpriteFont font, Color blockColor, Color textColor, int layer = 0, GUICanvas.GUIAlignment alignment = GUICanvas.GUIAlignment.None, Vector2 ParentDimensions = default(Vector2))
        {
            Position = position;
            Dimensions = dimensions;
            Color = blockColor;
            Layer = layer;
            Text = new StringBuilder(text);
            TextColor = textColor;
            TextFont = font;
            ComputeFontPosition();
            Alignment = alignment;
            if (Alignment != GUICanvas.GUIAlignment.None)
            {
                ParentResized(ParentDimensions);
            }
        }

        private void ComputeFontPosition()
        {
            Vector2 fontDim = TextFont.MeasureString(Text);
            _fontPosition = Dimensions/2 - fontDim/2;
        }

        public void Draw(GUIRenderer guiRenderer, Vector2 parentPosition)
        {
            guiRenderer.DrawQuad(parentPosition + Position, Dimensions, Color);
            guiRenderer.DrawText(parentPosition + Position + _fontPosition, Text, TextFont, TextColor);
        }

        public void ParentResized(Vector2 dimensions)
        {
            Position = GUICanvas.UpdateAlignment(Alignment, dimensions, Dimensions, Position);
        }

        public int Layer { get; }
        public GUICanvas.GUIAlignment Alignment { get; }
    }
}