using Microsoft.Xna.Framework;

namespace Gamejamai.Engine
{
    public class Player
    {
        public Microsoft.Xna.Framework.Vector2 Position;
        public int Ammo = 7;
        public int MaxAmmo = 7;
        private bool _prevReloadKey = false;
        private bool _prevReloadButton = false;

        public Player()
        {
            Position = new Microsoft.Xna.Framework.Vector2(400, 300);
        }

        public void Update(GameTime gameTime, World world, InventoryWheel inventoryWheel)
        {
            var k = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            var g = Microsoft.Xna.Framework.Input.GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);
            float speed = 100f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (k.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W)) Position.Y -= speed;
            if (k.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S)) Position.Y += speed;
            if (k.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A)) Position.X -= speed;
            if (k.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D)) Position.X += speed;
            // Clamp to world
            Position.X = MathHelper.Clamp(Position.X, 0, world.Width);
            Position.Y = MathHelper.Clamp(Position.Y, 0, world.Height);

            // Reload input (R or X button)
            bool reloadKey = k.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.R);
            bool reloadButton = g.Buttons.X == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
            if ((reloadKey && !_prevReloadKey) || (reloadButton && !_prevReloadButton))
            {
                Reload();
            }
            _prevReloadKey = reloadKey;
            _prevReloadButton = reloadButton;
        }

        public void Reload()
        {
            Ammo = MaxAmmo;
        }
    }
}
