using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Materials{
	public class Capsaicin : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Pepper");
		}

		public override void SetDefaults(){
			Item.width = 28;
			Item.height = 22;
			Item.scale = 0.6f;
			Item.rare = ItemRarityID.Blue;
			Item.value = 5;
			Item.maxStack = 999;
		}

		public override void AddRecipes(){
			// TODO: pepper plant
		}
	}
}
