using System.Text;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Recources.GUI;
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

            //GuiCanvas.AddElement(new GUIBlock(new Vector2(10, 210), new Vector2(200, 40), Color.Red));

            //GuiCanvas.AddElement(new GUITextBlock(new Vector2(10, 250), new Vector2(200, 40), "testString",
            //    _assets.MonospaceFont, new Color(74, 74, 74), Color.White));

            GuiCanvas.AddElement(_objectDescriptionList = new GUIList(Vector2.Zero, new Vector2(300,40),0,GUICanvas.GUIAlignment.TopRight, GuiCanvas.Dimensions));

            _objectDescriptionList.AddElement(_objectDescriptionName = new GUITextBlock(new Vector2(10, 250), new Vector2(200, 40), "objDescName", _assets.MonospaceFont, new Color(74, 74, 74), Color.White));
            _objectDescriptionList.AddElement(_objectDescriptionPos = new GUITextBlock(new Vector2(10, 250), new Vector2(240, 40), "objDescPos", _assets.MonospaceFont, new Color(74, 74, 74), Color.White));

            _objectDescriptionList.AddElement(_lightDescriptionList = new GUIList(Vector2.Zero, new Vector2(300, 40), 0));

            _lightDescriptionList.AddElement(_lightEnableToggle = new GUITextBlockToggle(new Vector2(10, 250), new Vector2(240, 40), "enabled:", _assets.MonospaceFont, new Color(74, 74, 74), Color.White));
            
        }

        public void Update(GameTime gameTime, bool isActive, TransformableObject editorObject)
        {
            GameStats.UIWasClicked = false;
            if (!isActive || !GameSettings.Editor_enable || !GameSettings.ui_DrawUI) return;

            Vector2 mousePosition = Input.GetMousePosition().ToVector2();

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

            GuiCanvas.Update(gameTime, mousePosition, Vector2.Zero);
        }

        public void UpdateResolution()
        {
            GuiCanvas.Resize(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight);
        }
    }
}
