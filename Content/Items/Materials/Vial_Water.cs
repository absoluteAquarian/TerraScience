using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Materials{
	public class Vial_Water : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Vial of Water");
			Tooltip.SetDefault("For use with the Salt Extractor");
		}

		public override void SetDefaults(){
			item.rare = ItemRarityID.Blue;
			item.value = Item.sellPrice(silver: 1);
			TerraScience.VialDefaults(item);
		}
	}
}
