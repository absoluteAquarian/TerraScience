using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Materials{
	public class IronPipe : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Iron Pipe");
		}

		public override void SetDefaults(){
			Item.maxStack = 99;
			Item.width = 28;
			Item.height = 28;
			Item.rare = ItemRarityID.White;
			Item.value = Item.sellPrice(silver: 1, copper: 15);
		}

		public override bool CanUseItem(Player player) => false;

		public override void AddRecipes(){
			Recipe.Create(this.Type, 2)
				.AddRecipeGroup("IronBar", 1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
