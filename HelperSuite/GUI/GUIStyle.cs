using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HelperSuite.GUI
{
    public class GUIStyle
    {
        public Vector2 DimensionsStyle;
        public Color BlockColorStyle;
        public Color TextColorStyle;
        public Color SliderColorStyle;
        public SpriteFont TextFontStyle;
        public GUIAlignment GuiAlignmentStyle;
        public TextAlignment TextAlignmentStyle;
        public TextAlignment TextButtonAlignmentStyle;
        public Vector2 ParentDimensionsStyle;
        public Vector2 TextBorderStyle;

        public GUIStyle(Vector2 dimensionsStyle, SpriteFont textFontStyle, Color blockColorStyle, Color textColorStyle, Color sliderColorStyle, GUIAlignment guiAlignmentStyle, TextAlignment textAlignmentStyle, TextAlignment textButtonAlignmentStyle, Vector2 textBorderStyle, Vector2 parentDimensionsStyle)
        {
            DimensionsStyle = dimensionsStyle;
            TextFontStyle = textFontStyle;
            BlockColorStyle = blockColorStyle;
            TextColorStyle = textColorStyle;
            GuiAlignmentStyle = guiAlignmentStyle;
            TextAlignmentStyle = textAlignmentStyle;
            ParentDimensionsStyle = parentDimensionsStyle;
            TextButtonAlignmentStyle = textButtonAlignmentStyle;
            TextBorderStyle = textBorderStyle;
            SliderColorStyle = sliderColorStyle;
        }

        public enum GUIAlignment
        {
            None,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Center
        }

        public enum TextAlignment
        {
            Left, Center, Right
        }


    }
}
