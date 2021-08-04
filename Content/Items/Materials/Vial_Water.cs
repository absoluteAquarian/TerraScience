using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Materials{
	public class Vial_Water : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Vial of Water");
			Tooltip.SetDefault("For use with the Salt Extractor");
		}

		public override void SetDefaults(){
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 1);
			TechMod.VialDefaults(Item);
		}
	}
}
