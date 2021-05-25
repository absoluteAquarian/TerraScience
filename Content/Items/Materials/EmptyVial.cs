using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Materials{
	public class EmptyVial : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Empty Vial");
		}

		public override void SetDefaults(){
			item.rare = ItemRarityID.White;
			item.value = Item.sellPrice(copper: 80);
			TechMod.VialDefaults(item);
		}

		public override void AddRecipes(){
			RecipeUtils.SimpleRecipe(ItemID.Glass, 3, TileID.WorkBenches, this, 5);
		}

		public override bool UseItem(Player player){
			if(player.whoAmI != Main.myPlayer)
				return false;

			Tile tile = Framing.GetTileSafely(Main.MouseWorld.ToTileCoordinates());
			//If the tile has water and enough water.  Determine what kind of vial the player should get
			if((tile.bTileHeader & 159) == 0 && tile.liquid > 63){
				item.stack--;
				if(player.ZoneBeach)
					player.QuickSpawnItem(ModContent.ItemType<Vial_Saltwater>());
				else
					player.QuickSpawnItem(ModContent.ItemType<Vial_Water>());

				//Stuff happened
				return true;
			}

			//Stuff did not happen
			return false;
		}
	}
}
