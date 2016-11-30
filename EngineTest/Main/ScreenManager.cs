using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics;
using EngineTest.Recources;
using EngineTest.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Main
{
    public class ScreenManager
    {
        private Renderer.Renderer _renderer;
        private MainLogic _logic;
        private EditorLogic _editorLogic;
        private Assets _assets;
        private DebugScreen _debug;

        private EditorLogic.EditorReceivedData _editorReceivedDataBuffer;

        public void Initialize(GraphicsDevice graphicsDevice, Space space)
        {
            _renderer.Initialize(graphicsDevice, _assets);
            _logic.Initialize(_assets, space);
            _editorLogic.Initialize(graphicsDevice);
            _debug.Initialize(graphicsDevice);
        }

        //Update per frame
        public void Update(GameTime gameTime, bool isActive)
        {
            _logic.Update(gameTime, isActive);
            _editorLogic.Update(gameTime, _logic.Entities, _logic.PointLights, _editorReceivedDataBuffer, _logic.MeshMaterialLibrary);
            _renderer.Update(gameTime, isActive);

            _debug.Update(gameTime);
        }

        //Load content
        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _renderer = new Renderer.Renderer();
            _logic = new MainLogic();
            _editorLogic = new EditorLogic();
            _assets = new Assets();
            _debug = new DebugScreen();

            Shaders.Load(content);
            _assets.Load(content, graphicsDevice);
            _renderer.Load(content);
            _logic.Load(content);
            _debug.LoadContent(content);
        }

        public void Unload(ContentManager content)
        {
            content.Dispose();
        }

        /// <summary>
        /// Draw the scene
        /// </summary>
        public void Draw(GameTime gameTime)
        {
            _editorReceivedDataBuffer = _renderer.Draw(_logic.Camera, _logic.MeshMaterialLibrary, _logic.Entities, _logic.PointLights, _logic.DirectionalLights, _editorLogic.GetEditorData(), gameTime);

            _debug.Draw(gameTime);
        }

        public void UpdateResolution()
        {
            _renderer.UpdateResolution();
        }
    }
}
