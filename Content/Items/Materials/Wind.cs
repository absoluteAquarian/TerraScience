using Terraria.ID;

namespace TerraScience.Content.Items.Materials{
	public class Wind : BrazilOnTouchItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Wind, Rain and Sandstorms");
		}

		public override void SetDefaults(){
			item.width = 30;
			item.height = 28;
			item.scale = 0.8f;

			item.maxStack = 999;

			item.rare = ItemRarityID.Blue;
		}
	}
}
