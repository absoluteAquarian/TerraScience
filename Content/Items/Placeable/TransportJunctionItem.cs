using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Tiles;
using Terraria.Audio;

namespace TerraScience.Content.Items.Placeable{
	public class TransportJunctionItem : ModItem{
		static int style = 0;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Junction");
			Tooltip.SetDefault("Allows wires, pipes and item transports from separate networks to go over each other without connecting" +
				"\nRight click while holding this to cycle forward through the different junction types" +
				"\n<>");
		}

		public override void SetDefaults(){
			Item.width = 16;
			Item.height = 16;
			Item.scale = 16f / 14f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 10;
			Item.useAnimation = 15;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(copper: 50);
			Item.consumable = true;
			Item.maxStack = 999;
			Item.createTile = ModContent.TileType<TransportJunction>();
			Item.placeStyle = style;
		}

		public override void AddRecipes(){
			CreateRecipe(10)
				.AddIngredient(ModContent.ItemType<IronPipe>(), 2)
				.AddRecipeGroup(RecipeGroupID.Wood, 10)
				.AddTile(TileID.WorkBenches)
				.Register();
		}

		internal static string display = null;
		internal static int displayTimer = -1;

		static uint lastUpdate = 0;
		public override void HoldItem(Player player){
			Item.placeStyle = style;

			if(lastUpdate == Main.GameUpdateCount)
				return;

			lastUpdate = Main.GameUpdateCount;

			if(!Main.blockMouse && player.inventory[58] != Item && Main.mouseRight && Main.mouseRightRelease){
				style = ++style % 16;

				SoundEngine.PlaySound(SoundID.MenuTick);

				if(Main.myPlayer == player.whoAmI){
					display = "Mode: " + GetModeText(JunctionMergeable.mergeTypes[style]);
					displayTimer = 75;
				}
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips){
			int index = tooltips.FindIndex(tl => tl.text == "<>");
			if(index >= 0)
				tooltips[index].text = $"[c/dddd00:Mode: {GetModeText(JunctionMergeable.mergeTypes[Item.placeStyle])}]";
		}

		private static string GetModeText(JunctionMerge mode){
			if(mode == JunctionMerge.None)
				return "None";

			if(mode == JunctionMerge.Wires_All)
				return "Wires";
			if(mode == JunctionMerge.Items_All)
				return "Items";
			if(mode == JunctionMerge.Fluids_All)
				return "Fluids";

			string text = "";
			if((mode & JunctionMerge.Wires_UpDown) != 0)
				text += "Wires Up & Down";
			if((mode & JunctionMerge.Items_UpDown) != 0)
				text += "Items Up & Down";
			if((mode & JunctionMerge.Fluids_UpDown) != 0)
				text += "Fluids Up & Down";

			if((mode & JunctionMerge.Wires_LeftRight) != 0){
				if(text != "")
					text += ", ";

				text += "Wires Left & Right";
			}
			if((mode & JunctionMerge.Items_LeftRight) != 0){
				if(text != "")
					text += ", ";

				text += "Items Left & Right";
			}
			if((mode & JunctionMerge.Fluids_LeftRight) != 0){
				if(text != "")
					text += ", ";

				text += "Fluids Left & Right";
			}

			return text;
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale){
			var texture = ModContent.Request<Texture2D>("TerraScience/Content/Items/Placeable/TransportJunctionItem_Sheet").Value;
			frame = texture.Frame(16, 1, Item.placeStyle, 0);
			spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0);
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI){
			var texture = ModContent.Request<Texture2D>("TerraScience/Content/Items/Placeable/TransportJunctionItem_Sheet").Value;
			Rectangle frame = texture.Frame(16, 1, Item.placeStyle, 0);
			spriteBatch.Draw(texture, Item.Center - Main.screenPosition, frame, lightColor, rotation, frame.Size() / 2f, scale, SpriteEffects.None, 0);
			return false;
		}
	}
}
