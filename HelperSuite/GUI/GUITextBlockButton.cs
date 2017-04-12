using System;
using System.Reflection;
using HelperSuite.GUIHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HelperSuite.GUI
{
    public class GUITextBlockButton : GUITextBlock
    {
        public bool Toggle;
        
        private static readonly float ButtonBorder = 2;

        private static readonly Color HoverColor = Color.Tomato;
        
        private bool _isHovered;
        
        public MethodInfo ButtonMethod;
        public object[] ButtonMethodArgs = null;
        public Object ButtonObject;

        public GUITextBlockButton(GUIStyle guitStyle, String text) : this(
            position: Vector2.Zero,
            dimensions: guitStyle.DimensionsStyle,
            text: text,
            font: guitStyle.TextFontStyle,
            blockColor: guitStyle.BlockColorStyle,
            textColor: guitStyle.TextColorStyle,
            textAlignment: guitStyle.TextButtonAlignmentStyle,
            textBorder: guitStyle.TextBorderStyle,
            layer: 0,
            alignment: guitStyle.GuiAlignmentStyle,
            parentDimensions: guitStyle.ParentDimensionsStyle)
        { }

        public GUITextBlockButton(Vector2 position, Vector2 dimensions, String text, SpriteFont font, Color blockColor, Color textColor, GUIStyle.TextAlignment textAlignment = GUIStyle.TextAlignment.Center, Vector2 textBorder = default(Vector2), int layer = 0, GUIStyle.GUIAlignment alignment = GUIStyle.GUIAlignment.None, Vector2 parentDimensions = default(Vector2)) : base(position, dimensions, text, font, blockColor, textColor, textAlignment, textBorder, layer)
        {

        }
        public void SetButtonMethod(Object obj, string method, object[] args = null)
        {
            ButtonObject = obj;
            ButtonObject.GetType().GetMethod(method);
            ButtonMethodArgs = args;
        }

        public override void Draw(GUIRenderer.GUIRenderer guiRenderer, Vector2 parentPosition, Vector2 mousePosition)
        {
            guiRenderer.DrawQuad(parentPosition + Position, Dimensions, Color.DimGray);
            guiRenderer.DrawQuad(parentPosition + Position + Vector2.One * ButtonBorder, Dimensions - 2*Vector2.One*ButtonBorder, _isHovered ? HoverColor : BlockColor);
            guiRenderer.DrawText(parentPosition + Position + _fontPosition, Text, TextFont, TextColor);
        }

        public override void Update(GameTime gameTime, Vector2 mousePosition, Vector2 parentPosition)
        {
            _isHovered = false;

            Vector2 bound1 = Position + parentPosition;
            Vector2 bound2 = bound1 + Dimensions;

            if (mousePosition.X >= bound1.X && mousePosition.Y >= bound1.Y && mousePosition.X < bound2.X &&
                mousePosition.Y < bound2.Y)
            {
                _isHovered = true;

                if (!GUIControl.WasLMBClicked()) return;

                GUIControl.UIWasUsed = true;

                if (ButtonObject != null)
                {
                    if (ButtonMethod != null) ButtonMethod.Invoke(ButtonObject, ButtonMethodArgs);
                }
            }
        }

    }
    
}