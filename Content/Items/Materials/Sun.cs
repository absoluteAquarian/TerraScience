using Terraria.ID;

namespace TerraScience.Content.Items.Materials{
	public class Sun : BrazilOnTouchItem{
		public override string Texture => "Terraria/SunOrb";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Sunlight");
		}

		public override void SetDefaults(){
			item.width = 26;
			item.height = 26;

			item.maxStack = 999;

			item.rare = ItemRarityID.Yellow;
		}
	}
}
