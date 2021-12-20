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
			item.rare = ItemRarityID.Blue;
			item.value = Item.sellPrice(silver: 1);
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
