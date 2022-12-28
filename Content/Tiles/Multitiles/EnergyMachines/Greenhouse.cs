using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Items.Placeable.Machines.Energy;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles.EnergyMachines{
	public class Greenhouse : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType){
			mapName = "Greenhouse";
			width = 2;
			height = 4;
			itemType = ModContent.ItemType<GreenhouseItem>();
		}

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<GreenhouseEntity>(this, pos, () => true);

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch){
			Point16 pos = new Point16(i, j);

			//Only draw if the TileCoord is (0, 0), otherwise we'll get weird effects like the things being drawn on top of other tiles
			if(MiscUtils.TryGetTileEntity(pos, out GreenhouseEntity entity)){
				Vector2 offset = MiscUtils.GetLightingDrawOffset();
				Vector2 drawPos = entity.Position.ToVector2() * 16 - Main.screenPosition + offset;

				Item input = entity.RetrieveItem(0);
				Item block = entity.RetrieveItem(1);
				Item modifier = entity.RetrieveItem(2);

				//Draw the ground
				string effect = null;
				switch(block.type){
					case ItemID.DirtBlock:
						if(modifier.IsAir)
							effect = "dirt";
						else if(modifier.type == ItemID.GrassSeeds)
							effect = "grasspurity";
						else if(modifier.type == ItemID.CorruptSeeds)
							effect = "grasscorrupt";
						else if(modifier.type == ItemID.CrimsonSeeds)
							effect = "grasscrimson";
						else if(modifier.type == ItemID.HallowedSeeds)
							effect = "grasshallow";
						else if(modifier.type == ItemID.JungleGrassSeeds)
							effect = "grassjungle";
						else if(modifier.type == ModContent.ItemType<Vial_Saltwater>())
							effect = "grassbeach";
						break;
					case ItemID.SandBlock:
						effect = "sand";
						break;
					case ItemID.SnowBlock:
						effect = "snow";
						break;
					case ItemID.MudBlock:
						if(modifier.type == ItemID.JungleGrassSeeds)
							effect = "grassjungle";
						else if(input.type == ItemID.MushroomGrassSeeds)
							effect = "grassmushroom";
						break;
					case ItemID.AshBlock:
						effect = "ash";
						break;
				}

				int growTileType = -1;
				Rectangle growFrame = Rectangle.Empty;

				switch(input.type){
					case ItemID.Acorn:
						growTileType = TileID.Saplings;

						int saplingX = -1;
						switch(effect){
							case "grasspurity":
								saplingX = 0;
								break;
							case "grasscorrupt":
								saplingX = 3;
								break;
							case "grasscrimson":
								saplingX = 4;
								break;
							case "snow":
								saplingX = 1;
								break;
							case "grassjungle":
								saplingX = 2;
								break;
							case "grasshallow":
								saplingX = 5;
								break;
							case "grassbeach":
								saplingX = 7;
								break;
						}
						
						growFrame = new Rectangle(18 * 3 * saplingX + entity.saplingRand * 18, 0, 16, 36);
						break;
					case ItemID.DaybloomSeeds:
					case ItemID.BlinkrootSeeds:
					case ItemID.WaterleafSeeds:
					case ItemID.MoonglowSeeds:
					case ItemID.DeathweedSeeds:
					case ItemID.ShiverthornSeeds:
					case ItemID.FireblossomSeeds:
						if(entity.ReactionProgress < 50f)
							growTileType = TileID.ImmatureHerbs;
						else if(entity.ReactionProgress < 90f)
							growTileType = TileID.MatureHerbs;
						else
							growTileType = TileID.BloomingHerbs;

						int herbX = input.type == ItemID.ShiverthornSeeds ? 6 : input.type - ItemID.DaybloomSeeds;

						growFrame = new Rectangle(18 * herbX, 0, 16, 20);

						break;
					case ItemID.MushroomGrassSeeds:
						growTileType = TileID.MushroomPlants;

						int mushX;
						if(entity.ReactionProgress < 50f)
							mushX = 1;
						else if(entity.ReactionProgress < 90f)
							mushX = 3;
						else
							mushX = 4;

						growFrame = new Rectangle(mushX * 18, 0, 16, 20);
						break;
					case ItemID.Cactus:
						growTileType = TileID.Cactus;

						int cactusY;
						if(entity.ReactionProgress < 50f)
							cactusY = 1;
						else if(entity.ReactionProgress < 90f)
							cactusY = 2;
						else
							cactusY = 3;

						growFrame = new Rectangle(2, 0, 12, 10 * cactusY);
						break;
					case ItemID.PumpkinSeed:
						growTileType = TileID.Pumpkins;

						int pumpkinX = (int)(entity.ReactionProgress / 20f);

						growFrame = new Rectangle(pumpkinX * 36, entity.saplingRand * 36, 36, 36);
						break;
				}
				
				Vector2 growBottom = drawPos + new Vector2(16, 34);

				var color = Lighting.GetColor(i, j);

				if(growTileType >= 0){
					Main.instance.LoadTiles(growTileType);

					var texture = TextureAssets.Tile[growTileType].Value;

					if(growTileType != TileID.Pumpkins){
						Vector2 growFrameOffset = new Vector2(growFrame.Width / 2f, growFrame.Height);

						float scale;
						if(growTileType == TileID.Saplings)
							scale = 0.7f;
						else if(growTileType == TileID.Cactus)
							scale = 0.6f;
						else
							scale = 1f;

						spriteBatch.Draw(texture, growBottom, growFrame, color, 0f, growFrameOffset, scale, SpriteEffects.None, 0);
					}else{
						//Pumpkins are 2x2, so their frames need to be stitched together
						float scale = 0.8f;
						Vector2 growFrameOffset = new Vector2(-8, -8) * scale;
						Vector2 scaleOrig = new Vector2(8, 16);
						
						spriteBatch.Draw(texture, growBottom + growFrameOffset, new Rectangle(growFrame.X, growFrame.Y, 16, 16), color, 0f, scaleOrig, scale, SpriteEffects.None, 0);
						growFrameOffset = new Vector2(8, -8) * scale;
						spriteBatch.Draw(texture, growBottom + growFrameOffset, new Rectangle(growFrame.X + 18, growFrame.Y, 16, 16), color, 0f, scaleOrig, scale, SpriteEffects.None, 0);
						growFrameOffset = new Vector2(-8, 0) * scale;
						spriteBatch.Draw(texture, growBottom + growFrameOffset, new Rectangle(growFrame.X, growFrame.Y + 18, 16, 16), color, 0f, scaleOrig, scale, SpriteEffects.None, 0);
						growFrameOffset = new Vector2(8, 0) * scale;
						spriteBatch.Draw(texture, growBottom + growFrameOffset, new Rectangle(growFrame.X + 18, growFrame.Y + 18, 16, 16), color, 0f, scaleOrig, scale, SpriteEffects.None, 0);
					}

					// TODO: networks aren't working anymore and any placed pump tiles disappear
				}

				if(effect != null)
					spriteBatch.Draw(this.GetEffectTexture(effect), drawPos, null, color);
			}

			return true;
		}
	}
}
