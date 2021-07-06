namespace TerraScience.API.Edits.Detours{
	public static partial class Vanilla{
		public static void Load(){
			On.Terraria.Player.PlaceThing += Player_PlaceThing;
			On.Terraria.Chest.AfterPlacement_Hook += Chest_AfterPlacement_Hook;
			On.Terraria.WorldGen.TileFrame += WorldGen_TileFrame;
		}
	}
}
