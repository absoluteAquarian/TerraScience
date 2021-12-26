using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TerraScience.API.CrossMod.MagicStorage;
using TerraScience.Content.Items.Placeable;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Systems;
using TerraScience.Systems.Pipes;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles{
	public class ItemPumpTile : JunctionMergeable{
		public override JunctionType MergeType => JunctionType.Items;

		public virtual int StackPerExtraction => 1;

		public override void SafeSetDefaults(){
			//Having tile object data is REQUIRED for the tile.frameX and tile.frameY to be set BEFORE ModTile.TileFrame is called
			//This is an annoyance, but it's required for junction tiles to merge the surrounding junction-mergeable tiles
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.FlattenAnchors = false;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleWrapLimit = 16;
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.addTile(Type);

			AddMapEntry(Color.LightGray);
			drop = ModContent.ItemType<ItemPump>();
		}

		public override void PlaceInWorld(int i, int j, Item item){
			base.PlaceInWorld(i, j, item);

			item.placeStyle %= 4;

			var tile = Framing.GetTileSafely(i, j);
			//Sanity check; TileObjectData should already handle this
			tile.frameX = (short)(item.placeStyle * 18);

			NetworkCollection.OnItemPipePlace(new Point16(i, j));
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem){
			if(!fail){
				//Tile was mined.  Update the networks
				NetworkCollection.OnItemPipeKill(new Point16(i, j));
			}
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) => false;

		internal Point16 GetBackwardsOffset(Point16 orig){
			Tile tile = Framing.GetTileSafely(orig);
			Point16 dir;
			switch(tile.frameX / 18){
				case 0:
					dir = new Point16(0, 1);
					break;
				case 1:
					dir = new Point16(1, 0);
					break;
				case 2:
					dir = new Point16(0, -1);
					break;
				case 3:
					dir = new Point16(-1, 0);
					break;
				default:
					throw new Exception($"Inner TerraScience error -- Unexpected pump tile frame (ID: {tile.frameX / 18})");
			}

			return orig + dir;
		}

		public MachineEntity GetConnectedMachine(Point16 location){
			var actualLocation = GetBackwardsOffset(location);
			Tile tile = Framing.GetTileSafely(actualLocation);

			if(!(ModContent.GetModTile(tile.type) is Machine))
				return null;

			Point16 origin = actualLocation - tile.TileCoord();

			return TileEntity.ByPosition.TryGetValue(origin, out TileEntity entity) && entity is MachineEntity machineEntity ? machineEntity : null;
		}

		public Chest GetConnectedChest(Point16 location){
			Point16 back = GetBackwardsOffset(location);
			int index = ChestUtils.FindChestByGuessingImproved(back.X, back.Y);

			return index > -1 ? Main.chest[index] : null;
		}

		public bool IsConnectedToMagicStorageAccess(Point16 location, out Point16 connectLocation){
			connectLocation = GetBackwardsOffset(location);

			return MagicStorageHandler.HasStorageHeartAt(connectLocation) || MagicStorageHandler.HasStorageAccessAt(connectLocation) || MagicStorageHandler.HasRemoteStorageAccessAt(connectLocation);
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch){
			ModContent.GetInstance<ItemTransportTile>().PreDraw(i, j, spriteBatch);

			//Pump draws itself if the config is enabled
			return !TechModConfig.Instance.AnimatePumps;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch){
			if(!TechModConfig.Instance.AnimatePumps)
				return;

			Point16 pos = new Point16(i, j);
			NetworkCollection.HasItemPipeAt(pos, out ItemNetwork net);

			//Uh oh
			if(net is null || !net.pumpTimers.TryGetValue(pos, out Timer pumpTimer))
				return;

			int timer = pumpTimer.value;

			/*  Graph:   Cosine                       Sine
			 *            _|_                          |   ___
			 *          .' | `.            '.          | .'   `.
			 *  _______/___|___\_______    __\_________|/_______\___
			 *        /    |    \             \       /|         \
			 *  '-__-'     |     '-__-'        '-___-' |          '-
			 */

			const float max = 18f;
			//Get the value on the graph for this sinusoidal movement
			float time = (timer - max / 4) / (max / 2);
			float radians = MathHelper.Pi * time;
			float sin = (float)Math.Sin(radians);

			//Move it up to above the X-axis
			sin += 1;

			//Then stretch it to fit the entire movement of the pump
			sin *= 8f / 2f;

			//Find the direction the offset needs to sway in
			Vector2 dir;
			int tileFrame = Framing.GetTileSafely(i, j).frameX / 18;
			switch(tileFrame){
				case 0:
					dir = new Vector2(0, -1);
					break;
				case 1:
					dir = new Vector2(-1, 0);
					break;
				case 2:
					dir = new Vector2(0, 1);
					break;
				case 3:
					dir = new Vector2(1, 0);
					break;
				default:
					throw new Exception($"Inner TerraScience error -- Unexpected pump tile frame (ID: {tileFrame})");
			}

			Texture2D tileTexture = Main.tileTexture[Type];
			Texture2D texture = ModContent.GetTexture("TerraScience/Content/Tiles/Effect_ItemPumpTile_bar");
			Rectangle tileSource = tileTexture.Frame(4, 1, tileFrame, 0);
			Rectangle frame = texture.Frame(4, 1, tileFrame, 0);
			frame.Width -= 2;
			frame.Height -= 2;

			Vector2 offset = MiscUtils.GetLightingDrawOffset();

			Vector2 tilePos = pos.ToVector2() * 16 - Main.screenPosition + offset;
			Vector2 drawPos = tilePos + dir * sin;
			var color = Lighting.GetColor(i, j);

			spriteBatch.Draw(tileTexture, tilePos, tileSource, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
			spriteBatch.Draw(texture, drawPos, frame, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
		}
	}
}
