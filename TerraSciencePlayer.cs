using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items;

namespace TerraScience{
	public class TerraSciencePlayer : ModPlayer {
		public bool InventoryKeyPressed { get; private set; } = false;

		public static bool LocalPlayerHasAdmin => Main.LocalPlayer.GetModPlayer<TerraSciencePlayer>().tesseractAdmin;

		internal bool tesseractAdmin;

		public override void ProcessTriggers(TriggersSet triggersSet) {
			if(TechMod.DebugHotkey.JustPressed){
				TechMod.debugging = !TechMod.debugging;

				Main.NewText($"[TerraScience]: Debugging Turned {(TechMod.debugging ? "On" : "Off")}");
			}

			InventoryKeyPressed = PlayerInput.Triggers.JustPressed.Inventory;
		}

		public override void PostUpdate(){
			if(Player.HeldItem.ModItem is BrazilOnTouchItem){
				Player.HeldItem.type = ItemID.None;
				Player.HeldItem.stack = 0;
			}
		}
	}
}
