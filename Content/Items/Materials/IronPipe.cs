using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Materials{
	public class IronPipe : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Iron Pipe");
		}

		public override void SetDefaults(){
			item.maxStack = 99;
			item.width = 28;
			item.height = 28;
			item.rare = ItemRarityID.White;
			item.value = Item.sellPrice(silver: 1, copper: 15);
		}

		public override bool CanUseItem(Player player) => false;

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup("IronBar", 1);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this, 2);
			recipe.AddRecipe();
		}
	}
}
