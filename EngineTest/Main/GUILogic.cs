using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineTest.Recources;
using EngineTest.Recources.GUI;
using Microsoft.Xna.Framework;

namespace EngineTest.Main
{
    public class GUILogic
    {
        private Assets _assets;
        public GUICanvas GuiCanvas;

        private GUIList _lightList;

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

            GuiCanvas.AddElement(new GUIBlock(new Vector2(10, 210), new Vector2(200, 40), Color.Red));

            GuiCanvas.AddElement(new GUITextBlock(new Vector2(10, 250), new Vector2(200, 40), "testString",
                _assets.MonospaceFont, new Color(74, 74, 74), Color.White));

            GuiCanvas.AddElement(_lightList = new GUIList(Vector2.Zero, new Vector2(200,40),0,GUICanvas.GUIAlignment.TopRight, GuiCanvas.Dimensions));
            
            _lightList.AddElement(new GUITextBlockToggle(new Vector2(10, 250), new Vector2(200, 40), "testString", _assets.MonospaceFont, new Color(74, 74, 74), Color.White));
            _lightList.AddElement(new GUITextBlockToggle(new Vector2(10, 250), new Vector2(240, 40), "testString", _assets.MonospaceFont, new Color(74, 74, 74), Color.White));


        }

        public void Update(GameTime gameTime, bool isActive)
        {
            if (!isActive || !GameSettings.ui_DrawUI) return;

            Vector2 mousePosition = Input.GetMousePosition().ToVector2();

            GuiCanvas.Update(gameTime, mousePosition, Vector2.Zero);
        }

        public void UpdateResolution()
        {
            GuiCanvas.Resize(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight);
        }
    }
}
