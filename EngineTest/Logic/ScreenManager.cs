using System;
using BEPUphysics;
using DeferredEngine.Recources;
using HelperSuite.GUIRenderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Logic
{
    /// <summary>
    /// Manages our different screens and passes information accordingly
    /// </summary>
    public class ScreenManager : IDisposable
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private Renderer.Renderer _renderer;
        private GUIRenderer _guiRenderer;
        private MainSceneLogic _sceneLogic;
        private GUILogic _guiLogic;
        private EditorLogic _editorLogic;
        private Assets _assets;
        private DebugScreen _debug;

        private EditorLogic.EditorReceivedData _editorReceivedDataBuffer;
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Initialize(GraphicsDevice graphicsDevice, Space space)
        {
            _renderer.Initialize(graphicsDevice, _assets);
            _sceneLogic.Initialize(_assets, space, graphicsDevice);
            _guiLogic.Initialize(_assets, _sceneLogic.Camera);
            _editorLogic.Initialize(graphicsDevice);
            _debug.Initialize(graphicsDevice);
            _guiRenderer.Initialize(graphicsDevice, GameSettings.g_screenwidth, GameSettings.g_screenheight);
        }

        //Update per frame
        public void Update(GameTime gameTime, bool isActive)
        {
            _guiLogic.Update(gameTime, isActive, _editorLogic.SelectedObject);
            _editorLogic.Update(gameTime, _sceneLogic.BasicEntities, _sceneLogic.Decals, _sceneLogic.PointLights, _sceneLogic.DirectionalLights, _sceneLogic.EnvironmentSample, _sceneLogic.VolumeTexture, _editorReceivedDataBuffer, _sceneLogic.MeshMaterialLibrary);
            _sceneLogic.Update(gameTime, isActive);
            _renderer.Update(gameTime, isActive, _sceneLogic._sdfGenerator, _sceneLogic.VolumeTexture);
            
            _debug.Update(gameTime);
        }

        //Load content
        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _renderer = new Renderer.Renderer();
            _sceneLogic = new MainSceneLogic();
            _guiLogic = new GUILogic();
            _editorLogic = new EditorLogic();
            _assets = new Assets();
            _debug = new DebugScreen();
            _guiRenderer = new GUIRenderer();

            Globals.content = content;
            Shaders.Load(content);
            _assets.Load(content, graphicsDevice);
            _renderer.Load(content);
            _sceneLogic.Load(content);
            _debug.LoadContent(content);
            _guiRenderer.Load(content);
        }

        public void Unload(ContentManager content)
        {
            content.Dispose();
        }
        
        public void Draw(GameTime gameTime)
        {
            //Our renderer gives us information on what id is currently hovered over so we can update / manipulate objects in the logic functions
            _editorReceivedDataBuffer = _renderer.Draw(_sceneLogic.Camera, 
                _sceneLogic.MeshMaterialLibrary, 
                _sceneLogic.BasicEntities, _sceneLogic.Decals,
                pointLights: _sceneLogic.PointLights,
                directionalLights: _sceneLogic.DirectionalLights, 
                envSample: _sceneLogic.EnvironmentSample, 
                volumeTexture: _sceneLogic.VolumeTexture, 
                editorData: _editorLogic.GetEditorData(), 
                gameTime: gameTime);
            
            if (GameSettings.e_enableeditor && GameSettings.ui_enabled)
                _guiRenderer.Draw(_guiLogic.GuiCanvas);

            _debug.Draw(gameTime);
        }

        public void UpdateResolution()
        {
            _renderer.UpdateResolution();
            _guiLogic.UpdateResolution();
        }

        public void Dispose()
        {
            _guiRenderer?.Dispose();
        }
    }
}
