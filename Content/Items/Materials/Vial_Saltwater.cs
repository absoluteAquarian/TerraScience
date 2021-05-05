using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Materials{
	public class Vial_Saltwater : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Vial of Saltwater");
			Tooltip.SetDefault("Seems like the Salt Extractor will be able to process this" +
				"\nmore efficiently than regular water...");
		}

		public override void SetDefaults(){
			item.rare = ItemRarityID.Blue;
			item.value = Item.sellPrice(silver: 1, copper: 15);
			TerraScience.VialDefaults(item);
		}
	}
}
