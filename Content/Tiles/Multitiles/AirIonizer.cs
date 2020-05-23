using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles{
	public class AirIonizer : Machine{
		public override Tile[,] Structure => TileUtils.Structures.AirIonizer;

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<AirIonizerEntity>(this, pos, () => true);

		public override void GetDefaultParams(out string mapName, out uint width, out uint height){
			mapName = "Air Ionizer";
			width = 3;
			height = 3;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch){
			Point16 pos = new Point16(i, j);

			//Add some light and make it brighter if the UI is open
			//Only draw if the TileCoord is (0, 0), otherwise we'll get weird effects like the things being draw on top of other tiles
			if(MiscUtils.TryGetTileEntity(pos, out AirIonizerEntity ions)){
				//This code has been in a lot of places.  Oughta abstract it
				Vector2 offset = MiscUtils.GetLightingDrawOffset();
				Point drawPos = (ions.Position.ToVector2() * 16 - Main.screenPosition + offset).ToPoint();
				Rectangle draw = new Rectangle(drawPos.X, drawPos.Y, 48, 48);

				if(ions.ParentState?.Active ?? false)
					Lighting.AddLight(TileUtils.TileEntityCenter(ions, Structure) - new Vector2(0, 8), 0, 0.8f, 0.8f);
				else
					Lighting.AddLight(TileUtils.TileEntityCenter(ions, Structure) - new Vector2(0, 8), 0, 0.22f, 0.22f);

				//Draw the back texture
				spriteBatch.Draw(this.GetEffectTexture("machineback"), draw, null, Lighting.GetColor(i, j));

				//Random chance to draw either zappy 1 or zappy 2 if the charge is > 0
				if(ions.CurBatteryCharge > 0 && Main.rand.NextFloat() < 0.35f){
					if(Main.rand.NextBool())
						spriteBatch.Draw(this.GetEffectTexture("zappyzappy"), draw, null, Color.White * 0.3f);
					else
						spriteBatch.Draw(this.GetEffectTexture("zappyzappy2"), draw, null, Color.White * 0.3f);
				}
			}

			return true;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch){
			Tile tile = Framing.GetTileSafely(i, j);
			Point16 pos = new Point16(i, j) - tile.TileCoord();

			if(MiscUtils.TryGetTileEntity(pos, out AirIonizerEntity ions)){
				//This code has been in a lot of places.  Oughta abstract it
				Vector2 offset = MiscUtils.GetLightingDrawOffset();
				Point drawPos = (ions.Position.ToVector2() * 16 - Main.screenPosition + offset).ToPoint();
				Rectangle draw = new Rectangle(drawPos.X, drawPos.Y, 48, 48);

				//Draw the little window opened or closed
				if(ions.ParentState?.Active ?? false)
					spriteBatch.Draw(this.GetEffectTexture("windowopen"), draw, null, Lighting.GetColor(i, j));
				else
					spriteBatch.Draw(this.GetEffectTexture("windowclose"), draw, null, Lighting.GetColor(i, j));
			}
		}
	}
}
