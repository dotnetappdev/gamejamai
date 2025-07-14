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
        private BasicEffect _skyboxEffect;
        private BasicEffect _playerEffect;
        private VertexBuffer _terrainVertexBuffer = null!;
        private IndexBuffer _terrainIndexBuffer = null!;
        private VertexBuffer _waterVertexBuffer = null!;
        private IndexBuffer _waterIndexBuffer = null!;
        private VertexBuffer _skyboxVertexBuffer = null!;
        private IndexBuffer _skyboxIndexBuffer = null!;
        private VertexBuffer _playerVertexBuffer = null!;
        private IndexBuffer _playerIndexBuffer = null!;
        private int _terrainIndexCount;
        private int _waterIndexCount;
        private int _skyboxIndexCount;
        private int _playerIndexCount;
        private Texture2D _whitePixel;
        private Texture2D? _radarTexture;
        private bool _terrain3DInitialized = false;
        private bool _skyboxInitialized = false;
        private bool _playerModelInitialized = false;

        public Renderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _basicEffect = new BasicEffect(graphicsDevice);
            _skyboxEffect = new BasicEffect(graphicsDevice);
            _playerEffect = new BasicEffect(graphicsDevice);
            
            // Create a single white pixel for basic rendering
            _whitePixel = new Texture2D(_graphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });
            
            // Create radar texture
            CreateRadarTexture();
            
            // Setup advanced lighting effect
            _basicEffect.Alpha = 1.0f;
            _basicEffect.LightingEnabled = true;
            _basicEffect.TextureEnabled = false; // We'll enable this when we have textures
            
            // Set up realistic lighting
            _basicEffect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.3f); // Soft ambient light
            _basicEffect.DirectionalLight0.Enabled = true;
            _basicEffect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, -1, -1));
            _basicEffect.DirectionalLight0.DiffuseColor = Vector3.One;
            _basicEffect.DirectionalLight0.SpecularColor = Vector3.One;
            
            // Enable fog for distance effect
            _basicEffect.FogEnabled = true;
            _basicEffect.FogColor = Vector3.One * 0.8f;
            _basicEffect.FogStart = 50.0f;
            _basicEffect.FogEnd = 200.0f;
            
            // Enable per-pixel lighting
            _basicEffect.PreferPerPixelLighting = true;
            
            // Setup skybox effect
            _skyboxEffect.LightingEnabled = false;
            _skyboxEffect.TextureEnabled = false;
            _skyboxEffect.VertexColorEnabled = true;
            
            // Setup player effect
            _playerEffect.LightingEnabled = true;
            _playerEffect.PreferPerPixelLighting = true;
            _playerEffect.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.4f);
            _playerEffect.DirectionalLight0.Enabled = true;
            _playerEffect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, -1, -1));
            _playerEffect.DirectionalLight0.DiffuseColor = Vector3.One;
            
            InitializeSkybox();
            InitializePlayerModel();
        }
        
        private void CreateRadarTexture()
        {
            int size = 120;
            _radarTexture = new Texture2D(_graphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            // Create circular radar background
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 5;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center);
                    
                    if (distance <= radius)
                    {
                        // Inner radar area
                        if (distance <= radius - 10)
                        {
                            data[y * size + x] = Color.Black * 0.7f;
                        }
                        // Radar border
                        else
                        {
                            data[y * size + x] = Color.DarkGray;
                        }
                    }
                    else
                    {
                        data[y * size + x] = Color.Transparent;
                    }
                }
            }
            
            _radarTexture.SetData(data);
        }
        
        private void InitializePlayerModel()
        {
            if (_playerModelInitialized) return;
            
            var playerVertices = new List<VertexPositionNormalTexture>();
            var playerIndices = new List<short>();
            
            // Create a more realistic player model with multiple parts
            
            // Head
            CreateCube(playerVertices, playerIndices, 0.4f, Vector3.Zero + new Vector3(0, 1.7f, 0));
            
            // Body (torso)
            CreateCube(playerVertices, playerIndices, new Vector3(0.6f, 0.8f, 0.3f), Vector3.Zero + new Vector3(0, 0.9f, 0));
            
            // Arms
            CreateCube(playerVertices, playerIndices, new Vector3(0.2f, 0.7f, 0.2f), Vector3.Zero + new Vector3(-0.5f, 1.0f, 0));
            CreateCube(playerVertices, playerIndices, new Vector3(0.2f, 0.7f, 0.2f), Vector3.Zero + new Vector3(0.5f, 1.0f, 0));
            
            // Legs
            CreateCube(playerVertices, playerIndices, new Vector3(0.25f, 0.8f, 0.2f), Vector3.Zero + new Vector3(-0.15f, 0.0f, 0));
            CreateCube(playerVertices, playerIndices, new Vector3(0.25f, 0.8f, 0.2f), Vector3.Zero + new Vector3(0.15f, 0.0f, 0));
            
            // Create player buffers
            if (playerVertices.Count > 0)
            {
                _playerVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionNormalTexture), playerVertices.Count, BufferUsage.WriteOnly);
                _playerVertexBuffer.SetData(playerVertices.ToArray());
                
                _playerIndexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), playerIndices.Count, BufferUsage.WriteOnly);
                _playerIndexBuffer.SetData(playerIndices.ToArray());
                _playerIndexCount = playerIndices.Count;
            }
            
            _playerModelInitialized = true;
        }
        
        private void InitializeSkybox()
        {
            if (_skyboxInitialized) return;
            
            // Create a large cube for the skybox
            var skyboxVertices = new List<VertexPositionColor>();
            var skyboxIndices = new List<short>();
            
            float size = 1000.0f;
            
            // Define skybox vertices with gradient colors (blue to light blue)
            Vector3[] positions = {
                new Vector3(-size, -size, -size), new Vector3(-size, -size,  size),
                new Vector3(-size,  size, -size), new Vector3(-size,  size,  size),
                new Vector3( size, -size, -size), new Vector3( size, -size,  size),
                new Vector3( size,  size, -size), new Vector3( size,  size,  size),
            };
            
            // Add vertices with sky colors (darker at bottom, lighter at top)
            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 pos = positions[i];
                Color skyColor = Color.Lerp(
                    new Color(0.5f, 0.7f, 1.0f), // Light blue at top
                    new Color(0.2f, 0.4f, 0.8f), // Darker blue at bottom
                    (pos.Y + size) / (2 * size)
                );
                skyboxVertices.Add(new VertexPositionColor(pos, skyColor));
            }
            
            // Define skybox indices (inside faces)
            short[] cubeIndices = {
                0, 1, 2, 1, 3, 2, // Left face
                4, 6, 5, 5, 6, 7, // Right face  
                0, 5, 1, 0, 4, 5, // Bottom face
                2, 3, 6, 3, 7, 6, // Top face
                0, 2, 4, 2, 6, 4, // Front face
                1, 5, 3, 3, 5, 7  // Back face
            };
            
            skyboxIndices.AddRange(cubeIndices);
            
            // Create skybox buffers
            _skyboxVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColor), skyboxVertices.Count, BufferUsage.WriteOnly);
            _skyboxVertexBuffer.SetData(skyboxVertices.ToArray());
            
            _skyboxIndexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), skyboxIndices.Count, BufferUsage.WriteOnly);
            _skyboxIndexBuffer.SetData(skyboxIndices.ToArray());
            _skyboxIndexCount = skyboxIndices.Count;
            
            _skyboxInitialized = true;
        }

        private void Initialize3DTerrain(World world)
        {
            if (_terrain3DInitialized) return;
            
            // Generate terrain vertices with proper normals for realistic lighting
            var terrainVertices = new List<VertexPositionNormalTexture>();
            var terrainIndices = new List<short>();
            
            int width = world.Width;
            int height = world.Height;
            float scale = 2.0f; // Larger scale for more dramatic terrain
            float heightScale = 0.5f; // More pronounced height differences
            
            // Create terrain vertices with calculated normals
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    float y = world.Heights[x, z] * heightScale;
                    
                    // Calculate normal by sampling neighboring heights
                    Vector3 normal = CalculateNormal(world, x, z, heightScale);
                    
                    // Create texture coordinates
                    Vector2 texCoord = new Vector2((float)x / width, (float)z / height);
                    
                    terrainVertices.Add(new VertexPositionNormalTexture(
                        new Vector3(x * scale, y, z * scale), 
                        normal, 
                        texCoord));
                }
            }
            
            // Create terrain indices for triangles
            for (int x = 0; x < width - 1; x++)
            {
                for (int z = 0; z < height - 1; z++)
                {
                    short topLeft = (short)(x * height + z);
                    short topRight = (short)((x + 1) * height + z);
                    short bottomLeft = (short)(x * height + (z + 1));
                    short bottomRight = (short)((x + 1) * height + (z + 1));
                    
                    // First triangle (counter-clockwise)
                    terrainIndices.Add(topLeft);
                    terrainIndices.Add(bottomLeft);
                    terrainIndices.Add(topRight);
                    
                    // Second triangle (counter-clockwise)
                    terrainIndices.Add(topRight);
                    terrainIndices.Add(bottomLeft);
                    terrainIndices.Add(bottomRight);
                }
            }
            
            // Create realistic water vertices with wave animation
            var waterVertices = new List<VertexPositionNormalTexture>();
            var waterIndices = new List<short>();
            float waterLevel = 2.0f; // Higher water level
            
            foreach (var river in world.Rivers)
            {
                // Create detailed water mesh with subdivisions for wave effects
                int subdivisions = 10;
                float stepX = (float)river.Width / subdivisions;
                float stepZ = (float)river.Height / subdivisions;
                
                int waterStartIndex = waterVertices.Count;
                
                for (int i = 0; i <= subdivisions; i++)
                {
                    for (int j = 0; j <= subdivisions; j++)
                    {
                        float x = (river.X + i * stepX) * scale;
                        float z = (river.Y + j * stepZ) * scale;
                        
                        Vector3 normal = Vector3.Up;
                        Vector2 texCoord = new Vector2((float)i / subdivisions, (float)j / subdivisions);
                        
                        waterVertices.Add(new VertexPositionNormalTexture(
                            new Vector3(x, waterLevel, z), 
                            normal, 
                            texCoord));
                    }
                }
                
                // Create water indices
                for (int i = 0; i < subdivisions; i++)
                {
                    for (int j = 0; j < subdivisions; j++)
                    {
                        short topLeft = (short)(waterStartIndex + i * (subdivisions + 1) + j);
                        short topRight = (short)(waterStartIndex + (i + 1) * (subdivisions + 1) + j);
                        short bottomLeft = (short)(waterStartIndex + i * (subdivisions + 1) + (j + 1));
                        short bottomRight = (short)(waterStartIndex + (i + 1) * (subdivisions + 1) + (j + 1));
                        
                        waterIndices.Add(topLeft);
                        waterIndices.Add(bottomLeft);
                        waterIndices.Add(topRight);
                        
                        waterIndices.Add(topRight);
                        waterIndices.Add(bottomLeft);
                        waterIndices.Add(bottomRight);
                    }
                }
            }
            
            // Create vertex and index buffers
            if (terrainVertices.Count > 0)
            {
                _terrainVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionNormalTexture), terrainVertices.Count, BufferUsage.WriteOnly);
                _terrainVertexBuffer.SetData(terrainVertices.ToArray());
                
                _terrainIndexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), terrainIndices.Count, BufferUsage.WriteOnly);
                _terrainIndexBuffer.SetData(terrainIndices.ToArray());
                _terrainIndexCount = terrainIndices.Count;
            }
            
            if (waterVertices.Count > 0)
            {
                _waterVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionNormalTexture), waterVertices.Count, BufferUsage.WriteOnly);
                _waterVertexBuffer.SetData(waterVertices.ToArray());
                
                _waterIndexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), waterIndices.Count, BufferUsage.WriteOnly);
                _waterIndexBuffer.SetData(waterIndices.ToArray());
                _waterIndexCount = waterIndices.Count;
            }
            
            _terrain3DInitialized = true;
        }

        private Vector3 CalculateNormal(World world, int x, int z, float heightScale)
        {
            // Sample neighboring heights to calculate normal
            float heightL = GetHeightSafe(world, x - 1, z) * heightScale;
            float heightR = GetHeightSafe(world, x + 1, z) * heightScale;
            float heightD = GetHeightSafe(world, x, z - 1) * heightScale;
            float heightU = GetHeightSafe(world, x, z + 1) * heightScale;
            
            Vector3 normal = new Vector3(heightL - heightR, 4.0f, heightD - heightU);
            normal.Normalize();
            return normal;
        }
        
        private float GetHeightSafe(World world, int x, int z)
        {
            if (x < 0 || x >= world.Width || z < 0 || z >= world.Height)
                return 0;
            return world.Heights[x, z];
        }

        public void DrawWorld3D(World world, Matrix view, Matrix projection)
        {
            Initialize3DTerrain(world);
            
            // Set up realistic rendering states
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            
            // Draw skybox first (without depth writing)
            DrawSkybox(view, projection);
            
            // Set up the effect matrices
            _basicEffect.World = Matrix.Identity;
            _basicEffect.View = view;
            _basicEffect.Projection = projection;
            
            // Draw terrain with realistic materials
            if (_terrainVertexBuffer != null)
            {
                _graphicsDevice.SetVertexBuffer(_terrainVertexBuffer);
                _graphicsDevice.Indices = _terrainIndexBuffer;
                
                // Set terrain material properties
                _basicEffect.DiffuseColor = new Vector3(0.4f, 0.6f, 0.2f); // Grass green
                _basicEffect.SpecularColor = new Vector3(0.1f, 0.1f, 0.1f); // Low specular
                _basicEffect.SpecularPower = 8.0f;
                
                foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _terrainIndexCount / 3);
                }
            }
            
            // Draw water with advanced transparency and reflection effects
            if (_waterVertexBuffer != null)
            {
                // Set up water rendering states
                _graphicsDevice.BlendState = BlendState.AlphaBlend;
                _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                
                _graphicsDevice.SetVertexBuffer(_waterVertexBuffer);
                _graphicsDevice.Indices = _waterIndexBuffer;
                
                // Animate water with time-based effects
                float time = (float)DateTime.Now.TimeOfDay.TotalSeconds;
                float waveHeight = 0.1f * (float)Math.Sin(time * 2.0f);
                
                // Set water material properties
                _basicEffect.DiffuseColor = new Vector3(0.1f, 0.3f, 0.8f); // Deep blue water
                _basicEffect.SpecularColor = new Vector3(0.8f, 0.8f, 1.0f); // High specular for reflection
                _basicEffect.SpecularPower = 128.0f;
                _basicEffect.Alpha = 0.7f; // Semi-transparent
                
                // Create wave effect by slightly modifying the world matrix
                Matrix waveMatrix = Matrix.CreateTranslation(0, waveHeight, 0);
                _basicEffect.World = waveMatrix;
                
                foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _waterIndexCount / 3);
                }
                
                // Reset states
                _graphicsDevice.BlendState = BlendState.Opaque;
                _graphicsDevice.DepthStencilState = DepthStencilState.Default;
                _basicEffect.World = Matrix.Identity;
                _basicEffect.Alpha = 1.0f;
            }
            
            // Draw additional 3D environment objects
            DrawEnvironmentObjects(world, view, projection);
            
            // Draw the player
            DrawPlayer3D(world, view, projection);
        }
        
        private void DrawPlayer3D(World world, Matrix view, Matrix projection)
        {
            // Note: For now, we'll skip drawing the 3D player model in first person
            // In a real FPS game, you might draw weapon models or other first-person elements
            return;
        }
        
        public void DrawRadar(SpriteBatch spriteBatch, World world, Player player)
        {
            if (_radarTexture == null) return;
            
            // RDR2-style radar position (bottom left)
            Vector2 radarPosition = new Vector2(20, _graphicsDevice.Viewport.Height - 140);
            
            // Draw radar background
            spriteBatch.Draw(_radarTexture, radarPosition, Color.White);
            
            // Draw player dot in center
            Vector2 radarCenter = radarPosition + new Vector2(60, 60);
            spriteBatch.Draw(_whitePixel, new Rectangle((int)radarCenter.X - 2, (int)radarCenter.Y - 2, 4, 4), Color.Yellow);
            
            // Draw terrain features and objects on radar
            float radarScale = 0.3f; // Scale factor for world to radar
            float radarRadius = 50f; // Radar detection radius
            
            // Draw nearby terrain features
            for (int x = -20; x <= 20; x += 2)
            {
                for (int z = -20; z <= 20; z += 2)
                {
                    int worldX = (int)(player.Position.X + x);
                    int worldZ = (int)(player.Position.Y + z);
                    
                    if (worldX >= 0 && worldX < world.Width && worldZ >= 0 && worldZ < world.Height)
                    {
                        float distance = (float)Math.Sqrt(x * x + z * z);
                        if (distance <= radarRadius)
                        {
                            Vector2 dotPosition = radarCenter + new Vector2(x * radarScale, z * radarScale);
                            
                            // Color based on terrain height
                            float height = world.Heights[worldX, worldZ];
                            Color terrainColor = Color.Green;
                            
                            if (height > 0.7f)
                                terrainColor = Color.Brown; // Mountains
                            else if (height < 0.3f)
                                terrainColor = Color.Blue; // Water
                            
                            spriteBatch.Draw(_whitePixel, new Rectangle((int)dotPosition.X, (int)dotPosition.Y, 1, 1), terrainColor * 0.6f);
                        }
                    }
                }
            }
            
            // Draw radar border
            int borderThickness = 2;
            Color borderColor = Color.Gray;
            
            // Top border
            spriteBatch.Draw(_whitePixel, new Rectangle((int)radarPosition.X, (int)radarPosition.Y, 120, borderThickness), borderColor);
            // Bottom border
            spriteBatch.Draw(_whitePixel, new Rectangle((int)radarPosition.X, (int)radarPosition.Y + 118, 120, borderThickness), borderColor);
            // Left border
            spriteBatch.Draw(_whitePixel, new Rectangle((int)radarPosition.X, (int)radarPosition.Y, borderThickness, 120), borderColor);
            // Right border
            spriteBatch.Draw(_whitePixel, new Rectangle((int)radarPosition.X + 118, (int)radarPosition.Y, borderThickness, 120), borderColor);
        }
        
        private void DrawSkybox(Matrix view, Matrix projection)
        {
            // Remove translation from view matrix for skybox
            Matrix skyboxView = view;
            skyboxView.Translation = Vector3.Zero;
            
            // Set up skybox rendering
            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            _graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            
            _skyboxEffect.World = Matrix.Identity;
            _skyboxEffect.View = skyboxView;
            _skyboxEffect.Projection = projection;
            
            _graphicsDevice.SetVertexBuffer(_skyboxVertexBuffer);
            _graphicsDevice.Indices = _skyboxIndexBuffer;
            
            foreach (EffectPass pass in _skyboxEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _skyboxIndexCount / 3);
            }
            
            // Reset states
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }
        
        private void DrawEnvironmentObjects(World world, Matrix view, Matrix projection)
        {
            // Draw 3D trees, rocks, and other environment objects
            var random = new Random(12345); // Fixed seed for consistent placement
            
            for (int i = 0; i < 50; i++) // Place 50 random objects
            {
                int x = random.Next(world.Width);
                int z = random.Next(world.Height);
                float height = world.Heights[x, z] * 0.5f;
                
                // Only place objects on land (not in water)
                bool isOnWater = false;
                foreach (var river in world.Rivers)
                {
                    if (river.Contains(x, z))
                    {
                        isOnWater = true;
                        break;
                    }
                }
                
                if (!isOnWater && height > 1.0f)
                {
                    // Create simple tree/rock object
                    Vector3 position = new Vector3(x * 2.0f, height + 1.0f, z * 2.0f);
                    DrawSimple3DObject(position, view, projection, random.Next(3));
                }
            }
        }
        
        private void DrawSimple3DObject(Vector3 position, Matrix view, Matrix projection, int type)
        {
            // Create simple 3D objects (trees, rocks) using basic shapes
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<short>();
            
            switch (type)
            {
                case 0: // Tree trunk
                    CreateCylinder(vertices, indices, 0.3f, 3.0f, 8);
                    break;
                case 1: // Rock
                    CreateCube(vertices, indices, 1.0f);
                    break;
                case 2: // Bush
                    CreateSphere(vertices, indices, 0.8f, 8);
                    break;
            }
            
            if (vertices.Count > 0)
            {
                // Create temporary buffers for this object
                var vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionNormalTexture), vertices.Count, BufferUsage.WriteOnly);
                vertexBuffer.SetData(vertices.ToArray());
                
                var indexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), indices.Count, BufferUsage.WriteOnly);
                indexBuffer.SetData(indices.ToArray());
                
                // Set up transformation
                _basicEffect.World = Matrix.CreateTranslation(position);
                _basicEffect.View = view;
                _basicEffect.Projection = projection;
                
                // Set material based on type
                switch (type)
                {
                    case 0: // Tree - brown
                        _basicEffect.DiffuseColor = new Vector3(0.4f, 0.2f, 0.1f);
                        break;
                    case 1: // Rock - gray
                        _basicEffect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
                        break;
                    case 2: // Bush - green
                        _basicEffect.DiffuseColor = new Vector3(0.2f, 0.4f, 0.1f);
                        break;
                }
                
                _graphicsDevice.SetVertexBuffer(vertexBuffer);
                _graphicsDevice.Indices = indexBuffer;
                
                foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indices.Count / 3);
                }
                
                // Clean up temporary buffers
                vertexBuffer.Dispose();
                indexBuffer.Dispose();
            }
        }
        
        private void CreateCylinder(List<VertexPositionNormalTexture> vertices, List<short> indices, float radius, float height, int segments)
        {
            int startIndex = vertices.Count;
            
            // Create cylinder vertices
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * MathHelper.TwoPi;
                float x = (float)Math.Cos(angle) * radius;
                float z = (float)Math.Sin(angle) * radius;
                
                // Bottom vertex
                vertices.Add(new VertexPositionNormalTexture(
                    new Vector3(x, 0, z),
                    Vector3.Normalize(new Vector3(x, 0, z)),
                    new Vector2((float)i / segments, 0)
                ));
                
                // Top vertex
                vertices.Add(new VertexPositionNormalTexture(
                    new Vector3(x, height, z),
                    Vector3.Normalize(new Vector3(x, 0, z)),
                    new Vector2((float)i / segments, 1)
                ));
            }
            
            // Create cylinder indices
            for (int i = 0; i < segments; i++)
            {
                short bottomLeft = (short)(startIndex + i * 2);
                short bottomRight = (short)(startIndex + (i + 1) * 2);
                short topLeft = (short)(startIndex + i * 2 + 1);
                short topRight = (short)(startIndex + (i + 1) * 2 + 1);
                
                // First triangle
                indices.Add(bottomLeft);
                indices.Add(topLeft);
                indices.Add(bottomRight);
                
                // Second triangle
                indices.Add(bottomRight);
                indices.Add(topLeft);
                indices.Add(topRight);
            }
        }
        
        private void CreateCube(List<VertexPositionNormalTexture> vertices, List<short> indices, float size)
        {
            int startIndex = vertices.Count;
            Vector3 half = Vector3.One * size * 0.5f;
            
            // Define cube vertices with normals
            Vector3[] positions = {
                new Vector3(-half.X, -half.Y, -half.Z), new Vector3(-half.X, -half.Y,  half.Z),
                new Vector3(-half.X,  half.Y, -half.Z), new Vector3(-half.X,  half.Y,  half.Z),
                new Vector3( half.X, -half.Y, -half.Z), new Vector3( half.X, -half.Y,  half.Z),
                new Vector3( half.X,  half.Y, -half.Z), new Vector3( half.X,  half.Y,  half.Z),
            };
            
            // Add vertices
            foreach (var pos in positions)
            {
                vertices.Add(new VertexPositionNormalTexture(pos, Vector3.Normalize(pos), Vector2.Zero));
            }
            
            // Define cube indices
            short[] cubeIndices = {
                (short)(startIndex + 0), (short)(startIndex + 2), (short)(startIndex + 1), (short)(startIndex + 1), (short)(startIndex + 2), (short)(startIndex + 3),
                (short)(startIndex + 4), (short)(startIndex + 5), (short)(startIndex + 6), (short)(startIndex + 5), (short)(startIndex + 7), (short)(startIndex + 6),
                (short)(startIndex + 0), (short)(startIndex + 1), (short)(startIndex + 5), (short)(startIndex + 0), (short)(startIndex + 5), (short)(startIndex + 4),
                (short)(startIndex + 2), (short)(startIndex + 6), (short)(startIndex + 7), (short)(startIndex + 2), (short)(startIndex + 7), (short)(startIndex + 3),
                (short)(startIndex + 0), (short)(startIndex + 4), (short)(startIndex + 6), (short)(startIndex + 0), (short)(startIndex + 6), (short)(startIndex + 2),
                (short)(startIndex + 1), (short)(startIndex + 3), (short)(startIndex + 7), (short)(startIndex + 1), (short)(startIndex + 7), (short)(startIndex + 5)
            };
            
            indices.AddRange(cubeIndices);
        }
        
        private void CreateCube(List<VertexPositionNormalTexture> vertices, List<short> indices, Vector3 size, Vector3 position)
        {
            int startVertex = vertices.Count;
            
            // Define the 8 vertices of a cube
            Vector3[] cubeVertices = {
                new Vector3(-size.X, -size.Y, -size.Z), // 0
                new Vector3(size.X, -size.Y, -size.Z),  // 1
                new Vector3(size.X, size.Y, -size.Z),   // 2
                new Vector3(-size.X, size.Y, -size.Z),  // 3
                new Vector3(-size.X, -size.Y, size.Z),  // 4
                new Vector3(size.X, -size.Y, size.Z),   // 5
                new Vector3(size.X, size.Y, size.Z),    // 6
                new Vector3(-size.X, size.Y, size.Z)    // 7
            };
            
            // Define normals for each face
            Vector3[] faceNormals = {
                new Vector3(0, 0, -1), // Front
                new Vector3(0, 0, 1),  // Back
                new Vector3(-1, 0, 0), // Left
                new Vector3(1, 0, 0),  // Right
                new Vector3(0, -1, 0), // Bottom
                new Vector3(0, 1, 0)   // Top
            };
            
            // Define texture coordinates
            Vector2[] texCoords = {
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0)
            };
            
            // Define face indices
            int[,] faceIndices = {
                {0, 1, 2, 3}, // Front
                {5, 4, 7, 6}, // Back
                {4, 0, 3, 7}, // Left
                {1, 5, 6, 2}, // Right
                {4, 5, 1, 0}, // Bottom
                {3, 2, 6, 7}  // Top
            };
            
            // Create vertices for each face
            for (int face = 0; face < 6; face++)
            {
                Vector3 normal = faceNormals[face];
                
                for (int i = 0; i < 4; i++)
                {
                    Vector3 vertex = cubeVertices[faceIndices[face, i]] + position;
                    vertices.Add(new VertexPositionNormalTexture(vertex, normal, texCoords[i]));
                }
                
                // Add indices for two triangles per face
                short baseIndex = (short)(startVertex + face * 4);
                indices.Add((short)(baseIndex + 0));
                indices.Add((short)(baseIndex + 1));
                indices.Add((short)(baseIndex + 2));
                
                indices.Add((short)(baseIndex + 0));
                indices.Add((short)(baseIndex + 2));
                indices.Add((short)(baseIndex + 3));
            }
        }
        
        private void CreateCube(List<VertexPositionNormalTexture> vertices, List<short> indices, float size, Vector3 position)
        {
            CreateCube(vertices, indices, new Vector3(size, size, size), position);
        }
        
        private void CreateSphere(List<VertexPositionNormalTexture> vertices, List<short> indices, float radius, int segments)
        {
            int startIndex = vertices.Count;
            
            // Create sphere vertices
            for (int i = 0; i <= segments; i++)
            {
                float phi = (float)i / segments * MathHelper.Pi;
                for (int j = 0; j <= segments; j++)
                {
                    float theta = (float)j / segments * MathHelper.TwoPi;
                    
                    Vector3 position = new Vector3(
                        radius * (float)(Math.Sin(phi) * Math.Cos(theta)),
                        radius * (float)Math.Cos(phi),
                        radius * (float)(Math.Sin(phi) * Math.Sin(theta))
                    );
                    
                    vertices.Add(new VertexPositionNormalTexture(
                        position,
                        Vector3.Normalize(position),
                        new Vector2((float)j / segments, (float)i / segments)
                    ));
                }
            }
            
            // Create sphere indices
            for (int i = 0; i < segments; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    short current = (short)(startIndex + i * (segments + 1) + j);
                    short next = (short)(startIndex + i * (segments + 1) + (j + 1));
                    short currentRow = (short)(startIndex + (i + 1) * (segments + 1) + j);
                    short nextRow = (short)(startIndex + (i + 1) * (segments + 1) + (j + 1));
                    
                    indices.Add(current);
                    indices.Add(currentRow);
                    indices.Add(next);
                    
                    indices.Add(next);
                    indices.Add(currentRow);
                    indices.Add(nextRow);
                }
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
