using System;
using System.Text;
using HelperSuite.GUIHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HelperSuite.GUI
{
    public class GuiDropList : GUITextBlock
    {
        public bool Toggle;
        
        private static readonly float ButtonBorder = 2;

        private static readonly Color HoverColor = Color.LightGray;

        private static readonly int HoverImageWidth = 250;

        private Vector2 _declarationTextDimensions;
        
        private bool _isHovered;

        private bool _isToggled = false;

        private Vector2 _baseDimensions;

        //Load
        private StringBuilder _selectedOptionName = new StringBuilder(100);
        
        public GuiDropList(GUIStyle style, string text) : this(
            position: Vector2.Zero,
            dimensions: style.DimensionsStyle,
            text: text,
            font: style.TextFontStyle,
            blockColor: style.BlockColorStyle,
            textColor: style.TextColorStyle,
            textAlignment: GUIStyle.TextAlignment.Left,
            textBorder: style.TextBorderStyle,
            layer: 0,
            alignment: style.GuiAlignmentStyle,
            parentDimensions: style.ParentDimensionsStyle
            )
        {
        }

        public GuiDropList(Vector2 position, Vector2 dimensions, string text, SpriteFont font, Color blockColor, Color textColor, GUIStyle.TextAlignment textAlignment = GUIStyle.TextAlignment.Center, Vector2 textBorder = default(Vector2), int layer = 0, GUIStyle.GUIAlignment alignment = GUIStyle.GUIAlignment.None, Vector2 parentDimensions = default(Vector2)) : base(position, dimensions, text, font, blockColor, textColor, textAlignment, textBorder, layer)
        {
            _selectedOptionName.Append("...");

            _baseDimensions = Dimensions;

            throw new NotImplementedException();
        }

        protected override void ComputeFontPosition()
        {
            if (_text == null) return;
            _declarationTextDimensions = TextFont.MeasureString(_text);

            //Let's check wrap!

            //FontWrap(ref textDimension, Dimensions);
            
            _fontPosition = Dimensions * 0.5f * Vector2.UnitY + _textBorder * Vector2.UnitX - _declarationTextDimensions * 0.5f * Vector2.UnitY;
        }

        protected void ComputeObjectNameLength()
        {
            if (_selectedOptionName.Length > 0)
            {
                //Max length
                Vector2 textDimensions = TextFont.MeasureString(_selectedOptionName);

                float characterLength = textDimensions.X/_selectedOptionName.Length;

                Vector2 buttonLeft = (_declarationTextDimensions + _fontPosition * 1.5f) * Vector2.UnitX;
                Vector2 spaceAvailable = Dimensions - 2*Vector2.One*ButtonBorder - buttonLeft -
                                         (2 + _textBorder.X)*Vector2.UnitX;

                int characters = (int) (spaceAvailable.X/characterLength);

                _selectedOptionName.Length = characters < _selectedOptionName.Length ? characters : _selectedOptionName.Length;
            }
        }


        public override void Draw(GUIRenderer.GUIRenderer guiRenderer, Vector2 parentPosition, Vector2 mousePosition)
        {
            Vector2 buttonLeft = (_declarationTextDimensions + _fontPosition * 1.2f)*Vector2.UnitX;
            guiRenderer.DrawQuad(parentPosition + Position, Dimensions, BlockColor);
            guiRenderer.DrawQuad(parentPosition + Position + buttonLeft + Vector2.One * ButtonBorder, Dimensions - 2*Vector2.One*ButtonBorder - buttonLeft - (2+_textBorder.X)*Vector2.UnitX, _isHovered ? HoverColor : Color.DimGray);
            
            guiRenderer.DrawText(parentPosition + Position + _fontPosition, Text, TextFont, TextColor);

            //Description
            guiRenderer.DrawText(parentPosition + Position + buttonLeft + new Vector2(4, _fontPosition.Y), _selectedOptionName, TextFont, TextColor);
            
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

                _isToggled = !_isToggled;
                Dimensions = new Vector2(_baseDimensions.X, _baseDimensions.Y + (_isToggled ? 100 : 0));

                GUIControl.UIWasUsed = true;
            }
        }

    }
    
}