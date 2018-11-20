using System;
using System.Reflection;
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
        private GUIList _leftSideList;

        private GUIList _objectDescriptionList;
        private GUITextBlock _objectDescriptionName;
        private GUITextBlock _objectDescriptionPos;
        private GUITextBlockButton _objectButton1;
        private GUITextBlockToggle _objectToggle0;
        private GUITextBlockToggle _objectToggle1;
        private GUITextBlockToggle _objectToggle2;
        private GUITextBlockToggle _objectToggle3;
        private GuiSliderFloatText _objectSlider0;
        private GuiSliderFloatText _objectSlider1;
        private GuiSliderIntText _objectSlider2;
        private GUIColorPicker _objectColorPicker1;

        private GUIStyle defaultStyle;

        private GUITextBlockButton _gizmoTranslation;
        private GUITextBlockButton _gizmoRotation;
        private GUITextBlockButton _gizmoScale;
        private EditorLogic.GizmoModes _gizmoModePrevious;

        //Selected object
        private TransformableObject activeObject;
        private string activeObjectName;
        private Vector3 activeObjectPos;


        public void Initialize(Assets assets, Camera sceneLogicCamera)
        {
            _assets = assets;

            CreateGUI(sceneLogicCamera);
        }


        /// <summary>
        /// Creates the GUI for the default editor
        /// </summary>
        /// <param name="sceneLogicCamera"></param>
        private void CreateGUI(Camera sceneLogicCamera)
        {
            GuiCanvas = new GUICanvas(Vector2.Zero, new Vector2(GameSettings.g_screenwidth, GameSettings.g_screenheight));

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

            //Editor gizmo control!
            GuiCanvas.AddElement(_leftSideList = new GUIList(Vector2.Zero, defaultStyle));

            _leftSideList.AddElement(_gizmoTranslation = new GUITextBlockButton(defaultStyle, "Translate (T)")
            {
                ButtonObject = this,
                ButtonMethod = GetType().GetMethod("ChangeGizmoMode"),
                ButtonMethodArgs = new object[]{ EditorLogic.GizmoModes.Translation },
            });
            _leftSideList.AddElement(_gizmoRotation = new GUITextBlockButton(defaultStyle, "Rotate (R)")
            {
                ButtonObject = this,
                ButtonMethod = GetType().GetMethod("ChangeGizmoMode"),
                ButtonMethodArgs = new object[] { EditorLogic.GizmoModes.Rotation },
            });
            _leftSideList.AddElement(_gizmoScale = new GUITextBlockButton(defaultStyle, "Scale (Z)")
            {
                ButtonObject = this,
                ButtonMethod = GetType().GetMethod("ChangeGizmoMode"),
                ButtonMethodArgs = new object[] { EditorLogic.GizmoModes.Scale },
            });
            _leftSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Local: ")
            {
               ToggleField = typeof(GameStats).GetField("e_LocalTransformation"),
                Toggle = GameStats.e_LocalTransformation
            });
            _leftSideList.Alignment = GUIStyle.GUIAlignment.BottomLeft;

            ChangeGizmoMode(EditorLogic.GizmoModes.Translation);

            //Editor options
            GuiCanvas.AddElement(_rightSideList = new GuiListToggleScroll(new Vector2(-20,0), defaultStyle));

            GUITextBlock helperText = new GUITextBlock(new Vector2(0, 100), new Vector2(300, 200), CreateHelperText(), defaultStyle.TextFontStyle, new Color(Color.DimGray, 0.2f), Color.White, GUIStyle.TextAlignment.Left, new Vector2(10, 1)) {IsHidden = true};
            GuiCanvas.AddElement(helperText);

            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Enable Editor")
            {
                ToggleField = typeof(GameStats).GetField("e_EnableSelection"),
                Toggle = GameStats.e_EnableSelection
            });

            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Highlight Meshes")
            {
                ToggleField = typeof(GameSettings).GetField("e_drawoutlines"),
                Toggle = GameSettings.e_drawoutlines
            });

            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Show Controls")
            {
                ToggleProperty = typeof(GUITextBlock).GetProperty("IsVisible"),
                ToggleObject = helperText,
                Toggle = helperText.IsVisible
            });

            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Default Material")
            {
                ToggleField = typeof(GameSettings).GetField("d_defaultmaterial"),
                Toggle = GameSettings.d_defaultmaterial
            });

            _rightSideList.AddElement(new GuiSliderFloatText(defaultStyle, 0.1f, 3/*(float) (Math.PI - 0.1)*/, 2, "Field Of View: ")
            {
                SliderObject = sceneLogicCamera,
                SliderProperty = typeof(Camera).GetProperty("FieldOfView"),
                SliderValue = sceneLogicCamera.FieldOfView
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
            _objectDescriptionList.AddElement(_objectToggle3 = new GUITextBlockToggle(defaultStyle, "objToggle3") { IsHidden = true });
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
            //SDF
            /////////////////////////////////////////////////////////////////
            /// 
            optionList.AddElement(new GUITextBlock(Vector2.Zero, new Vector2(200, 10), "SDF",
                defaultStyle.TextFontStyle, Color.DarkSlateGray, Color.White, GUIStyle.TextAlignment.Center,
                Vector2.Zero));

            GuiListToggle sdfList = new GuiListToggle(Vector2.Zero, defaultStyle) { ToggleBlockColor = Color.DarkSlateGray, IsToggled = false };
            optionList.AddElement(sdfList);

            sdfList.AddElement(new GUITextBlockToggle(defaultStyle, "Draw SDF")
            {
                ToggleField = typeof(GameSettings).GetField("sdf_drawdistance"),
                Toggle = GameSettings.sdf_drawdistance
            });

            sdfList.AddElement(new GUITextBlockToggle(defaultStyle, "Draw SDF volume")
            {
                ToggleField = typeof(GameSettings).GetField("sdf_drawvolume"),
                Toggle = GameSettings.sdf_drawvolume
            });

            /////////////////////////////////////////////////////////////////
            //Post Processing
            /////////////////////////////////////////////////////////////////

            optionList.AddElement(new GUITextBlock(defaultStyle, "PostProcessing") { BlockColor = Color.DarkSlateGray, Dimensions = new Vector2(200, 10), TextAlignment = GUIStyle.TextAlignment.Center });

            GuiListToggle postprocessingList = new GuiListToggle(Vector2.Zero, defaultStyle) {ToggleBlockColor = Color.DarkSlateGray, IsToggled = false};
            optionList.AddElement(postprocessingList);

            postprocessingList.AddElement(new GUITextBlockToggle(defaultStyle, "Temporal AA")
            {
                ToggleField = typeof(GameSettings).GetField("g_taa"),
                Toggle = GameSettings.g_taa
            });

            postprocessingList.AddElement(new GUITextBlockToggle(defaultStyle, "Tonemap TAA")
            {
                ToggleField = typeof(GameSettings).GetField("g_taa_tonemapped"),
                Toggle = GameSettings.g_taa_tonemapped
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

            postprocessingList.AddElement(new GUITextBlockToggle(defaultStyle, "Color Grading")
            {
                ToggleField = typeof(GameSettings).GetField("g_ColorGrading"),
                Toggle = GameSettings.g_ColorGrading
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
                ToggleProperty = typeof(GameSettings).GetProperty("g_ssao_draw"),
                Toggle = GameSettings.g_ssao_draw
            });

            ssaoList.AddElement(new GUITextBlockToggle(defaultStyle, "SSAO Blur: ")
            {
                ToggleField = typeof(GameSettings).GetField("g_ssao_blur"),
                Toggle = GameSettings.g_ssao_blur
            });

            ssaoList.AddElement(new GuiSliderIntText(defaultStyle, 1, 32, 1, "SSAO Samples: ")
            {
                SliderProperty = typeof(GameSettings).GetProperty("g_ssao_samples"),
                SliderValue = GameSettings.g_ssao_samples
            });

            ssaoList.AddElement(new GuiSliderFloatText(defaultStyle, 1, 100, 2, "Sample Radius: ")
            {
                SliderProperty = typeof(GameSettings).GetProperty("g_ssao_radius"),
                SliderValue = GameSettings.g_ssao_radius
            });

            ssaoList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 4, 1, "SSAO Strength: ")
            {
                SliderProperty = typeof(GameSettings).GetProperty("g_ssao_strength"),
                SliderValue = GameSettings.g_ssao_strength
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

            _rightSideList.Alignment = GUIStyle.GUIAlignment.TopRight;
        }

        public void ChangeGizmoMode(EditorLogic.GizmoModes mode)
        {
            GameStats.e_gizmoMode = mode;

            UpdateGizmoSelection(mode);
        }

        private void UpdateGizmoSelection(EditorLogic.GizmoModes mode)
        {
            switch (mode)
            {
                case EditorLogic.GizmoModes.Translation:
                    _gizmoTranslation.BlockColor = Color.MonoGameOrange;
                    _gizmoRotation.BlockColor = Color.Gray;
                    _gizmoScale.BlockColor = Color.Gray;
                    break;
                case EditorLogic.GizmoModes.Rotation:
                    _gizmoTranslation.BlockColor = Color.Gray;
                    _gizmoRotation.BlockColor = Color.MonoGameOrange;
                    _gizmoScale.BlockColor = Color.Gray;
                    break;
                case EditorLogic.GizmoModes.Scale:
                    _gizmoTranslation.BlockColor = Color.Gray;
                    _gizmoRotation.BlockColor = Color.Gray;
                    _gizmoScale.BlockColor = Color.MonoGameOrange;
                    break;
            }
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
                    "Z - select scale gizmo\n" +
                   "\n" +
                   "F1 - Cycle through render targets\n";
        }

        public void Update(GameTime gameTime, bool isActive, TransformableObject selectedObject)
        {
            GameStats.UIIsHovered = false;
            if (!isActive || !GameSettings.e_enableeditor || !GameSettings.ui_enabled) return;

            if (GameStats.e_gizmoMode != _gizmoModePrevious)
            {
                _gizmoModePrevious = GameStats.e_gizmoMode;
                UpdateGizmoSelection(_gizmoModePrevious);
            }
            
            GUIControl.Update(Input.mouseLastState, Input.mouseState);

            if (GUIControl.GetMousePosition().X > _rightSideList.Position.X &&
                GUIControl.GetMousePosition().Y < _rightSideList.Dimensions.Y)
            {
                GameStats.UIIsHovered = true;
            }

             _leftSideList.IsHidden = !GameStats.e_EnableSelection;

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
                _objectToggle3.IsHidden = true;
                _objectSlider0.IsHidden = true;
                _objectSlider1.IsHidden = true;
                _objectSlider2.IsHidden = true;
                _objectColorPicker1.IsHidden = true;

                if (selectedObject is PointLight)
                {
                    _objectToggle0.IsHidden = false;
                    _objectToggle1.IsHidden = false;
                    _objectToggle2.IsHidden = false;
                    _objectToggle3.IsHidden = false;
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

                        _objectToggle3.SetField(selectedObject, "CastSDFShadows");
                        _objectToggle3.Text = new StringBuilder("Cast SDF Shadows");

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
                    _objectToggle2.IsHidden = false;

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
                        
                        _objectToggle2.SetField(selectedObject, "UseSDFAO");
                        _objectToggle2.Text = new StringBuilder("Use SDFAO");

                        _objectSlider0.SetField(selectedObject, "SpecularStrength");
                        _objectSlider0.SetValues("Specular Strength: ", 0.01f, 1, 2);

                        _objectSlider1.SetField(selectedObject, "DiffuseStrength");
                        _objectSlider1.SetValues("Diffuse Strength: ", 0, 1, 2);
                    }
                }

                else if (selectedObject is DebugEntity)
                {
                    _objectSlider0.IsHidden = false;
                    _objectSlider1.IsHidden = false;
                    _objectSlider2.IsHidden = false;
                    if (activeObject != selectedObject)
                    {
                        //_objectSlider0.SetProperty(selectedObject, "SizeX");
                        //_objectSlider0.SetValues("Size X: ", 0.1f, 200, 2);
                        //_objectSlider1.SetProperty(selectedObject, "SizeY");
                        //_objectSlider1.SetValues("Size Y: ", 0.1f, 200, 2);
                        //_objectSlider2.SetProperty(selectedObject, "SizeZ");
                        //_objectSlider2.SetValues("Size Z: ", 0.1f, 200, 2);
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
            GUIControl.UpdateResolution(GameSettings.g_screenwidth, GameSettings.g_screenheight);
            GuiCanvas.Resize(GameSettings.g_screenwidth, GameSettings.g_screenheight);
        }
    }
}
