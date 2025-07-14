namespace Gamejamai.Engine
{
    public static class Assets
    {
        public static Microsoft.Xna.Framework.Graphics.Model HorseModel = null!;
        public static Microsoft.Xna.Framework.Graphics.Model HandsModel = null!;
        public static Microsoft.Xna.Framework.Graphics.Model GunModel = null!;
        public static Dictionary<string, Microsoft.Xna.Framework.Graphics.Texture2D> InventoryIcons = new();

        public static void Load(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            // Load the horse 3D model (ensure horse.obj is added to the .mgcb pipeline as "Models/horse")
            HorseModel = content.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/horse");

            // Load the FPS hands model (add your hands model as "Models/fps_hands")
            // HandsModel = content.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/fps_hands");

            // Load the gun model (Beretta_M9.obj as "Models/Beretta_M9")
            GunModel = content.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/Beretta_M9");

            // Load inventory icons (place .png files in Content/InventoryIcons and add to the .mgcb pipeline)
            // Example: InventoryIcons["Pistol"] = content.Load<Microsoft.Xna.Framework.Graphics.Texture2D>("InventoryIcons/pistol_icon");
        }
    }
}
