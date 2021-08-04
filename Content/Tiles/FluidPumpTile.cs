using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TerraScience.Content.Items.Placeable;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Systems;
using TerraScience.Systems.Pipes;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles{
	public class FluidPumpTile : JunctionMergeable{
		public override JunctionType MergeType => JunctionType.Fluids;

		public virtual float CapacityExtractedPerPump => 0.25f;

		public override void SafeSetDefaults(){
			//Having tile object data is REQUIRED for the tile.frameX and tile.frameY to be set BEFORE ModTile.TileFrame is called
			//This is an annoyance, but it's required for junction tiles to merge the surrounding junction-mergeable tiles
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.FlattenAnchors = false;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleWrapLimit = 16;
			TileObjectData.addTile(Type);

			AddMapEntry(Color.LightBlue);
			ItemDrop = ModContent.ItemType<FluidPump>();
		}

		public override void PlaceInWorld(int i, int j, Item item){
			base.PlaceInWorld(i, j, item);

			item.placeStyle %= 4;

			var tile = Framing.GetTileSafely(i, j);
			//Sanity check; TileObjectData should already handle this
			tile.frameX = (short)(item.placeStyle * 18);

			NetworkCollection.OnFluidPipePlace(new Point16(i, j));
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem){
			if(!fail){
				//Tile was mined.  Update the networks
				NetworkCollection.OnFluidPipeKill(new Point16(i, j));
			}
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) => false;

		internal static Point16 GetBackwardsOffset(Point16 orig){
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

		public static MachineEntity GetConnectedMachine(Point16 location){
			var actualLocation = GetBackwardsOffset(location);
			Tile tile = Framing.GetTileSafely(actualLocation);

			if(!(ModContent.GetModTile(tile.type) is Machine))
				return null;

			Point16 origin = actualLocation - tile.TileCoord();

			return TileEntity.ByPosition.TryGetValue(origin, out TileEntity entity) && entity is MachineEntity machineEntity && (machineEntity is ILiquidMachine || machineEntity is IGasMachine) ? machineEntity : null;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch){
			FluidTransportTile.DrawFluid(new Point16(i, j), ModContent.Request<Texture2D>("TerraScience/Content/Tiles/Effect_FluidPumpTile_fluid").Value, spriteBatch);

			//Pump draws itself
			return false;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch){
			//Essentially a copy of ItemPumpTile, but for fluids

			Point16 pos = new Point16(i, j);
			NetworkCollection.HasFluidPipeAt(pos, out FluidNetwork net);

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

			const float max = 34f;
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

			Texture2D tileTexture = TextureAssets.Tile[Type].Value;
			Texture2D texture = ModContent.Request<Texture2D>("TerraScience/Content/Tiles/Effect_FluidPumpTile_bar").Value;
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
