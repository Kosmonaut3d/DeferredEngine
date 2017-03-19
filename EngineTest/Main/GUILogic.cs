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

        private GUIList _objectDescriptionList;
        private GUITextBlock _objectDescriptionName;
        private GUITextBlock _objectDescriptionPos;

        private GUIList _lightDescriptionList;
        private GUITextBlockToggle _lightEnableToggle;

        private GUIStyle defaultStyle;


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


            GuiCanvas.AddElement(_objectDescriptionList = new GuiListToggle(Vector2.Zero, defaultStyle));
            _objectDescriptionList.Alignment = GUIStyle.GUIAlignment.TopRight;

            _objectDescriptionList.AddElement(_objectDescriptionName = new GUITextBlock(defaultStyle, "objDescName"));
            _objectDescriptionList.AddElement(_objectDescriptionPos = new GUITextBlock(defaultStyle, "objDescName"));

            _objectDescriptionList.AddElement(_lightDescriptionList = new GUIList(Vector2.Zero, new Vector2(300, 40), 0));

            _lightDescriptionList.AddElement(_lightEnableToggle = new GUITextBlockToggle(defaultStyle, "enabled:"));
        }

        public void Update(GameTime gameTime, bool isActive, TransformableObject editorObject)
        {
            if (!isActive || !GameSettings.Editor_enable || !GameSettings.ui_DrawUI) return;
            
            GUIControl.Update(Input.mouseLastState, Input.mouseState);

            if (editorObject != null)
            {
                _objectDescriptionList.IsEnabled = true;
                _objectDescriptionName.Text = new StringBuilder(editorObject.GetType().Name);
                _objectDescriptionPos.Text = new StringBuilder(editorObject.Position.ToString());

                if (editorObject is PointLightSource)
                {
                    _lightDescriptionList.IsEnabled = true;
                    _lightEnableToggle.ToggleObject = editorObject;
                    _lightEnableToggle.Toggle = (editorObject as PointLightSource).IsEnabled;
                    _lightEnableToggle.ToggleProperty = typeof(PointLightSource).GetProperty("IsEnabled");
                }
                else
                {
                    _lightDescriptionList.IsEnabled = false;
                }
            }
            else
            {
                _objectDescriptionList.IsEnabled = false;
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
