using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles{
	public class ItemCache : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType){
			mapName = "Item Cache";
			width = 2;
			height = 2;
			itemType = ModContent.ItemType<ItemCacheItem>();
		}

		public override bool PreHandleMouse(Point16 pos){
			bool hasKey = Main.LocalPlayer.HeldItem.type == ItemID.GoldenKey;
			if(hasKey && MiscUtils.TryGetTileEntity(pos, out ItemCacheEntity entity)){
				entity.locked = !entity.locked;

				//Set the new locked item type
				if(!entity.locked)
					entity.lockItemType = ItemID.None;
				else
					entity.lockItemType = entity.RetrieveItem(-1).type;
			}

			return hasKey;
		}

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<ItemCacheEntity>(this, pos, () => Main.LocalPlayer.HeldItem.type != ItemID.GoldenKey);

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch){
			GetDefaultParams(out _, out uint width, out uint height, out _);

			Tile tile = Framing.GetTileSafely(i, j);
			Point16 frame = tile.TileCoord();
			Point16 pos = new Point16(i, j) - frame;
			bool lastTile = frame.X == width - 1 && frame.Y == height - 1;

			if(MiscUtils.TryGetTileEntity(pos, out ItemCacheEntity entity) && lastTile){
				Vector2 offset = MiscUtils.GetLightingDrawOffset();

				Vector2 drawPos = entity.Position.ToVector2() * 16 - Main.screenPosition + offset;

				//Draw the item in the little slot
				Vector2 itemOffset = new Vector2(16, 13);
				Vector2 itemArea = new Vector2(14, 10);

				Item topItem = entity.RetrieveItem(-1);

				if(!topItem.IsAir)
					spriteBatch.DrawItemInWorld(topItem, drawPos + itemOffset, itemArea);

				if(entity.locked)
					spriteBatch.Draw(this.GetEffectTexture("key"), drawPos, null, Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
			}
		}
	}
}
