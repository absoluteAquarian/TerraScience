using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace TerraScience{
	public class TerraSciencePlayer : ModPlayer {
		public bool InventoryKeyPressed { get; private set; } = false;

		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (TerraScience.DebugHotkey.JustPressed) {
				//var terra = ModContent.GetInstance<TerraScience>();
			}

			InventoryKeyPressed = PlayerInput.Triggers.JustPressed.Inventory;
		}
	}
}
