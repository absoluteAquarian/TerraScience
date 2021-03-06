﻿using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items;

namespace TerraScience{
	public class TerraSciencePlayer : ModPlayer {
		public bool InventoryKeyPressed { get; private set; } = false;

		public override void ProcessTriggers(TriggersSet triggersSet) {
			if(TechMod.DebugHotkey.JustPressed){
				TechMod.debugging = !TechMod.debugging;

				Main.NewText($"[TerraScience]: Debugging Turned {(TechMod.debugging ? "On" : "Off")}");
			}

			InventoryKeyPressed = PlayerInput.Triggers.JustPressed.Inventory;
		}

		public override void PostUpdate(){
			if(player.HeldItem.modItem is BrazilOnTouchItem){
				player.HeldItem.type = ItemID.None;
				player.HeldItem.stack = 0;
			}
		}
	}
}
