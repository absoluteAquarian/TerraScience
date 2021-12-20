using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
			item.maxStack = 99;
			item.width = 26;
			item.height = 26;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTime = 15;
			item.useAnimation = 10;
			item.autoReuse = true;
			item.useTurn = true;
			item.noMelee = true;
		}
	}
}
