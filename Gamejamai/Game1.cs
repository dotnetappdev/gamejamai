using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Gamejamai.Engine;

namespace Gamejamai
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch = null!;
        private Renderer _renderer = null!;
        private World _world = null!;
        private Player _player = null!;
        private InventoryWheel _inventoryWheel = null!;
        private Engine.Horse _horse = null!;

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

        private float GetTerrainHeightAtPosition(float x, float y)
        {
            // Clamp coordinates to world bounds
            int worldX = (int)MathHelper.Clamp(x, 0, _world.Width - 1);
            int worldY = (int)MathHelper.Clamp(y, 0, _world.Height - 1);
            
            // Get height from world and apply same scaling as in terrain generation
            float worldHeight = _world.Heights[worldX, worldY];
            float heightScale = 0.5f; // Same as in Renderer.cs Initialize3DTerrain
            
            return worldHeight * heightScale;
        }

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
            
            // Create view and projection matrices for 3D rendering
            // Player position in world space (scaled by 2.0f in terrain generation)
            Vector3 playerWorldPos = new Vector3(_player.Position.X * 2.0f, 0, _player.Position.Y * 2.0f);
            
            // Sample terrain height at player position for accurate camera positioning
            float terrainHeight = GetTerrainHeightAtPosition(_player.Position.X, _player.Position.Y);
            
            // Position camera at player eye level (about 1.7 units above ground)
            Vector3 cameraPosition = new Vector3(playerWorldPos.X, terrainHeight + 1.7f, playerWorldPos.Z);
            
            // Look forward from player position (first person view)
            Vector3 cameraTarget = new Vector3(playerWorldPos.X, terrainHeight + 1.7f, playerWorldPos.Z + 10);
            Vector3 cameraUp = Vector3.Up;
            
            Matrix view = Matrix.CreateLookAt(cameraPosition, cameraTarget, cameraUp);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45),
                GraphicsDevice.Viewport.AspectRatio,
                0.1f,
                1000.0f);
            
            // Draw 3D world
            _renderer.DrawWorld3D(_world, view, projection);
            
            // Draw 2D UI elements
            _spriteBatch.Begin();
            _renderer.DrawInventoryWheel(_spriteBatch, _inventoryWheel, _player);
            _renderer.DrawRadar(_spriteBatch, _world, _player);
            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}
