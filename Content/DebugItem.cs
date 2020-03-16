using Terraria;
using Terraria.ModLoader;

namespace TerraScience.Content {
	public class DebugItem : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("null.jpg");
		}

		public override void SetDefaults() {
			item.width = 16;
			item.height = 16;
			item.useTime = 10;
			item.useAnimation = 10;
		}

		public override bool UseItem(Player player) {
			if (Main.tile[Player.tileTargetX, Player.tileTargetY].liquid == 0 || Main.tile[Player.tileTargetX, Player.tileTargetY].liquidType() == 0)
				ModContent.GetInstance<ScienceLiquidLoader>().mercuryLiquid?.SpawnLiquid(Player.tileTargetX, Player.tileTargetY, 255);
			return true;
		}
	}
}