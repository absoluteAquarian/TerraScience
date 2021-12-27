﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles;
using TerraScience.Systems;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Tools{
	public class DebugTool : ModItem{
		public override string Texture => "Terraria/Item_" + ItemID.IronPickaxe;

		public override bool CloneNewInstances => true;

		public override void SetStaticDefaults(){
			Tooltip.SetDefault("Displays debug information for Terraria Tech Mod" +
				"\nPress <KEY> to toggle world information" +
				"\nWhile holding, right click on one of the following to do something:" +
				"\n  [c/cccc00:Muffler:] Clear the \"placed mufflers\" list" +
				"\n  [c/cccc00:Any Item Pipe:] Force all items in the pipe's network to recalculate their movement paths" +
				"\n  [c/cccc00:Any Machine:] Reset the machine's entity to its default state" +
				"\n<NETWORK_COUNTS>" +
				"\n<TOTAL_ITEMS>" +
				"\n<NETWORK_TIMES>");
		}

		public override void SetDefaults(){
			item.CloneDefaults(ItemID.IronPickaxe);
			item.pick = 0;
			item.value = 0;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips){
			MiscUtils.FindAndModify(tooltips, "<KEY>", () => {
				var hotkeys = TechMod.DebugHotkey.GetAssignedKeys();

				return "\"" + (hotkeys.Count > 0 ? hotkeys[0] : "<NOT BOUND>") + "\"";
			});

			MiscUtils.FindAndInsertLines(tooltips, "<NETWORK_COUNTS>",
				() => "Network Counts:" +
				$"\n  Item Networks = {NetworkCollection.itemNetworks.Count} | Total Entries = {NetworkCollection.itemNetworks.Select(i => i.Hash.Count).Sum()}" +
				$"\n  Wire Networks = {NetworkCollection.wireNetworks.Count} | Total Entries = {NetworkCollection.wireNetworks.Select(i => i.Hash.Count).Sum()}" +
				$"\n  Fluid Networks = {NetworkCollection.fluidNetworks.Count} | Total Entries = {NetworkCollection.fluidNetworks.Select(i => i.Hash.Count).Sum()}");

			MiscUtils.FindAndInsertLines(tooltips, "<TOTAL_ITEMS>",
				() => $"Total Items in Networks: {NetworkCollection.itemNetworks.Select(i => i.paths).Select(list => list.Count).Sum()}");

			MiscUtils.FindAndInsertLines(tooltips, "<NETWORK_TIMES>",
				() => "Network Update Times:" +
				$"\n  Item Network Pumps: {NetworkCollection.ItemNetworkPumpUpdateTime * 1000 :0.###}ms ({NetworkCollection.ItemNetworkPumpUpdateTime * 60 :0.###} ticks)" +
				$"\n  Item Network Items: {NetworkCollection.ItemNetworkMovingItemsUpdateTime * 1000 :0.###}ms ({NetworkCollection.ItemNetworkMovingItemsUpdateTime * 60 :0.###} ticks)" +
				$"\n  Fluid Networks: {NetworkCollection.FluidNetworkUpdateTime * 1000 :0.###}ms ({NetworkCollection.FluidNetworkUpdateTime * 60 :0.###} ticks)");
		}

		static uint oldUpdate = 0;

		public override void HoldItem(Player player){
			if(TechMod.debugging && Main.mouseRight && Main.mouseRightRelease && player.inventory[58] != item){
				var pos = Main.MouseWorld.ToTileCoordinates16();
				var tile = Framing.GetTileSafely(pos.X, pos.Y);
				var mTile = ModContent.GetModTile(tile.type);

				if(mTile is MachineMufflerTile){
					MachineMufflerTile.mufflers.Clear();

					Main.NewText("Cleared muffler reference list");
					return;
				}

				if(mTile is ItemTransportTile && Systems.NetworkCollection.HasItemPipeAt(pos, out Systems.Pipes.ItemNetwork net)){
					for(int i = 0; i < net.paths.Count; i++){
						var path = net.paths[i];
						path.needsPathRefresh = true;
						path.delayPathCalc = false;
						path.wander = false;
					}

					Main.NewText("Force Item Network items to refresh their pathfinding");
					return;
				}

				if(!TileUtils.tileToEntity.ContainsKey(tile.type))
					return;

				pos -= tile.TileCoord();

				if(MiscUtils.TryGetTileEntity(pos, out MachineEntity entity)){
					//The default tag compound for an empty machine
					TagCompound tag = new TagCompound(){
						["machineInfo"] = new TagCompound(){
							["ReactionSpeed"] = 1f,
							["ReactionProgress"] = 0f,
							["ReactionInProgress"] = false
						},
						["slots"] = new TagCompound(){
							["items"] = null
						},
						["extra"] = null
					};

					entity.Load(tag);

					if(Main.GameUpdateCount != oldUpdate)
						Main.NewText($"Reset machine \"{entity.MachineName}\" at position {entity.Position}");

					oldUpdate = Main.GameUpdateCount;
				}
			}
		}
	}
}
