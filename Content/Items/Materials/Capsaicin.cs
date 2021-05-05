using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Materials{
	public class Capsaicin : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Pepper");
		}

		public override void SetDefaults(){
			item.width = 28;
			item.height = 22;
			item.scale = 0.6f;
			item.rare = ItemRarityID.Blue;
			item.value = 5;
		}

		public override void AddRecipes(){
			// TODO: pepper plant
		}
	}
}
