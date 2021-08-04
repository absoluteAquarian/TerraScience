using TerraScience.World;

namespace TerraScience.API.Edits.Detours{
	public static partial class Vanilla{
		private static void WorldGen_TileFrame(On.Terraria.WorldGen.orig_TileFrame orig, int i, int j, bool resetFrame, bool noBreak){
			TerraScienceWorld.SetNetworkTilesSolid();

			try{
				orig(i, j, resetFrame, noBreak);
			}catch{ }

			TerraScienceWorld.ResetNetworkTilesSolid();
		}
	}
}
