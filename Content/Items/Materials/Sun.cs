using Terraria.ID;

namespace TerraScience.Content.Items.Materials{
	public class Sun : BrazilOnTouchItem{
		public override string Texture => "Terraria/Images/SunOrb";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Sunlight");
		}

		public override void SetDefaults(){
			Item.width = 26;
			Item.height = 26;

			Item.maxStack = 999;

			Item.rare = ItemRarityID.Yellow;
		}
	}
}
