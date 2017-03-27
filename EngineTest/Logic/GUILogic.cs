using System.Text;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using HelperSuite.GUI;
using HelperSuite.GUIHelper;
using Microsoft.Xna.Framework;

namespace DeferredEngine.Logic
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
        private GUITextBlockToggle _objectToggle0;
        private GUITextBlockToggle _objectToggle1;
        private GUITextBlockToggle _objectToggle2;
        private GuiSliderFloatText _objectSlider0;
        private GuiSliderFloatText _objectSlider1;
        private GuiSliderIntText _objectSlider2;
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

            GUITextBlock helperText = new GUITextBlock(new Vector2(0, 100), new Vector2(300, 200), CreateHelperText(), defaultStyle.TextFontStyle, new Color(Color.DimGray, 0.2f), Color.White, GUIStyle.TextAlignment.Left, new Vector2(10, 1)) {IsHidden = true};
            GuiCanvas.AddElement(helperText);

            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Enable Editor")
            {
                ToggleField = typeof(GameStats).GetField("e_EnableSelection"),
                Toggle = GameStats.e_EnableSelection
            });

            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Highlight Meshes")
            {
                ToggleField = typeof(GameSettings).GetField("e_DrawOutlines"),
                Toggle = GameSettings.e_DrawOutlines
            });

            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Show Controls")
            {
                ToggleProperty = typeof(GUITextBlock).GetProperty("IsVisible"),
                ToggleObject = helperText,
                Toggle = helperText.IsVisible
            });

            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Default Material")
            {
                ToggleField = typeof(GameSettings).GetField("d_defaultMaterial"),
                Toggle = GameSettings.d_defaultMaterial
            });

            //_rightSideList.AddElement(new GuiDropList(defaultStyle, "Show: ")
            //{
            //});

            _rightSideList.AddElement(new GUITextBlock(defaultStyle, "Selection") { BlockColor = Color.DimGray, Dimensions = new Vector2(200, 10), TextAlignment = GUIStyle.TextAlignment.Center });

            GuiListToggle _selectionList = new GuiListToggle(Vector2.Zero, defaultStyle);
            _objectDescriptionList = new GUIList(Vector2.Zero, defaultStyle);

            _objectDescriptionList.AddElement(_objectDescriptionName = new GUITextBlock(defaultStyle, "objDescName"));
            _objectDescriptionList.AddElement(_objectDescriptionPos = new GUITextBlock(defaultStyle, "objDescName"));
            _objectDescriptionList.AddElement(_objectButton1 = new GUITextBlockButton(defaultStyle, "objButton1") {IsHidden = true});
            _objectDescriptionList.AddElement(_objectToggle0 = new GUITextBlockToggle(defaultStyle, "objToggle0") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectToggle1 = new GUITextBlockToggle(defaultStyle, "objToggle1") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectToggle2 = new GUITextBlockToggle(defaultStyle, "objToggle2") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectSlider0 = new GuiSliderFloatText(defaultStyle, 0,1,2,"objToggle1") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectSlider1 = new GuiSliderFloatText(defaultStyle, 0, 1, 2, "objToggle2") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectSlider2 = new GuiSliderIntText(defaultStyle, 0, 10, 1, "objToggle3") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectColorPicker1 = new GUIColorPicker(defaultStyle) { IsHidden = true });

            _selectionList.AddElement(_objectDescriptionList);
            _rightSideList.AddElement(_selectionList);

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

            GuiListToggle postprocessingList = new GuiListToggle(Vector2.Zero, defaultStyle) {ToggleBlockColor = Color.DarkSlateGray, IsToggled = false};
            optionList.AddElement(postprocessingList);

            postprocessingList.AddElement(new GUITextBlockToggle(defaultStyle, "Temporal AA")
            {
                ToggleField = typeof(GameSettings).GetField("g_TemporalAntiAliasing"),
                Toggle = GameSettings.g_TemporalAntiAliasing
            });

            postprocessingList.AddElement(new GUITextBlockToggle(defaultStyle, "Tonemap TAA")
            {
                ToggleField = typeof(GameSettings).GetField("g_TemporalAntiAliasingUseTonemap"),
                Toggle = GameSettings.g_TemporalAntiAliasingUseTonemap
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

            GuiListToggle ssrList = new GuiListToggle(Vector2.Zero, defaultStyle) { ToggleBlockColor = Color.DarkSlateGray, IsToggled = false };
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

            /////////////////////////////////////////////////////////////////
            //SSAO
            /////////////////////////////////////////////////////////////////
            /// 
            optionList.AddElement(new GUITextBlock(Vector2.Zero, new Vector2(200, 10), "Ambient Occlusion",
                defaultStyle.TextFontStyle, Color.DarkSlateGray, Color.White, GUIStyle.TextAlignment.Center,
                Vector2.Zero));

            GuiListToggle ssaoList = new GuiListToggle(Vector2.Zero, defaultStyle) { ToggleBlockColor = Color.DarkSlateGray, IsToggled = false };
            optionList.AddElement(ssaoList);

            ssaoList.AddElement(new GUITextBlockToggle(defaultStyle, "Enable SSAO")
            {
                ToggleProperty = typeof(GameSettings).GetProperty("ssao_Active"),
                Toggle = GameSettings.ssao_Active
            });

            ssaoList.AddElement(new GUITextBlockToggle(defaultStyle, "SSAO Blur: ")
            {
                ToggleField = typeof(GameSettings).GetField("ssao_Blur"),
                Toggle = GameSettings.ssao_Blur
            });

            ssaoList.AddElement(new GuiSliderIntText(defaultStyle, 1, 32, 1, "SSAO Samples: ")
            {
                SliderProperty = typeof(GameSettings).GetProperty("ssao_Samples"),
                SliderValue = GameSettings.ssao_Samples
            });

            ssaoList.AddElement(new GuiSliderFloatText(defaultStyle, 1, 100, 2, "Sample Radius: ")
            {
                SliderProperty = typeof(GameSettings).GetProperty("ssao_SampleRadius"),
                SliderValue = GameSettings.ssao_SampleRadius
            });

            ssaoList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 4, 1, "SSAO Strength: ")
            {
                SliderProperty = typeof(GameSettings).GetProperty("ssao_Strength"),
                SliderValue = GameSettings.ssao_Strength
            });

            /////////////////////////////////////////////////////////////////
            //Bloom
            /////////////////////////////////////////////////////////////////
            /// 
            optionList.AddElement(new GUITextBlock(Vector2.Zero, new Vector2(200, 10), "Bloom",
                defaultStyle.TextFontStyle, Color.DarkSlateGray, Color.White, GUIStyle.TextAlignment.Center,
                Vector2.Zero));

            GuiListToggle bloomList = new GuiListToggle(Vector2.Zero, defaultStyle) { ToggleBlockColor = Color.DarkSlateGray, IsToggled = false };
            optionList.AddElement(bloomList);

            bloomList.AddElement(new GUITextBlockToggle(defaultStyle, "Enable Bloom")
            {
                ToggleField = typeof(GameSettings).GetField("g_BloomEnable"),
                Toggle = GameSettings.g_BloomEnable
            });

            bloomList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 4, 1, "Threshold: ")
            {
                SliderField = typeof(GameSettings).GetField("g_BloomThreshold"),
                SliderValue = GameSettings.g_BloomThreshold
            });

            bloomList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 8, 1, "MIP0 Radius: ")
            {
                SliderField = typeof(GameSettings).GetField("g_BloomRadius1"),
                SliderValue = GameSettings.g_BloomRadius1
            });

            bloomList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 8, 1, "MIP0 Strength: ")
            {
                SliderField = typeof(GameSettings).GetField("g_BloomStrength1"),
                SliderValue = GameSettings.g_BloomStrength1
            });


            bloomList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 8, 1, "MIP1 Radius: ")
            {
                SliderField = typeof(GameSettings).GetField("g_BloomRadius2"),
                SliderValue = GameSettings.g_BloomRadius2
            });

            bloomList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 8, 1, "MIP1 Strength: ")
            {
                SliderField = typeof(GameSettings).GetField("g_BloomStrength2"),
                SliderValue = GameSettings.g_BloomStrength2
            });

            bloomList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 8, 1, "MIP2 Radius: ")
            {
                SliderField = typeof(GameSettings).GetField("g_BloomRadius3"),
                SliderValue = GameSettings.g_BloomRadius3
            });

            bloomList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 8, 1, "MIP2 Strength: ")
            {
                SliderField = typeof(GameSettings).GetField("g_BloomStrength3"),
                SliderValue = GameSettings.g_BloomStrength3
            });

            bloomList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 8, 1, "MIP3 Radius: ")
            {
                SliderField = typeof(GameSettings).GetField("g_BloomRadius4"),
                SliderValue = GameSettings.g_BloomRadius4
            });

            bloomList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 8, 1, "MIP3 Strength: ")
            {
                SliderField = typeof(GameSettings).GetField("g_BloomStrength4"),
                SliderValue = GameSettings.g_BloomStrength4
            });


            bloomList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 8, 1, "MIP4 Radius: ")
            {
                SliderField = typeof(GameSettings).GetField("g_BloomRadius5"),
                SliderValue = GameSettings.g_BloomRadius5
            });

            bloomList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 8, 1, "MIP4 Strength: ")
            {
                SliderField = typeof(GameSettings).GetField("g_BloomStrength5"),
                SliderValue = GameSettings.g_BloomStrength5
            });

        }

        private string CreateHelperText()
        {
            return "Deferred Engine Controls\n" +
                    "Space - toggle on/off tools\n" +
                    "W A S D - move camera\n" +
                    "Right Mouse Button - Rotate Camera\n" +
                    "\n" +
                    "In Editor mode:\n" +
                    "Left Mouse Button - Select Object\n" +
                    "CTRL-C / Insert - duplicate object\n" +
                    "Del - delete object\n" +
                    "T - select translation gizmo\n" +
                    "R - select rotation gizmo\n" +
                   "\n" +
                   "F1 - Cycle through render targets\n";
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
                _objectToggle0.IsHidden = true;
                _objectToggle1.IsHidden = true;
                _objectToggle2.IsHidden = true;
                _objectSlider0.IsHidden = true;
                _objectSlider1.IsHidden = true;
                _objectSlider2.IsHidden = true;
                _objectColorPicker1.IsHidden = true;

                if (selectedObject is PointLight)
                {
                    _objectToggle0.IsHidden = false;
                    _objectToggle1.IsHidden = false;
                    _objectToggle2.IsHidden = false;
                    _objectSlider0.IsHidden = false;
                    _objectSlider1.IsHidden = false;
                    _objectSlider2.IsHidden = false;
                    _objectColorPicker1.IsHidden = false;

                    if (activeObject != selectedObject)
                    {
                        _objectToggle0.SetProperty(selectedObject, "IsEnabled");
                        _objectToggle0.Text = new StringBuilder("IsEnabled");

                        _objectToggle1.SetField(selectedObject, "IsVolumetric");
                        _objectToggle1.Text = new StringBuilder("Volumetric");

                        _objectToggle2.SetField(selectedObject, "CastShadows");
                        _objectToggle2.Text = new StringBuilder("Cast Shadows");

                        _objectSlider0.MinValue = 1.1f;
                        _objectSlider0.MaxValue = 200;

                        _objectSlider0.SetProperty(selectedObject, "Radius");
                        _objectSlider0.SetText(new StringBuilder("Radius: "));

                        _objectSlider1.MinValue = 0.01f;
                        _objectSlider1.MaxValue = 1000;

                        _objectSlider1.SetField(selectedObject, "Intensity");
                        _objectSlider1.SetText(new StringBuilder("Intensity: "));

                        _objectSlider2.SetValues("Shadow Softness: ", 1, 20, 1);
                        _objectSlider2.SetField(selectedObject, "ShadowMapRadius");

                        _objectColorPicker1.SetProperty(selectedObject, "Color");
                    }
                }

                else if (selectedObject is DirectionalLight)
                {
                    _objectToggle0.IsHidden = false;
                    _objectToggle2.IsHidden = false;
                    _objectSlider1.IsHidden = false;
                    _objectColorPicker1.IsHidden = false;

                    if (activeObject != selectedObject)
                    {
                        _objectToggle0.SetProperty(selectedObject, "IsEnabled");
                        _objectToggle0.Text = new StringBuilder("IsEnabled");

                        _objectToggle2.SetField(selectedObject, "CastShadows");
                        _objectToggle2.Text = new StringBuilder("Cast Shadows");
                        
                        _objectSlider1.MinValue = 0.01f;
                        _objectSlider1.MaxValue = 1000;

                        _objectSlider1.SetField(selectedObject, "Intensity");
                        _objectSlider1.SetText(new StringBuilder("Intensity: "));

                        _objectColorPicker1.SetProperty(selectedObject, "Color");
                    }
                }

                // Environment Sample!
                else if(selectedObject is EnvironmentSample)
                {
                    _objectButton1.IsHidden = false;
                    _objectToggle1.IsHidden = false;

                    _objectSlider0.IsHidden = false;
                    _objectSlider1.IsHidden = false;

                    if (activeObject != selectedObject)
                    {
                        _objectButton1.ButtonObject = selectedObject;
                        _objectButton1.ButtonMethod = selectedObject.GetType().GetMethod("Update");

                        _objectButton1.Text = new StringBuilder("Update Cubemap");

                        _objectToggle1.ToggleObject = selectedObject;
                        _objectToggle1.ToggleField = selectedObject.GetType().GetField("AutoUpdate");

                        _objectToggle1.Toggle = (selectedObject as EnvironmentSample).AutoUpdate;

                        _objectToggle1.Text = new StringBuilder("Update on move");

                        _objectSlider0.SetField(selectedObject, "SpecularStrength");
                        _objectSlider0.SetValues("Specular Strength: ", 0.01f, 1, 2);

                        _objectSlider1.SetField(selectedObject, "DiffuseStrength");
                        _objectSlider1.SetValues("Diffuse Strength: ", 0, 1, 2);
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
