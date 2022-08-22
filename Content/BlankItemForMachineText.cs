using Terraria.ModLoader;

namespace TerraScience.Content{
	public class BlankItemForMachineText : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("null.jpg");
			Tooltip.SetDefault("Why do you have this?");
		}

		public override void SetDefaults(){
			Item.width = 16;
			Item.height = 16;
		}
	}
}