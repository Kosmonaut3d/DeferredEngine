using System;
using System.Reflection;
using System.Text;
using HelperSuite.GUIHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HelperSuite.GUI
{
    public class GuiSliderFloatText : GUIBlock
    {
        protected bool IsEngaged = false;

        protected const float SliderIndicatorSize = 15;
        protected const float SliderIndicatorBorder = 10;
        protected const float SliderBaseHeight = 5;

        private Vector2 SliderPosition;
        private Vector2 _tempPosition = Vector2.One;

        protected Vector2 SliderDimensions;

        protected float _sliderPercent;

        private float _sliderValue;
        public float SliderValue
        {
            get { return _sliderValue; }
            set
            {
                _sliderValue = value;
                _sliderPercent = (_sliderValue - MinValue)/(MaxValue - MinValue);
                UpdateText();
            }
        }

        public float MaxValue = 1; 
        public float MinValue;

        protected Color _sliderColor;
        
        //TextBlock associated
        protected GUITextBlock _textBlock;
        private uint roundDecimals = 1;
        protected String baseText;

        //Associated reference
        public PropertyInfo SliderProperty;
        public FieldInfo SliderField;
        public Object SliderObject;

        public GuiSliderFloatText(GUIStyle guiStyle, float min, float max, uint decimals, String text) : this(
            position: Vector2.Zero, 
            sliderDimensions: new Vector2(guiStyle.DimensionsStyle.X, 35),
            textdimensions: new Vector2(guiStyle.DimensionsStyle.X, 20),
            min: min,
            max: max,
            decimals: decimals,
            text: text,
            font: guiStyle.TextFontStyle,
            textBorder: guiStyle.TextBorderStyle,
            textAlignment: GUIStyle.TextAlignment.Left,
            blockColor: guiStyle.BlockColorStyle,
            sliderColor: guiStyle.SliderColorStyle,
            layer: 0,
            alignment: guiStyle.GuiAlignmentStyle,
            ParentDimensions: guiStyle.ParentDimensionsStyle
            )
        { }

        public GuiSliderFloatText(Vector2 position, Vector2 sliderDimensions, Vector2 textdimensions, float min, float max, uint decimals, String text, SpriteFont font, Color blockColor, Color sliderColor, int layer = 0, GUIStyle.GUIAlignment alignment = GUIStyle.GUIAlignment.None, GUIStyle.TextAlignment textAlignment = GUIStyle.TextAlignment.Left, Vector2 textBorder = default(Vector2), Vector2 ParentDimensions = new Vector2()) : base(position, sliderDimensions, blockColor, layer, alignment, ParentDimensions)
        {
            _textBlock = new GUITextBlock(position, textdimensions, text, font, blockColor, sliderColor, textAlignment, textBorder, layer, alignment, ParentDimensions);

            Dimensions = sliderDimensions + _textBlock.Dimensions*Vector2.UnitY;
            SliderDimensions = sliderDimensions;
            _sliderColor = sliderColor;
            MinValue = min;
            MaxValue = max;
            _sliderValue = min;
            roundDecimals = decimals;
            baseText = text;

            UpdateText();
        }

        public void SetText(StringBuilder text)
        {
            baseText = text.ToString();
            _textBlock.Text = text;
            UpdateText();
        }

        public void SetField(Object obj, string field)
        {
            SliderObject = obj;
            SliderField = obj.GetType().GetField(field);
            SliderProperty = null;
            SliderValue = (float)SliderField.GetValue(obj);
        }

        public void SetProperty(Object obj, string property)
        {
            SliderObject = obj;
            SliderProperty = obj.GetType().GetProperty(property);
            SliderField = null;
            SliderValue = (float)SliderProperty.GetValue(obj);
        }

        public void SetValues(string text, float minValue, float maxValue, uint decimals)
        {
            SetText(new StringBuilder(text));
            MinValue = minValue;
            MaxValue = maxValue;
            roundDecimals = decimals;
        }

        public override void Update(GameTime gameTime, Vector2 mousePosition, Vector2 parentPosition)
        {
            if (GUIControl.UIElementEngaged && !IsEngaged) return;

            //Break Engagement
            if (IsEngaged && !GUIControl.IsLMBPressed())
            {
                GUIControl.UIElementEngaged = false;
                IsEngaged = false;
            }

            if (!GUIControl.IsLMBPressed()) return;

            Vector2 bound1 = Position + parentPosition + _textBlock.Dimensions * Vector2.UnitY /*+ SliderIndicatorBorder*Vector2.UnitX*/;
            Vector2 bound2 = bound1 + SliderDimensions/* - 2*SliderIndicatorBorder * Vector2.UnitX*/;

            if (mousePosition.X >= bound1.X && mousePosition.Y >= bound1.Y && mousePosition.X < bound2.X &&
                mousePosition.Y < bound2.Y + 1)
            {
                GUIControl.UIElementEngaged = true;
                IsEngaged = true;
            }

            if (IsEngaged)
            {
                GUIControl.UIWasUsed = true;

                float lowerx = bound1.X + SliderIndicatorBorder;
                float upperx = bound2.X - SliderIndicatorBorder;

                _sliderPercent = MathHelper.Clamp((mousePosition.X - lowerx)/(upperx - lowerx), 0, 1);

                _sliderValue = _sliderPercent*(MaxValue - MinValue) + MinValue;

                UpdateText();

                if (SliderObject != null)
                {
                    if (SliderField != null)
                        SliderField.SetValue(SliderObject, SliderValue, BindingFlags.Public, null, null);
                    else if (SliderProperty != null) SliderProperty.SetValue(SliderObject, SliderValue);
                }
                else
                {
                    if (SliderField != null)
                        SliderField.SetValue(null, SliderValue, BindingFlags.Static | BindingFlags.Public, null, null);
                    else if (SliderProperty != null) SliderProperty.SetValue(null, SliderValue);
                }
            }
        }

        private void UpdateText()
        {
            _textBlock.Text.Clear();
            _textBlock.Text.Append(baseText);
            _textBlock.Text.Concat(_sliderValue, roundDecimals);
        }

        public override void Draw(GUIRenderer.GUIRenderer guiRenderer, Vector2 parentPosition, Vector2 mousePosition)
        {
            _textBlock.Draw(guiRenderer, parentPosition, mousePosition);

            _tempPosition = parentPosition + Position + _textBlock.Dimensions*Vector2.UnitY;
            guiRenderer.DrawQuad(_tempPosition, SliderDimensions, BlockColor);
            
            Vector2 slideDimensions = new Vector2(SliderDimensions.X - SliderIndicatorBorder*2, SliderBaseHeight);
            guiRenderer.DrawQuad(_tempPosition + new Vector2(SliderIndicatorBorder,
                SliderDimensions.Y* 0.5f - SliderBaseHeight * 0.5f), slideDimensions, Color.DarkGray);

            //slideDimensions = new Vector2(slideDimensions.X + SliderIndicatorSize* 0.5f, slideDimensions.Y);
            guiRenderer.DrawQuad(_tempPosition + new Vector2(SliderIndicatorBorder - SliderIndicatorSize* 0.5f,
                 SliderDimensions.Y * 0.5f - SliderIndicatorSize * 0.5f) + _sliderPercent*slideDimensions * Vector2.UnitX, new Vector2(SliderIndicatorSize, SliderIndicatorSize), _sliderColor);
        }
    }
}