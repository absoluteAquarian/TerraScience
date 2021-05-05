using Microsoft.Xna.Framework;
using Terraria;

namespace TerraScience.API.Classes.ModLiquid {
	public static class ModLiquidManager {
		internal static void RunInLiquidEvent(Rectangle liquidRect, ModLiquid.InLiquidEventHandler inLiquidEvent) {
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];

				if (player.getRect().Intersects(liquidRect)) {
					ModLiquid.InLiquidEventHandler handler = inLiquidEvent;
					handler?.Invoke(player);
				}
			}
		}
	}
}