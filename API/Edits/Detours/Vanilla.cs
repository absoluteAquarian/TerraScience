using TerraScience.API.UI;

namespace TerraScience.API.Edits.Detours{
	public static partial class Vanilla{
		public static void Load(){
			On.Terraria.Player.PlaceThing += Player_PlaceThing;
			On.Terraria.Chest.AfterPlacement_Hook += Chest_AfterPlacement_Hook;
			On.Terraria.WorldGen.TileFrame += WorldGen_TileFrame;

/*			//I am going to kill whoever wrote the input-handling code
			On.Terraria.Main.DoUpdate_Enter_ToggleChat += orig => {
				if(UITextPrompt.AnyPromptHasFocus())
					return;

				orig();
			};*/
		}
	}
}
