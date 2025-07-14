using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Gamejamai.Engine
{
    public class World
    {
        public int Width { get; } = 800;
        public int Height { get; } = 600;
        public float[,] Heights; // For hills
        public List<Rectangle> Roads = new List<Rectangle>();
        public List<Rectangle> Rivers = new List<Rectangle>();
        public List<Rectangle> Bridges = new List<Rectangle>();

        public World()
        {
            Heights = new float[Width, Height];
            GenerateHills();
            GenerateRoads();
            GenerateRivers();
            GenerateBridges();
        }
        private void GenerateRivers()
        {
            // Add a horizontal river
            Rivers.Add(new Rectangle(0, Height / 3, Width, 24));
            // Add a vertical river
            Rivers.Add(new Rectangle(Width / 3, 0, 24, Height));
        }

        private void GenerateBridges()
        {
            // Add bridges over the rivers (as rectangles)
            // Horizontal bridge over vertical river
            Bridges.Add(new Rectangle(Width / 3 - 8, Height / 2 - 30, 40, 60));
            // Vertical bridge over horizontal river
            Bridges.Add(new Rectangle(Width / 2 - 30, Height / 3 - 8, 60, 40));
        }

        private void GenerateHills()
        {
            Random rand = new Random();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    // Simple rolling hills using sine waves
                    Heights[x, y] = (float)(10 * Math.Sin(x * 0.01) + 10 * Math.Cos(y * 0.01) + rand.NextDouble() * 2);
                }
            }
        }

        private void GenerateRoads()
        {
            // Add a rough road horizontally across the map
            Roads.Add(new Rectangle(0, Height / 2 - 10, Width, 20));
            // Add a vertical road
            Roads.Add(new Rectangle(Width / 2 - 10, 0, 20, Height));
        }
    }
}
