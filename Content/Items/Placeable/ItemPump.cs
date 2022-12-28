using Terraria.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles;

namespace TerraScience.Content.Items.Placeable{
	public class ItemPump : ModItem{
		static int style = 0;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Item Conveyor");
			Tooltip.SetDefault("Exports items from machines and chests into Item Pipes");
		}

		public override void SetDefaults(){
			Item.width = 34;
			Item.height = 8;
			Item.scale = 0.75f;
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 999;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 10;
			Item.useAnimation = 15;
			Item.createTile = ModContent.TileType<ItemPumpTile>();
			Item.value = 70;
			Item.consumable = true;
			Item.autoReuse = true;
			Item.useTurn = true;
		}

		public override void AddRecipes(){
			Recipe.Create(this.Type, 5)
				.AddIngredient(ModContent.ItemType<ItemTransport>(), 5)
				.AddRecipeGroup(RecipeGroupID.IronBar, 3)
				.AddIngredient(ItemID.Gel, 20)
				.AddTile(TileID.Anvils)
				.Register();
		}

		static uint lastUpdate = 0;
		public override void HoldItem(Player player){
			style %= 4;
			Item.placeStyle = style;

			if(lastUpdate == Main.GameUpdateCount)
				return;
			
			lastUpdate = Main.GameUpdateCount;

			if(!Main.blockMouse && player.inventory[58] != Item && Main.mouseRight && Main.mouseRightRelease){
				style = ++style % 4;

				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale){
			var texture = ModContent.Request<Texture2D>("TerraScience/Content/Items/Placeable/ItemPump_Sheet").Value;
			var source = texture.Frame(4, 1, Item.placeStyle, 0);

			spriteBatch.Draw(texture, position + origin, source, drawColor, 0f, origin, scale, SpriteEffects.None, 0);

			return false;
		}
	}
}
