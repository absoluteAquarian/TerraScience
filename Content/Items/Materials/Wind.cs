using Terraria.ID;

namespace TerraScience.Content.Items.Materials{
	public class Wind : BrazilOnTouchItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Wind, Rain and Sandstorms");
		}

		public override void SetDefaults(){
			Item.width = 30;
			Item.height = 28;
			Item.scale = 0.8f;

			Item.maxStack = 999;

			Item.rare = ItemRarityID.Blue;
		}
	}
}
