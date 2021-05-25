using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Tools{
	public class DebugTool : ModItem{
		public override string Texture => "Terraria/Item_" + ItemID.IronPickaxe;

		public override void SetDefaults(){
			item.CloneDefaults(ItemID.IronPickaxe);
			item.pick = 0;
			item.value = 0;
		}

		static uint oldUpdate = 0;

		public override void HoldItem(Player player){
			if(TechMod.debugging && Main.mouseRight && Main.mouseRightRelease && player.inventory[58] != item){
				var pos = Main.MouseWorld.ToTileCoordinates16();
				var tile = Framing.GetTileSafely(pos.X, pos.Y);

				if(tile.type == ModContent.TileType<MachineMufflerTile>()){
					MachineMufflerTile.mufflers.Clear();

					Main.NewText("Cleared muffler reference list");
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
