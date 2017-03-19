using System.Text;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using HelperSuite.GUI;
using HelperSuite.GUIHelper;
using HelperSuite.GUIRenderer.Helper;
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
        private GuiSliderFloatText _objectSlider1;

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
                new Vector2(200,35),
                _assets.MonospaceFont,
                Color.Gray, 
                Color.White,
                Color.White,
                GUIStyle.GUIAlignment.None,
                GUIStyle.TextAlignment.Left,
                GUIStyle.TextAlignment.Center,
                Vector2.Zero,
                GuiCanvas.Dimensions);

            GuiCanvas.AddElement(_rightSideList = new GuiListToggleScroll(Vector2.Zero, defaultStyle));
            _rightSideList.Alignment = GUIStyle.GUIAlignment.TopRight;

            _objectDescriptionList = new GUIList(Vector2.Zero, defaultStyle);

            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Enable Selection ")
            {
                ToggleField = typeof(GameStats).GetField("e_EnableSelection"),
                Toggle = true
            });

            _objectDescriptionList.AddElement(_objectDescriptionName = new GUITextBlock(defaultStyle, "objDescName"));
            _objectDescriptionList.AddElement(_objectDescriptionPos = new GUITextBlock(defaultStyle, "objDescName"));
            _objectDescriptionList.AddElement(_objectButton1 = new GUITextBlockButton(defaultStyle, "objButton1") {IsHidden = true});
            _objectDescriptionList.AddElement(_objectToggle1 = new GUITextBlockToggle(defaultStyle, "objToggle1") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectSlider1 = new GuiSliderFloatText(defaultStyle, 0,1,2,"objToggle1") { IsHidden = true });

            _rightSideList.AddElement(_objectDescriptionList);
            //Options
            _rightSideList.AddElement(new GUITextBlock(defaultStyle, "Options") {BlockColor = Color.DimGray, Dimensions = new Vector2(200,10), TextAlignment = GUIStyle.TextAlignment.Center});

            GuiListToggle optionList = new GuiListToggle(Vector2.Zero, defaultStyle);
            _rightSideList.AddElement(optionList);

            optionList.AddElement(new GUITextBlockToggle(defaultStyle, "Temporal AA")
            {
                ToggleField = typeof(GameSettings).GetField("g_TemporalAntiAliasing"),
                Toggle = GameSettings.g_TemporalAntiAliasing
            });

            optionList.AddElement(new GUITextBlockToggle(defaultStyle, "Default Material")
            {
                ToggleField = typeof(GameSettings).GetField("d_defaultMaterial"),
                Toggle = GameSettings.d_defaultMaterial
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
                _objectSlider1.IsHidden = true;

                if (selectedObject is PointLightSource)
                {
                    _objectToggle1.IsHidden = false;
                    _objectSlider1.IsHidden = false;

                    if (activeObject != selectedObject)
                    {
                        _objectToggle1.SetField(selectedObject, "IsVolumetric");
                        _objectToggle1.Text = new StringBuilder("Volumetric");

                        _objectSlider1.MinValue = 1.1f;
                        _objectSlider1.MaxValue = 200;

                        _objectSlider1.SetProperty(selectedObject, "Radius");
                        _objectSlider1.SetText(new StringBuilder("Radius: "));

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
