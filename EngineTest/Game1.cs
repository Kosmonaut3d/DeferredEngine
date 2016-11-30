using System;
using BEPUphysics;
using BEPUutilities;
using EngineTest.Main;
using EngineTest.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EngineTest
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private ScreenManager screenManager;

        private bool isActive = true;

        private Space space;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            space = new Space();
            space.ForceUpdater.Gravity = new BEPUutilities.Vector3(0,0,-9.81f);

            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;

            graphics.PreferredBackBufferWidth = GameSettings.g_ScreenWidth;
            graphics.PreferredBackBufferHeight = GameSettings.g_ScreenHeight;

            screenManager = new ScreenManager();

            IsMouseVisible = true;

            Window.ClientSizeChanged += ClientChangedWindowSize;

            graphics.GraphicsProfile = GraphicsProfile.HiDef;

            //_graphics.GraphicsDevice.DeviceLost += new EventHandler<EventArgs>(ClientLostDevice);

            Window.AllowUserResizing = true;
            Window.IsBorderless = false;

            Activated += IsActivated;
            Deactivated += IsDeactivated;
        }

        private void IsActivated(object sender, EventArgs e)
        {
            isActive = true;
        }

        private void IsDeactivated(object sender, EventArgs e)
        {
            isActive = false;
        }

        private void ClientChangedWindowSize(object sender, EventArgs e)
        {
            if (GraphicsDevice.Viewport.Width != graphics.PreferredBackBufferWidth ||
                GraphicsDevice.Viewport.Height != graphics.PreferredBackBufferHeight)
            {
                if (Window.ClientBounds.Width == 0) return;
                graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                graphics.ApplyChanges();

                GameSettings.g_ScreenWidth = Window.ClientBounds.Width;
                GameSettings.g_ScreenHeight = Window.ClientBounds.Height;

                screenManager.UpdateResolution();

            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            screenManager.Load(Content, GraphicsDevice);
            // TODO: Add your initialization logic here
            screenManager.Initialize(GraphicsDevice, space);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here

            screenManager.Unload(Content);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //if (!isActive) return;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            screenManager.Update(gameTime, isActive);

            //BEPU
            if(!GameSettings.Editor_enable && GameSettings.p_Physics)
            space.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);

            
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (!isActive) return;

            screenManager.Draw(gameTime);
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
