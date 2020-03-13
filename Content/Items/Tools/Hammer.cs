using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Tools{
	public class Hammer : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Scientist's Hammer");
			Tooltip.SetDefault("Can be used to convert certain tile structures to multi-tile machines." +
				"\nAlso functions as a weak hammer.");
		}

		public override void SetDefaults(){
			item.damage = 20;
			item.knockBack = 7.2f;
			item.melee = true;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.UseSound = SoundID.Item1;
			item.useTime = 18;
			item.useAnimation = 18;
			item.useTurn = true;
			item.rare = ItemRarityID.Blue;
			item.hammer = 65;
			item.value = Item.sellPrice(silver: 6, copper: 33);
			item.width = 24;
			item.height = 24;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup("Wood", 6);
			recipe.AddRecipeGroup("IronBar", 4);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void HoldItem(Player player){
			//Terraria's inputs are fucking weird and 'player.altFunctionUse' isn't reliable (even with 'AltFunctionUse(Player player) => true;')
			//So, I opted to use 'Main.mouseRight && Main.mouseRightRelease' instead

			bool doThings = player.whoAmI == Main.myPlayer && Main.mouseRight && Main.mouseRightRelease;
			if(doThings){
				//Verify that the tile at the cursor is a valid structure tile and that the cursor isn't too far away from the player
				Point16 tilePos = (Main.MouseWorld / 16f).ToPoint16();
				int maxReach = player.lastTileRangeY * 16;
				if(TileUtils.TileIsViable(tilePos.X, tilePos.Y) && Vector2.DistanceSquared(player.Center, Main.MouseWorld) < maxReach * maxReach){
					//The tile is valid.  Call the utils method and then replace the tiles if the structure is there
					if(TileUtils.TryReplaceStructure(tilePos.X, tilePos.Y, out Point16 location, out Tile[,] structure)){
						//Main.NewText($"Structure at mouse position ({location.X}, {location.Y}) is valid!", Color.Orange);

						//Replace the tiles with the multitile
						int width = structure.GetLength(1);
						int height = structure.GetLength(0);
						for(int r = 0; r < height; r++){
							for(int c = 0; c < width; c++){
								Tile tile = Main.tile[location.X + c, location.Y + r];
								tile.type = (ushort)ModContent.TileType<SaltExtractor>();
								tile.active(true);
								tile.inActive(false);
								tile.frameX = (short)(c * 18);
								tile.frameY = (short)(r * 18);
							}
						}

						//Update the frames for the tiles
						WorldGen.RangeFrame(location.X, location.Y, location.X + width, location.X + height);
						//...and send a net message
						if(Main.netMode == NetmodeID.MultiplayerClient)
							NetMessage.SendTileRange(-1, location.X, location.Y, width, height);

						//Spawn the tile entity
						SaltExtractorEntity se = ModContent.GetInstance<SaltExtractorEntity>();
						if(se.Find(location.X, location.Y) < 0)
							se.Place(location.X, location.Y);
					}//else{
					//	Main.NewText("Structure wasn't valid.", Color.Orange);
					//}
				}//else{
				//	Main.NewText("Tile at mouse wasn't viable.", Color.Orange);
				//}
			}
		}
	}
}
