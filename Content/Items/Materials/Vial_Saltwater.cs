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
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 1, copper: 15);
			Item.maxStack = 99;
			Item.width = 26;
			Item.height = 26;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 15;
			Item.useAnimation = 10;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.noMelee = true;
		}
	}
}
