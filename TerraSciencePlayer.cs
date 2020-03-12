using Terraria.GameInput;
using Terraria.ModLoader;

namespace TerraScience{
	public class TerraSciencePlayer : ModPlayer {
		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (TerraScience.DebugHotkey.JustPressed) {
				var terra = ModContent.GetInstance<TerraScience>();
				terra.saltExtracterLoader.ShowUI(terra.saltExtracterLoader.saltExtractorUI);
			}
		}
	}
}
