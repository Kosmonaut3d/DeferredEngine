using System.Text;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using HelperSuite.GUI;
using HelperSuite.GUIHelper;
using Microsoft.Xna.Framework;

namespace DeferredEngine.Main
{
    public class GUILogic
    {
        private Assets _assets;
        public GUICanvas GuiCanvas;

        private GuiListToggleScroll _rightSideList;

        private GUIList _objectDescriptionList;
        private GUITextBlock _objectDescriptionName;
        private GUITextBlock _objectDescriptionPos;
        private GUITextBlockButton _objectButton1;
        private GUITextBlockToggle _objectToggle1;
        private GUITextBlockToggle _objectToggle2;
        private GuiSliderFloatText _objectSlider1;
        private GuiSliderFloatText _objectSlider2;
        private GUIColorPicker _objectColorPicker1;

        private GUIStyle defaultStyle;

        //Selected object
        private TransformableObject activeObject;
        private string activeObjectName;
        private Vector3 activeObjectPos;


        public void Initialize(Assets assets)
        {
            _assets = assets;

            CreateGUI();
        }


        /// <summary>
        /// Creates the GUI for the default editor
        /// </summary>
        private void CreateGUI()
        {
            GuiCanvas = new GUICanvas(Vector2.Zero, new Vector2(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight));

            defaultStyle = new GUIStyle(
                dimensionsStyle: new Vector2(200,35),
                textFontStyle: _assets.MonospaceFont,
                blockColorStyle: Color.Gray, 
                textColorStyle: Color.White,
                sliderColorStyle: Color.White,
                guiAlignmentStyle: GUIStyle.GUIAlignment.None,
                textAlignmentStyle: GUIStyle.TextAlignment.Left,
                textButtonAlignmentStyle: GUIStyle.TextAlignment.Center,
                textBorderStyle: new Vector2(10, 1),
                parentDimensionsStyle: GuiCanvas.Dimensions);

            GuiCanvas.AddElement(_rightSideList = new GuiListToggleScroll(new Vector2(-20,0), defaultStyle));
            _rightSideList.Alignment = GUIStyle.GUIAlignment.TopRight;
            

            _objectDescriptionList = new GUIList(Vector2.Zero, defaultStyle);

            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Enable Editor")
            {
                ToggleField = typeof(GameStats).GetField("e_EnableSelection"),
                Toggle = GameStats.e_EnableSelection
            });

            _objectDescriptionList.AddElement(_objectDescriptionName = new GUITextBlock(defaultStyle, "objDescName"));
            _objectDescriptionList.AddElement(_objectDescriptionPos = new GUITextBlock(defaultStyle, "objDescName"));
            _objectDescriptionList.AddElement(_objectButton1 = new GUITextBlockButton(defaultStyle, "objButton1") {IsHidden = true});
            _objectDescriptionList.AddElement(_objectToggle1 = new GUITextBlockToggle(defaultStyle, "objToggle1") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectToggle2 = new GUITextBlockToggle(defaultStyle, "objToggle2") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectSlider1 = new GuiSliderFloatText(defaultStyle, 0,1,2,"objToggle1") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectSlider2 = new GuiSliderFloatText(defaultStyle, 0, 1, 2, "objToggle2") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectColorPicker1 = new GUIColorPicker(defaultStyle) { IsHidden = true });

            _rightSideList.AddElement(_objectDescriptionList);

            /////////////////////////////////////////////////////////////////
            //Options
            /////////////////////////////////////////////////////////////////
            
            _rightSideList.AddElement(new GUITextBlock(defaultStyle, "Options") {BlockColor = Color.DimGray, Dimensions = new Vector2(200,10), TextAlignment = GUIStyle.TextAlignment.Center});

            GuiListToggle optionList = new GuiListToggle(Vector2.Zero, defaultStyle);
            _rightSideList.AddElement(optionList);

                /////////////////////////////////////////////////////////////////
                //Post Processing
                /////////////////////////////////////////////////////////////////

            optionList.AddElement(new GUITextBlock(defaultStyle, "PostProcessing") { BlockColor = Color.DarkSlateGray, Dimensions = new Vector2(200, 10), TextAlignment = GUIStyle.TextAlignment.Center });

