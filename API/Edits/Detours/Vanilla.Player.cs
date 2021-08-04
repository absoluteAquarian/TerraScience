using Terraria;
using TerraScience.World;

namespace TerraScience.API.Edits.Detours{
	public static partial class Vanilla{
		private static void Player_PlaceThing(On.Terraria.Player.orig_PlaceThing orig, Player self){
			TerraScienceWorld.SetNetworkTilesSolid();
			
			try{
				orig(self);
			}catch{ }

			TerraScienceWorld.ResetNetworkTilesSolid();
		}
	}
}
