using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles;

namespace TerraScience.Content.Items.Placeable{
	public class FluidPump : ModItem{
		static int style = 0;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Fluid Pump");
			Tooltip.SetDefault("Exports liquids and gases from machines and chests into Fluid Pipes");
		}

		public override void SetDefaults(){
			item.width = 34;
			item.height = 8;
			item.scale = 0.75f;
			item.rare = ItemRarityID.Blue;
			item.maxStack = 999;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTime = 10;
			item.useAnimation = 15;
			item.createTile = ModContent.TileType<FluidPumpTile>();
			item.value = 70;
			item.consumable = true;
			item.autoReuse = true;
			item.useTurn = true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<FluidTransport>(), 5);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 3);
			recipe.AddIngredient(ItemID.Gel, 20);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this, 5);
			recipe.AddRecipe();
		}

		static uint lastUpdate = 0;
		public override void HoldItem(Player player){
			item.placeStyle = style;

			if(lastUpdate == Main.GameUpdateCount)
				return;

			lastUpdate = Main.GameUpdateCount;

			if(!Main.blockMouse && player.inventory[58] != item && Main.mouseRight && Main.mouseRightRelease){
				style = ++style % 4;

				Main.PlaySound(SoundID.MenuTick);
			}
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale){
			var texture = ModContent.GetTexture("TerraScience/Content/Items/Placeable/FluidPump_Sheet");
			var source = texture.Frame(4, 1, item.placeStyle, 0);

			spriteBatch.Draw(texture, position + origin, source, drawColor, 0f, origin, scale, SpriteEffects.None, 0);

			return false;
		}
	}
}
