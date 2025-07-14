using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Gamejamai.Engine;

namespace Gamejamai
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Renderer _renderer;
        private World _world;
        private Player _player;
        private InventoryWheel _inventoryWheel;
        private Engine.Horse _horse;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _renderer = new Renderer(GraphicsDevice);
            _world = new World();
            _player = new Player();
            _inventoryWheel = new InventoryWheel();
            _horse = new Engine.Horse(new Microsoft.Xna.Framework.Vector2(500, 350));
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Engine.Assets.Load(Content);
        }

        private bool _prevIKey = false;
        private bool _prevEnterKey = false;
        private bool _prevLeftTrigger = false;
        private bool _prevAButton = false;

        protected override void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            var gamepad = GamePad.GetState(PlayerIndex.One);

            if (gamepad.Buttons.Back == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Escape))
                Exit();

            // Inventory wheel show/hide
            bool iKey = keyboard.IsKeyDown(Keys.I);
            bool leftTrigger = gamepad.Triggers.Left > 0.5f;
            if ((iKey && !_prevIKey) || (leftTrigger && !_prevLeftTrigger))
                _inventoryWheel.Show();
            if ((!iKey && _prevIKey) || (!leftTrigger && _prevLeftTrigger))
                _inventoryWheel.Hide();
            _prevIKey = iKey;
            _prevLeftTrigger = leftTrigger;

            // Inventory navigation (only when visible)
            if (_inventoryWheel.IsVisible)
            {
                if (keyboard.IsKeyDown(Keys.Left)) _inventoryWheel.PrevItem();
                if (keyboard.IsKeyDown(Keys.Right)) _inventoryWheel.NextItem();
                if (gamepad.DPad.Left == ButtonState.Pressed) _inventoryWheel.PrevItem();
                if (gamepad.DPad.Right == ButtonState.Pressed) _inventoryWheel.NextItem();

                // Use item
                bool enterKey = keyboard.IsKeyDown(Keys.Enter);
                bool aButton = gamepad.Buttons.A == ButtonState.Pressed;
                if ((enterKey && !_prevEnterKey) || (aButton && !_prevAButton))
                    _inventoryWheel.UseSelected(_player);
                _prevEnterKey = enterKey;
                _prevAButton = aButton;
            }
            else
            {
                _player.Update(gameTime, _world, _inventoryWheel);
            }
            _inventoryWheel.Update(gameTime, _player);
            _horse.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            _renderer.DrawWorld(_spriteBatch, _world);
            _renderer.DrawHorse(_spriteBatch, _horse);
            _renderer.DrawPlayer(_spriteBatch, _player);
            _renderer.DrawInventoryWheel(_spriteBatch, _inventoryWheel, _player);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
