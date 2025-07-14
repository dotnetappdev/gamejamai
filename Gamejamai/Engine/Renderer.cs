using Microsoft.Xna.Framework.Graphics;
using Gamejamai.Engine;
using System;

namespace Gamejamai.Engine
{
    public class Renderer
    {
        private GraphicsDevice _graphicsDevice;
        public Renderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        // Draw FPS hands and gun in first-person view
        public void DrawFirstPersonView(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, Microsoft.Xna.Framework.Matrix view, Microsoft.Xna.Framework.Matrix projection)
        {
            // Draw hands if model is loaded
            if (Assets.HandsModel != null)
            {
                var handsWorld = Microsoft.Xna.Framework.Matrix.CreateTranslation(0, -0.5f, 1.5f); // Adjust as needed
                foreach (var mesh in Assets.HandsModel.Meshes)
                {
                    foreach (Microsoft.Xna.Framework.Graphics.BasicEffect effect in mesh.Effects)
                    {
                        effect.World = handsWorld;
                        effect.View = view;
                        effect.Projection = projection;
                        effect.EnableDefaultLighting();
                    }
                    mesh.Draw();
                }
            }

            // Draw gun if model is loaded
            if (Assets.GunModel != null)
            {
                var gunWorld = Microsoft.Xna.Framework.Matrix.CreateTranslation(0.2f, -0.4f, 1.7f); // Adjust as needed to fit hands
                foreach (var mesh in Assets.GunModel.Meshes)
                {
                    foreach (Microsoft.Xna.Framework.Graphics.BasicEffect effect in mesh.Effects)
                    {
                        effect.World = gunWorld;
                        effect.View = view;
                        effect.Projection = projection;
                        effect.EnableDefaultLighting();
                    }
                    mesh.Draw();
                }
            }
        }

        public void DrawWorld(SpriteBatch spriteBatch, World world)
        {
            // Draw grass/hills as green pixels (very simple)
            Texture2D pixel = new Texture2D(_graphicsDevice, 1, 1);
            pixel.SetData(new[] { Microsoft.Xna.Framework.Color.White });
            for (int x = 0; x < world.Width; x += 8)
            {
                for (int y = 0; y < world.Height; y += 8)
                {
                    float h = world.Heights[x, y];
                    var color = Microsoft.Xna.Framework.Color.Green;
                    if (h > 18) color = Microsoft.Xna.Framework.Color.ForestGreen;
                    if (h < 5) color = Microsoft.Xna.Framework.Color.LightGreen;
                    spriteBatch.Draw(pixel, new Microsoft.Xna.Framework.Rectangle(x, y, 8, 8), color);
                }
            }
            // Draw rivers as blue rectangles with animated color
            float t = (float)(DateTime.Now.TimeOfDay.TotalSeconds);
            foreach (var river in world.Rivers)
            {
                var waterColor = new Microsoft.Xna.Framework.Color(0, 120, 255) * (0.7f + 0.3f * (float)Math.Sin(t * 2));
                spriteBatch.Draw(pixel, river, waterColor);
            }
            // Draw bridges as brown rectangles
            foreach (var bridge in world.Bridges)
            {
                spriteBatch.Draw(pixel, bridge, Microsoft.Xna.Framework.Color.Sienna);
            }
            // Draw roads as brown rectangles
            foreach (var road in world.Roads)
            {
                spriteBatch.Draw(pixel, road, Microsoft.Xna.Framework.Color.SaddleBrown);
            }
        }

        public void DrawPlayer(SpriteBatch spriteBatch, Player player)
        {
            // Draw player as a blue circle (placeholder)
            Texture2D pixel = new Texture2D(_graphicsDevice, 1, 1);
            pixel.SetData(new[] { Microsoft.Xna.Framework.Color.White });
            var pos = player.Position;
            spriteBatch.Draw(pixel, new Microsoft.Xna.Framework.Rectangle((int)pos.X - 8, (int)pos.Y - 8, 16, 16), Microsoft.Xna.Framework.Color.Blue);
            // TODO: Draw hands and held item

            // Draw bullet counter (ammo) in top-left corner
            // In a real project, use SpriteFont. Here, draw rectangles as bullets.
            int bulletSize = 12;
            int spacing = 4;
            for (int i = 0; i < player.MaxAmmo; i++)
            {
                var color = i < player.Ammo ? Microsoft.Xna.Framework.Color.Gold : Microsoft.Xna.Framework.Color.DarkGray;
                spriteBatch.Draw(pixel, new Microsoft.Xna.Framework.Rectangle(10 + i * (bulletSize + spacing), 10, bulletSize, bulletSize * 2), color);
            }
        }

        public void DrawHorse3D(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, Microsoft.Xna.Framework.Matrix view, Microsoft.Xna.Framework.Matrix projection, Engine.Horse horse)
        {
            if (Assets.HorseModel == null) return;
            var world = Microsoft.Xna.Framework.Matrix.CreateTranslation(new Microsoft.Xna.Framework.Vector3(horse.Position, 0));
            foreach (var mesh in Assets.HorseModel.Meshes)
            {
                foreach (Microsoft.Xna.Framework.Graphics.BasicEffect effect in mesh.Effects)
                {
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
        }

        public void DrawHorse(SpriteBatch spriteBatch, Engine.Horse horse)
        {
            // 2D fallback: Draw horse as a brown ellipse (placeholder)
            Texture2D pixel = new Texture2D(_graphicsDevice, 1, 1);
            pixel.SetData(new[] { Microsoft.Xna.Framework.Color.White });
            var pos = horse.Position;
            spriteBatch.Draw(pixel, new Microsoft.Xna.Framework.Rectangle((int)pos.X - 12, (int)pos.Y - 6, 24, 12), Microsoft.Xna.Framework.Color.Brown);
        }

        public void DrawInventoryWheel(SpriteBatch spriteBatch, InventoryWheel wheel, Player player)
        {
            if (!wheel.IsVisible) return;
            int cx = 400, cy = 300, r = 100;
            int n = wheel.Items.Count;
            for (int i = 0; i < n; i++)
            {
                double angle = (2 * Math.PI / n) * i - Math.PI / 2;
                int x = cx + (int)(Math.Cos(angle) * r);
                int y = cy + (int)(Math.Sin(angle) * r);
                var color = (i == wheel.SelectedIndex) ? Microsoft.Xna.Framework.Color.Yellow : Microsoft.Xna.Framework.Color.Gray;
                Texture2D pixel = new Texture2D(_graphicsDevice, 1, 1);
                pixel.SetData(new[] { Microsoft.Xna.Framework.Color.White });
                spriteBatch.Draw(pixel, new Microsoft.Xna.Framework.Rectangle(x - 30, y - 30, 60, 60), color * 0.5f);
                // Draw icon if available
                string item = wheel.Items[i];
                if (Assets.InventoryIcons.ContainsKey(item))
                {
                    var icon = Assets.InventoryIcons[item];
                    spriteBatch.Draw(icon, new Microsoft.Xna.Framework.Rectangle(x - 24, y - 24, 48, 48), Microsoft.Xna.Framework.Color.White);
                }
                // Optionally: Draw item name (requires SpriteFont)
            }
        }
    }
}
