using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Gamejamai.Engine
{
    public class InventoryWheel
    {
        public bool IsVisible { get; private set; } = false;
        public int SelectedIndex { get; private set; } = 0;
        public List<string> Items = new List<string> { "Pistol", "Rifle", "Lasso", "Food" };

        public void Show() => IsVisible = true;
        public void Hide() => IsVisible = false;

        public void NextItem() { SelectedIndex = (SelectedIndex + 1) % Items.Count; }
        public void PrevItem() { SelectedIndex = (SelectedIndex - 1 + Items.Count) % Items.Count; }

        public void UseSelected(Player player)
        {
            // TODO: Implement item usage logic
        }

        public void Update(GameTime gameTime, Player player)
        {
            // Input handled in Game1 for showing/hiding and selection
        }
    }
}
