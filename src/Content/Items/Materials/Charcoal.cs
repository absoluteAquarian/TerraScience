using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Materials {
	public class Charcoal : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.Coal;

		public override void SetDefaults(){
			Item.width = 18;
			Item.height = 18;
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 999;
			Item.value = 10;
		}
	}
}