            GuiListToggle postprocessingList = new GuiListToggle(Vector2.Zero, defaultStyle) {ToggleBlockColor = Color.DarkSlateGray};
            optionList.AddElement(postprocessingList);

            postprocessingList.AddElement(new GUITextBlockToggle(defaultStyle, "Temporal AA")
            {
                ToggleField = typeof(GameSettings).GetField("g_TemporalAntiAliasing"),
                Toggle = GameSettings.g_TemporalAntiAliasing
            });

            postprocessingList.AddElement(new GUITextBlockToggle(defaultStyle, "Default Material")
            {
                ToggleField = typeof(GameSettings).GetField("d_defaultMaterial"),
                Toggle = GameSettings.d_defaultMaterial
            });

            postprocessingList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 4, 2, "WhitePoint: ")
            {
                SliderProperty = typeof(GameSettings).GetProperty("WhitePoint"),
                SliderValue = GameSettings.WhitePoint
            });

            postprocessingList.AddElement(new GuiSliderFloatText(defaultStyle, -8, 8, 2, "Exposure: ")
            {
                SliderProperty = typeof(GameSettings).GetProperty("Exposure"),
                SliderValue = GameSettings.Exposure
            });

            postprocessingList.AddElement(new GuiSliderFloatText(defaultStyle, -1, 1, 2, "S-Curve: ")
            {
                SliderProperty = typeof(GameSettings).GetProperty("SCurveStrength"),
                SliderValue = GameSettings.SCurveStrength
            });

            postprocessingList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 1, 2, "Chr. Abb.: ")
            {
                SliderProperty = typeof(GameSettings).GetProperty("ChromaticAbberationStrength"),
                SliderValue = GameSettings.ChromaticAbberationStrength
            });

                /////////////////////////////////////////////////////////////////
                //SSR
                /////////////////////////////////////////////////////////////////

            optionList.AddElement(new GUITextBlock(Vector2.Zero, new Vector2(200, 10), "Screen Space Reflections",
                defaultStyle.TextFontStyle, Color.DarkSlateGray, Color.White, GUIStyle.TextAlignment.Center,
                Vector2.Zero));

            GuiListToggle ssrList = new GuiListToggle(Vector2.Zero, defaultStyle) { ToggleBlockColor = Color.DarkSlateGray };
            optionList.AddElement(ssrList);

            ssrList.AddElement(new GUITextBlockToggle(defaultStyle, "Enable SSR")
            {
                ToggleProperty = typeof(GameSettings).GetProperty("g_SSReflection"),
                Toggle = GameSettings.g_SSReflection
            });

            ssrList.AddElement(new GUITextBlockToggle(defaultStyle, "Stochastic distr.")
            {
                ToggleProperty = typeof(GameSettings).GetProperty("g_SSReflectionTaa"),
                Toggle = GameSettings.g_SSReflectionTaa
            });

            ssrList.AddElement(new GUITextBlockToggle(defaultStyle, "Temporal Noise")
            {
                ToggleField = typeof(GameSettings).GetField("g_SSReflectionNoise"),
                Toggle = GameSettings.g_SSReflectionNoise
            });

            ssrList.AddElement(new GUITextBlockToggle(defaultStyle, "Firefly Reduction")
            {
                ToggleProperty = typeof(GameSettings).GetProperty("g_SSReflection_FireflyReduction"),
                Toggle = GameSettings.g_SSReflection_FireflyReduction
            });

            ssrList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 5, 2, "Firefly Threshold ")
            {
                SliderProperty = typeof(GameSettings).GetProperty("g_SSReflection_FireflyThreshold"),
                SliderValue = GameSettings.g_SSReflection_FireflyThreshold
            });

            ssrList.AddElement(new GuiSliderIntText(defaultStyle, 1, 100, 1, "Samples: ")
            {
                SliderProperty = typeof(GameSettings).GetProperty("g_SSReflections_Samples"),
                SliderValue = GameSettings.g_SSReflections_Samples
            });

            ssrList.AddElement(new GuiSliderIntText(defaultStyle, 1, 100, 1, "Search Samples: ")
            {
                SliderProperty = typeof(GameSettings).GetProperty("g_SSReflections_RefinementSamples"),
                SliderValue = GameSettings.g_SSReflections_RefinementSamples
            });
        }

        public void Update(GameTime gameTime, bool isActive, TransformableObject selectedObject)
        {
            GameStats.UIIsHovered = false;
            if (!isActive || !GameSettings.Editor_enable || !GameSettings.ui_DrawUI) return;
            
            GUIControl.Update(Input.mouseLastState, Input.mouseState);

            if (GUIControl.GetMousePosition().X > _rightSideList.Position.X &&
                GUIControl.GetMousePosition().Y < _rightSideList.Dimensions.Y)
            {
                GameStats.UIIsHovered = true;
            }

            if (selectedObject != null)
            {
                //Check if cached, otherwise apply

                if (activeObjectName != selectedObject.Name || activeObjectPos != selectedObject.Position)
                {
                    _objectDescriptionList.IsHidden = false;
                    _objectDescriptionName.Text.Clear();
                    _objectDescriptionName.Text.Append(selectedObject.Name);
                    _objectDescriptionName.TextAlignment = GUIStyle.TextAlignment.Center;

                    _objectDescriptionPos.Text.Clear();
                    _objectDescriptionPos.Text.AppendVector3(selectedObject.Position);
                    _objectDescriptionPos.TextAlignment = GUIStyle.TextAlignment.Center;

                    activeObjectName = selectedObject.Name;
                    activeObjectPos = selectedObject.Position;
                }

                _objectButton1.IsHidden = true;
                _objectToggle1.IsHidden = true;
                _objectToggle2.IsHidden = true;
                _objectSlider1.IsHidden = true;
                _objectSlider2.IsHidden = true;
                _objectColorPicker1.IsHidden = true;

                if (selectedObject is PointLightSource)
                {
                    _objectToggle1.IsHidden = false;
                    _objectToggle2.IsHidden = false;
                    _objectSlider1.IsHidden = false;
                    _objectSlider2.IsHidden = false;
                    _objectColorPicker1.IsHidden = false;

                    if (activeObject != selectedObject)
                    {
                        _objectToggle1.SetField(selectedObject, "IsVolumetric");
                        _objectToggle1.Text = new StringBuilder("Volumetric");

                        _objectToggle2.SetField(selectedObject, "CastShadow");
                        _objectToggle2.Text = new StringBuilder("Cast Shadows");

                        _objectSlider1.MinValue = 1.1f;
                        _objectSlider1.MaxValue = 200;

                        _objectSlider1.SetProperty(selectedObject, "Radius");
                        _objectSlider1.SetText(new StringBuilder("Radius: "));

                        _objectSlider2.MinValue = 0.01f;
                        _objectSlider2.MaxValue = 1000;

                        _objectSlider2.SetField(selectedObject, "Intensity");
                        _objectSlider2.SetText(new StringBuilder("Intensity: "));

                        _objectColorPicker1.ReferenceObject = selectedObject;
                        _objectColorPicker1.ReferenceProperty = selectedObject.GetType().GetProperty("Color");
                    }
                }

                // Environment Sample!
                if (selectedObject is EnvironmentSample)
                {
                    _objectButton1.IsHidden = false;
                    _objectToggle1.IsHidden = false;

                    if (activeObject != selectedObject)
                    {
                        _objectButton1.ButtonObject = selectedObject;
                        _objectButton1.ButtonMethod = selectedObject.GetType().GetMethod("Update");

                        _objectButton1.Text = new StringBuilder("Update Cubemap");

                        _objectToggle1.ToggleObject = selectedObject;
                        _objectToggle1.ToggleField = selectedObject.GetType().GetField("AutoUpdate");

                        _objectToggle1.Toggle = (selectedObject as EnvironmentSample).AutoUpdate;

                        _objectToggle1.Text = new StringBuilder("Update on move");
                    }
                }

                activeObject = selectedObject;
            }
            else
            {
                _objectDescriptionList.IsHidden = true;
            }

            GuiCanvas.Update(gameTime, GUIControl.GetMousePosition(), Vector2.Zero);
        }

        public void UpdateResolution()
        {
            GUIControl.UpdateResolution(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight);
            GuiCanvas.Resize(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight);
        }
    }
}
