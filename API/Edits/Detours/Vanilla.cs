namespace TerraScience.API.Edits.Detours{
	public static partial class Vanilla{
		public static void Load(){
			On.Terraria.Player.PlaceThing += Player_PlaceThing;
			On.Terraria.Chest.AfterPlacement_Hook += Chest_AfterPlacement_Hook;
			On.Terraria.WorldGen.TileFrame += WorldGen_TileFrame;
#if MERGEDTESTING
			On.Terraria.Audio.SoundEngine.PlayTrackedSound_SoundStyle += SoundEngine_PlayTrackedSound_SoundStyle;
			On.Terraria.Audio.SoundEngine.PlayTrackedSound_SoundStyle_Vector2 += SoundEngine_PlayTrackedSound_SoundStyle_Vector2;
			On.Terraria.Audio.SoundEngine.PlaySound_LegacySoundStyle_int_int += SoundEngine_PlaySound_LegacySoundStyle_int_int;
#endif
		}
	}
}
