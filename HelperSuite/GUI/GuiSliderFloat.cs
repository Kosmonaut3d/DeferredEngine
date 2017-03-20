using System;
using System.Reflection;
using HelperSuite.GUIHelper;
using Microsoft.Xna.Framework;

namespace HelperSuite.GUI
{
    /// <summary>
    /// A slider that can reference float values
    /// </summary>
    public class GuiSliderFloat : GUIBlock
    {
        protected bool IsEngaged = false;

        protected const float SliderIndicatorSize = 15;
        protected const float SliderIndicatorBorder = 10;
        protected const float SliderBaseHeight = 5;

        private Vector2 SliderPosition;

        protected float _sliderPercent;

        private float _sliderValue;
        public float SliderValue
        {
            get { return _sliderValue; }
            set
            {
                _sliderValue = value;
                _sliderPercent = (_sliderValue - MinValue)/(MaxValue - MinValue);
            }
        }

        public float MaxValue = 1; 
        public float MinValue;

        protected Color _sliderColor;

        public PropertyInfo SliderProperty;
        public FieldInfo SliderField;
        public Object SliderObject;

        public GuiSliderFloat(GUIStyle guiStyle, float min, float max) : this(
            position: Vector2.Zero, 
            dimensions: new Vector2(guiStyle.DimensionsStyle.X, 35),
            min: min,
            max: max,
            blockColor: guiStyle.BlockColorStyle,
            sliderColor: guiStyle.SliderColorStyle,
            layer: 0,
            alignment: guiStyle.GuiAlignmentStyle,
            ParentDimensions: guiStyle.ParentDimensionsStyle
            )
        { }

        public GuiSliderFloat(Vector2 position, Vector2 dimensions, float min, float max, Color blockColor, Color sliderColor, int layer = 0, GUIStyle.GUIAlignment alignment = GUIStyle.GUIAlignment.None, Vector2 ParentDimensions = new Vector2()) : base(position, dimensions, blockColor, layer, alignment, ParentDimensions)
        {
            _sliderColor = sliderColor;
            MinValue = min;
            MaxValue = max;
            _sliderValue = min;
        }

        public void SetField(Object obj, string field)
        {
            SliderObject = obj;
            SliderField = obj.GetType().GetField(field);
            SliderValue = (float) SliderField.GetValue(obj);
        }

        public void SetProperty(Object obj, string property)
        {
            SliderObject = obj;
            SliderProperty = obj.GetType().GetProperty(property);
            SliderValue = (float)SliderProperty.GetValue(obj);
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

            Vector2 bound1 = Position + parentPosition /*+ SliderIndicatorBorder*Vector2.UnitX*/;
            Vector2 bound2 = bound1 + Dimensions/* - 2*SliderIndicatorBorder * Vector2.UnitX*/;

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

        public override void Draw(GUIRenderer.GUIRenderer guiRenderer, Vector2 parentPosition, Vector2 mousePosition)
        {
            guiRenderer.DrawQuad(parentPosition + Position, Dimensions, BlockColor);
            
            Vector2 slideDimensions = new Vector2(Dimensions.X - SliderIndicatorBorder*2, SliderBaseHeight);
            guiRenderer.DrawQuad(parentPosition + Position + new Vector2(SliderIndicatorBorder, 
                Dimensions.Y* 0.5f - SliderBaseHeight * 0.5f), slideDimensions, Color.DarkGray);

            //slideDimensions = new Vector2(slideDimensions.X + SliderIndicatorSize* 0.5f, slideDimensions.Y);
            guiRenderer.DrawQuad(parentPosition + Position + new Vector2(SliderIndicatorBorder - SliderIndicatorSize* 0.5f,
                 Dimensions.Y * 0.5f - SliderIndicatorSize * 0.5f) + _sliderPercent*slideDimensions * Vector2.UnitX, new Vector2(SliderIndicatorSize, SliderIndicatorSize), _sliderColor);
        }
    }
}