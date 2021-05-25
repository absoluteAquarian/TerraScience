using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Items{
	/// <summary>
	/// Items that should disappear from the world and player's inventory if interacted with
	/// </summary>
	public abstract class BrazilOnTouchItem : ModItem{
		public sealed override void HoldItem(Player player){
			item.type = ItemID.None;
			item.stack = 0;
		}

		public sealed override void Update(ref float gravity, ref float maxFallSpeed){
			item.type = ItemID.None;
			item.stack = 0;
		}

		public sealed override void UpdateInventory(Player player){
			item.type = ItemID.None;
			item.stack = 0;
		}
	}
}
