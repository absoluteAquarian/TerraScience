namespace TerraScience.API.Edits.Detours{
	public static partial class Vanilla{
		private static void WorldGen_TileFrame(On.Terraria.WorldGen.orig_TileFrame orig, int i, int j, bool resetFrame, bool noBreak){
			TechMod.Instance.SetNetworkTilesSolid();

			try{
				orig(i, j, resetFrame, noBreak);
			}catch{ }

			TechMod.Instance.ResetNetworkTilesSolid();
		}
	}
}
