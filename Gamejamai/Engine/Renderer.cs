using Microsoft.Xna.Framework.Graphics;
using Gamejamai.Engine;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Gamejamai.Engine
{
    public class Renderer
    {
        private GraphicsDevice _graphicsDevice;
        private BasicEffect _basicEffect;
        private VertexBuffer _terrainVertexBuffer = null!;
        private IndexBuffer _terrainIndexBuffer = null!;
        private VertexBuffer _waterVertexBuffer = null!;
        private IndexBuffer _waterIndexBuffer = null!;
        private int _terrainIndexCount;
        private int _waterIndexCount;
        private Texture2D _whitePixel;
        private bool _terrain3DInitialized = false;

        public Renderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _basicEffect = new BasicEffect(graphicsDevice);
            
            // Create a single white pixel for basic rendering
            _whitePixel = new Texture2D(_graphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });
            
            // Setup basic effect
            _basicEffect.Alpha = 1.0f;
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.LightingEnabled = true;
            _basicEffect.EnableDefaultLighting();
        }

        private void Initialize3DTerrain(World world)
        {
            if (_terrain3DInitialized) return;
            
            // Generate terrain vertices
            var terrainVertices = new List<VertexPositionColor>();
            var terrainIndices = new List<int>();
            
            int width = world.Width;
            int height = world.Height;
            float scale = 0.5f;
            
            // Create terrain vertices
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    float y = world.Heights[x, z] * 0.1f; // Scale height
                    
                    // Determine color based on height
                    Color vertexColor = Color.Green; // Default grass
                    if (y > 1.8f) vertexColor = Color.Gray; // Mountains
                    else if (y > 1.0f) vertexColor = Color.DarkGreen; // Hills
                    else if (y < 0.5f) vertexColor = Color.SandyBrown; // Valleys/beaches
                    
                    terrainVertices.Add(new VertexPositionColor(
                        new Vector3(x * scale, y, z * scale), 
                        vertexColor));
                }
            }
            
            // Create terrain indices for triangles
            for (int x = 0; x < width - 1; x++)
            {
                for (int z = 0; z < height - 1; z++)
                {
                    int topLeft = x * height + z;
                    int topRight = (x + 1) * height + z;
                    int bottomLeft = x * height + (z + 1);
                    int bottomRight = (x + 1) * height + (z + 1);
                    
                    // First triangle
                    terrainIndices.Add(topLeft);
                    terrainIndices.Add(bottomLeft);
                    terrainIndices.Add(topRight);
                    
                    // Second triangle
                    terrainIndices.Add(topRight);
                    terrainIndices.Add(bottomLeft);
                    terrainIndices.Add(bottomRight);
                }
            }
            
            // Create water vertices (flat plane at water level)
            var waterVertices = new List<VertexPositionColor>();
            var waterIndices = new List<int>();
            float waterLevel = 0.3f;
            
            foreach (var river in world.Rivers)
            {
                // Create water quads for each river rectangle
                float x1 = river.X * scale;
                float x2 = (river.X + river.Width) * scale;
                float z1 = river.Y * scale;
                float z2 = (river.Y + river.Height) * scale;
                
                int startIndex = waterVertices.Count;
                
                // Create water quad vertices
                waterVertices.Add(new VertexPositionColor(new Vector3(x1, waterLevel, z1), Color.CornflowerBlue));
                waterVertices.Add(new VertexPositionColor(new Vector3(x2, waterLevel, z1), Color.CornflowerBlue));
                waterVertices.Add(new VertexPositionColor(new Vector3(x1, waterLevel, z2), Color.CornflowerBlue));
                waterVertices.Add(new VertexPositionColor(new Vector3(x2, waterLevel, z2), Color.CornflowerBlue));
                
                // Create water quad indices
                waterIndices.Add(startIndex);
                waterIndices.Add(startIndex + 1);
                waterIndices.Add(startIndex + 2);
                waterIndices.Add(startIndex + 1);
                waterIndices.Add(startIndex + 3);
                waterIndices.Add(startIndex + 2);
            }
            
            // Create vertex and index buffers
            if (terrainVertices.Count > 0)
            {
                _terrainVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColor), terrainVertices.Count, BufferUsage.WriteOnly);
                _terrainVertexBuffer.SetData(terrainVertices.ToArray());
                
                _terrainIndexBuffer = new IndexBuffer(_graphicsDevice, typeof(int), terrainIndices.Count, BufferUsage.WriteOnly);
                _terrainIndexBuffer.SetData(terrainIndices.ToArray());
                _terrainIndexCount = terrainIndices.Count;
            }
            
            if (waterVertices.Count > 0)
            {
                _waterVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColor), waterVertices.Count, BufferUsage.WriteOnly);
                _waterVertexBuffer.SetData(waterVertices.ToArray());
                
                _waterIndexBuffer = new IndexBuffer(_graphicsDevice, typeof(int), waterIndices.Count, BufferUsage.WriteOnly);
                _waterIndexBuffer.SetData(waterIndices.ToArray());
                _waterIndexCount = waterIndices.Count;
            }
            
            _terrain3DInitialized = true;
        }

        public void DrawWorld3D(World world, Matrix view, Matrix projection)
        {
            Initialize3DTerrain(world);
            
            // Set up the effect
            _basicEffect.World = Matrix.Identity;
            _basicEffect.View = view;
            _basicEffect.Projection = projection;
            
            // Draw terrain
            if (_terrainVertexBuffer != null)
            {
                _graphicsDevice.SetVertexBuffer(_terrainVertexBuffer);
                _graphicsDevice.Indices = _terrainIndexBuffer;
                
                foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _terrainIndexCount / 3);
                }
            }
            
            // Draw water with transparency
            if (_waterVertexBuffer != null)
            {
                _graphicsDevice.BlendState = BlendState.AlphaBlend;
                _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                
                _graphicsDevice.SetVertexBuffer(_waterVertexBuffer);
                _graphicsDevice.Indices = _waterIndexBuffer;
                
                foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _waterIndexCount / 3);
                }
                
                _graphicsDevice.BlendState = BlendState.Opaque;
                _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
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
            // 2D fallback: Enhanced 2D rendering with better graphics
            for (int x = 0; x < world.Width; x += 4) // Smaller tiles for better detail
            {
                for (int y = 0; y < world.Height; y += 4)
                {
                    float h = world.Heights[x, y];
                    Color color = Color.Green; // Default grass
                    
                    // More realistic terrain coloring
                    if (h > 18) color = Color.Gray; // High mountains
                    else if (h > 15) color = Color.DarkGray; // Mountains
                    else if (h > 12) color = Color.Brown; // Rocky areas
                    else if (h > 8) color = Color.DarkGreen; // Hills
                    else if (h > 5) color = Color.Green; // Grass
                    else if (h > 2) color = Color.LightGreen; // Valleys
                    else color = Color.SandyBrown; // Beaches/lowlands
                    
                    // Add subtle noise for texture
                    var random = new Random(x * 1000 + y);
                    byte noise = (byte)(random.Next(-20, 20));
                    color = Color.FromNonPremultiplied(
                        Math.Max(0, Math.Min(255, color.R + noise)),
                        Math.Max(0, Math.Min(255, color.G + noise)),
                        Math.Max(0, Math.Min(255, color.B + noise)),
                        255);
                    
                    spriteBatch.Draw(_whitePixel, new Rectangle(x, y, 4, 4), color);
                }
            }
            
            // Draw rivers with animated water effect
            float t = (float)(DateTime.Now.TimeOfDay.TotalSeconds);
            foreach (var river in world.Rivers)
            {
                // Create animated water color
                var baseWaterColor = new Color(30, 144, 255); // DodgerBlue
                float animation = 0.8f + 0.2f * (float)Math.Sin(t * 3);
                var waterColor = baseWaterColor * animation;
                
                // Draw water with slight transparency
                spriteBatch.Draw(_whitePixel, river, waterColor * 0.9f);
                
                // Add water highlights
                var highlightRect = new Rectangle(river.X + 2, river.Y + 2, river.Width - 4, river.Height - 4);
                var highlightColor = Color.LightBlue * (0.3f + 0.2f * (float)Math.Sin(t * 5));
                spriteBatch.Draw(_whitePixel, highlightRect, highlightColor);
            }
            
            // Draw bridges with better materials
            foreach (var bridge in world.Bridges)
            {
                // Draw bridge base
                spriteBatch.Draw(_whitePixel, bridge, Color.Sienna);
                
                // Add bridge planks effect
                for (int i = 0; i < bridge.Width; i += 8)
                {
                    var plankRect = new Rectangle(bridge.X + i, bridge.Y, 6, bridge.Height);
                    spriteBatch.Draw(_whitePixel, plankRect, Color.SaddleBrown);
                }
                
                // Add bridge railings
                var railingTop = new Rectangle(bridge.X, bridge.Y, bridge.Width, 2);
                var railingBottom = new Rectangle(bridge.X, bridge.Y + bridge.Height - 2, bridge.Width, 2);
                spriteBatch.Draw(_whitePixel, railingTop, Color.DarkGray);
                spriteBatch.Draw(_whitePixel, railingBottom, Color.DarkGray);
            }
            
            // Draw roads with more realistic appearance
            foreach (var road in world.Roads)
            {
                // Draw road base
                spriteBatch.Draw(_whitePixel, road, Color.DarkGray);
                
                // Add road texture/gravel effect
                var random = new Random(road.X * 100 + road.Y * 100);
                for (int i = 0; i < road.Width * road.Height / 50; i++)
                {
                    int px = road.X + random.Next(road.Width);
                    int py = road.Y + random.Next(road.Height);
                    var gravelColor = Color.Gray;
                    if (random.Next(3) == 0) gravelColor = Color.LightGray;
                    spriteBatch.Draw(_whitePixel, new Rectangle(px, py, 1, 1), gravelColor);
                }
                
                // Add road center line for larger roads
                if (road.Width > 16)
                {
                    var centerLine = new Rectangle(road.X + road.Width / 2 - 1, road.Y, 2, road.Height);
                    spriteBatch.Draw(_whitePixel, centerLine, Color.Yellow);
                }
            }
        }

        public void DrawPlayer(SpriteBatch spriteBatch, Player player)
        {
            var pos = player.Position;
            
            // Draw player shadow
            var shadowRect = new Rectangle((int)pos.X - 10, (int)pos.Y + 6, 20, 8);
            spriteBatch.Draw(_whitePixel, shadowRect, Color.Black * 0.3f);
            
            // Draw player body (more detailed)
            var bodyRect = new Rectangle((int)pos.X - 6, (int)pos.Y - 12, 12, 18);
            spriteBatch.Draw(_whitePixel, bodyRect, Color.DarkBlue);
            
            // Draw player head
            var headRect = new Rectangle((int)pos.X - 4, (int)pos.Y - 16, 8, 8);
            spriteBatch.Draw(_whitePixel, headRect, Color.PeachPuff);
            
            // Draw player arms
            var leftArmRect = new Rectangle((int)pos.X - 9, (int)pos.Y - 8, 3, 10);
            var rightArmRect = new Rectangle((int)pos.X + 6, (int)pos.Y - 8, 3, 10);
            spriteBatch.Draw(_whitePixel, leftArmRect, Color.PeachPuff);
            spriteBatch.Draw(_whitePixel, rightArmRect, Color.PeachPuff);
            
            // Draw player legs
            var leftLegRect = new Rectangle((int)pos.X - 3, (int)pos.Y + 6, 3, 8);
            var rightLegRect = new Rectangle((int)pos.X + 0, (int)pos.Y + 6, 3, 8);
            spriteBatch.Draw(_whitePixel, leftLegRect, Color.DarkBlue);
            spriteBatch.Draw(_whitePixel, rightLegRect, Color.DarkBlue);

            // Draw enhanced bullet counter (ammo) with better styling
            int bulletSize = 8;
            int spacing = 2;
            int startX = 15;
            int startY = 15;
            
            // Draw ammo counter background
            var ammoBackRect = new Rectangle(startX - 5, startY - 5, 
                player.MaxAmmo * (bulletSize + spacing) + 5, bulletSize + 10);
            spriteBatch.Draw(_whitePixel, ammoBackRect, Color.Black * 0.5f);
            
            // Draw ammo text background
            var ammoTextRect = new Rectangle(startX - 3, startY - 20, 60, 12);
            spriteBatch.Draw(_whitePixel, ammoTextRect, Color.Black * 0.7f);
            
            for (int i = 0; i < player.MaxAmmo; i++)
            {
                var bulletRect = new Rectangle(startX + i * (bulletSize + spacing), startY, bulletSize, bulletSize);
                Color bulletColor = i < player.Ammo ? Color.Gold : Color.DarkGray;
                
                // Add bullet shine effect for loaded bullets
                if (i < player.Ammo)
                {
                    spriteBatch.Draw(_whitePixel, bulletRect, bulletColor);
                    var shineRect = new Rectangle(bulletRect.X + 1, bulletRect.Y + 1, 2, 2);
                    spriteBatch.Draw(_whitePixel, shineRect, Color.Yellow);
                }
                else
                {
                    spriteBatch.Draw(_whitePixel, bulletRect, bulletColor);
                }
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
            var pos = horse.Position;
            
            // Draw horse shadow
            var shadowRect = new Rectangle((int)pos.X - 15, (int)pos.Y + 4, 30, 8);
            spriteBatch.Draw(_whitePixel, shadowRect, Color.Black * 0.3f);
            
            // Draw horse body (more detailed)
            var bodyRect = new Rectangle((int)pos.X - 12, (int)pos.Y - 8, 24, 16);
            spriteBatch.Draw(_whitePixel, bodyRect, Color.SaddleBrown);
            
            // Draw horse head
            var headRect = new Rectangle((int)pos.X - 16, (int)pos.Y - 6, 8, 10);
            spriteBatch.Draw(_whitePixel, headRect, Color.Brown);
            
            // Draw horse legs
            var leg1Rect = new Rectangle((int)pos.X - 10, (int)pos.Y + 8, 3, 8);
            var leg2Rect = new Rectangle((int)pos.X - 5, (int)pos.Y + 8, 3, 8);
            var leg3Rect = new Rectangle((int)pos.X + 2, (int)pos.Y + 8, 3, 8);
            var leg4Rect = new Rectangle((int)pos.X + 7, (int)pos.Y + 8, 3, 8);
            spriteBatch.Draw(_whitePixel, leg1Rect, Color.DarkGoldenrod);
            spriteBatch.Draw(_whitePixel, leg2Rect, Color.DarkGoldenrod);
            spriteBatch.Draw(_whitePixel, leg3Rect, Color.DarkGoldenrod);
            spriteBatch.Draw(_whitePixel, leg4Rect, Color.DarkGoldenrod);
            
            // Draw horse tail
            var tailRect = new Rectangle((int)pos.X + 12, (int)pos.Y - 4, 6, 12);
            spriteBatch.Draw(_whitePixel, tailRect, Color.Brown);
            
            // Draw horse mane
            var maneRect = new Rectangle((int)pos.X - 14, (int)pos.Y - 10, 4, 8);
            spriteBatch.Draw(_whitePixel, maneRect, Color.Brown);
        }

        public void DrawInventoryWheel(SpriteBatch spriteBatch, InventoryWheel wheel, Player player)
        {
            if (!wheel.IsVisible) return;
            
            int cx = 400, cy = 300, r = 80;
            int n = wheel.Items.Count;
            
            // Draw outer ring background
            for (int ring = r + 10; ring > r - 10; ring--)
            {
                float alpha = (float)(r + 10 - ring) / 20.0f * 0.3f;
                // This is a simplified ring - in a real implementation you'd use proper circle drawing
                var ringRect = new Rectangle(cx - ring, cy - ring, ring * 2, ring * 2);
                spriteBatch.Draw(_whitePixel, ringRect, Color.Black * alpha);
            }
            
            for (int i = 0; i < n; i++)
            {
                double angle = (2 * Math.PI / n) * i - Math.PI / 2;
                int x = cx + (int)(Math.Cos(angle) * r);
                int y = cy + (int)(Math.Sin(angle) * r);
                
                bool isSelected = (i == wheel.SelectedIndex);
                Color slotColor = isSelected ? Color.Gold : Color.DarkGray;
                Color borderColor = isSelected ? Color.Yellow : Color.Gray;
                
                // Draw slot background with gradient effect
                var slotRect = new Rectangle(x - 25, y - 25, 50, 50);
                spriteBatch.Draw(_whitePixel, slotRect, slotColor * 0.7f);
                
                // Draw slot border
                var borderTop = new Rectangle(x - 26, y - 26, 52, 2);
                var borderBottom = new Rectangle(x - 26, y + 24, 52, 2);
                var borderLeft = new Rectangle(x - 26, y - 26, 2, 52);
                var borderRight = new Rectangle(x + 24, y - 26, 2, 52);
                
                spriteBatch.Draw(_whitePixel, borderTop, borderColor);
                spriteBatch.Draw(_whitePixel, borderBottom, borderColor);
                spriteBatch.Draw(_whitePixel, borderLeft, borderColor);
                spriteBatch.Draw(_whitePixel, borderRight, borderColor);
                
                // Draw item icon or placeholder
                string item = wheel.Items[i];
                if (Assets.InventoryIcons.ContainsKey(item))
                {
                    var icon = Assets.InventoryIcons[item];
                    var iconRect = new Rectangle(x - 20, y - 20, 40, 40);
                    spriteBatch.Draw(icon, iconRect, Color.White);
                }
                else
                {
                    // Draw placeholder icon based on item type
                    Color iconColor = Color.White;
                    if (item.ToLower().Contains("pistol") || item.ToLower().Contains("gun"))
                        iconColor = Color.Silver;
                    else if (item.ToLower().Contains("ammo"))
                        iconColor = Color.Gold;
                    else if (item.ToLower().Contains("health"))
                        iconColor = Color.Red;
                    
                    var iconRect = new Rectangle(x - 15, y - 15, 30, 30);
                    spriteBatch.Draw(_whitePixel, iconRect, iconColor);
                }
                
                // Add selection glow effect
                if (isSelected)
                {
                    var glowRect = new Rectangle(x - 28, y - 28, 56, 56);
                    float glowAlpha = 0.3f + 0.2f * (float)Math.Sin(DateTime.Now.TimeOfDay.TotalSeconds * 4);
                    spriteBatch.Draw(_whitePixel, glowRect, Color.Yellow * glowAlpha);
                }
            }
            
            // Draw center circle
            var centerRect = new Rectangle(cx - 15, cy - 15, 30, 30);
            spriteBatch.Draw(_whitePixel, centerRect, Color.DarkSlateGray);
            
            var centerBorder = new Rectangle(cx - 16, cy - 16, 32, 32);
            spriteBatch.Draw(_whitePixel, centerBorder, Color.Gray);
        }
    }
}
