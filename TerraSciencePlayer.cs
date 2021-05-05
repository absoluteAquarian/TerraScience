using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Utilities;

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
