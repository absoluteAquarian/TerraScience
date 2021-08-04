using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Tools{
	public class DebugTool : ModItem{
		public override string Texture => "Terraria/Images/Item_" + ItemID.IronPickaxe;

		public override void SetDefaults(){
			Item.CloneDefaults(ItemID.IronPickaxe);
			Item.pick = 0;
			Item.value = 0;
		}

		static uint oldUpdate = 0;

		public override void HoldItem(Player player){
			if(TechMod.debugging && Main.mouseRight && Main.mouseRightRelease && player.inventory[58] != Item){
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
