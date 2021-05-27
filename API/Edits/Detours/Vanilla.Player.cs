using Terraria;

namespace TerraScience.API.Edits.Detours{
	public static partial class Vanilla{
		private static void Player_PlaceThing(On.Terraria.Player.orig_PlaceThing orig, Player self){
			try{
				orig(self);
			}catch{ }

			TechMod.Instance.ResetNetworkTilesSolid();
		}
	}
}
