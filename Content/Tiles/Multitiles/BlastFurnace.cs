using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles{
	public class BlastFurnace : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType){
			mapName = "Blast Furnace";
			width = 5;
			height = 5;
			itemType = ModContent.ItemType<BlastFurnaceItem>();
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b){
			Tile tile = Framing.GetTileSafely(i, j);
			if(MiscUtils.TryGetTileEntity(new Point16(i, j) - tile.TileCoord(), out BlastFurnaceEntity entity) && entity.ReactionInProgress){
				Vector3 color = new Vector3(0xD5, 0x44, 0x00) * 2.35f;
				r = color.X;
				g = color.Y;
				b = color.Z;
			}
		}

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<BlastFurnaceEntity>(this, pos, () => true);

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch){
			GetDefaultParams(out _, out uint width, out uint height, out _);

			Tile tile = Framing.GetTileSafely(i, j);
			Point16 frame = tile.TileCoord();
			Point16 pos = new Point16(i, j) - frame;
			bool lastTile = frame.X == width - 1 && frame.Y == height - 1;

			if(MiscUtils.TryGetTileEntity(pos, out BlastFurnaceEntity furnace) && lastTile){
				//If the UI is open, open the door and draw the rest of the things
				//Otherwise, just draw the closed door
				Vector2 offset = MiscUtils.GetLightingDrawOffset();
				Point drawPos = (furnace.Position.ToVector2() * 16 - Main.screenPosition + offset).ToPoint();
				Rectangle draw = new Rectangle(drawPos.X, drawPos.Y, 80, 80);

				if(furnace.ParentState?.Active ?? false){
					if(furnace.ReactionInProgress)
						spriteBatch.Draw(this.GetEffectTexture("fire"), draw, null, Color.White);

					//Opened door
					spriteBatch.Draw(this.GetEffectTexture("dooropen"), draw, null, Lighting.GetColor(i, j));
				}else{
					//Closed door
					spriteBatch.Draw(this.GetEffectTexture("doorclosed"), draw, null, Lighting.GetColor(i, j));
				}
			}
		}
	}
}
