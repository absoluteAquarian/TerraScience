using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Tiles;

namespace TerraScience.Content.Items.Placeable{
	public class TransportJunctionItem : ModItem{
		public override bool CloneNewInstances => true;

		static int style = 0;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Junction");
			Tooltip.SetDefault("Allows wires, pipes and item transports from separate networks to go over each other without connecting" +
				"\nRight click while holding this to cycle forward through the different junction types" +
				"\n<>");
		}

		public override void SetDefaults(){
			item.width = 16;
			item.height = 16;
			item.scale = 16f / 14f;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTime = 10;
			item.useAnimation = 15;
			item.useTurn = true;
			item.autoReuse = true;
			item.rare = ItemRarityID.Blue;
			item.value = Item.buyPrice(copper: 50);
			item.consumable = true;
			item.maxStack = 999;
			item.createTile = ModContent.TileType<TransportJunction>();
			item.placeStyle = style;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<IronPipe>(), 2);
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this, 10);
			recipe.AddRecipe();
		}

		internal static string display = null;
		internal static int displayTimer = -1;

		static uint lastUpdate = 0;
		public override void HoldItem(Player player){
			item.placeStyle = style;

			if(lastUpdate == Main.GameUpdateCount)
				return;

			lastUpdate = Main.GameUpdateCount;

			if(!Main.blockMouse && player.inventory[58] != item && Main.mouseRight && Main.mouseRightRelease){
				style = ++style % 16;

				Main.PlaySound(SoundID.MenuTick);

				if(Main.myPlayer == player.whoAmI){
					display = "Mode: " + GetModeText(JunctionMergeable.mergeTypes[style]);
					displayTimer = 75;
				}
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips){
			int index = tooltips.FindIndex(tl => tl.text == "<>");
			if(index >= 0)
				tooltips[index].text = $"[c/dddd00:Mode: {GetModeText(JunctionMergeable.mergeTypes[item.placeStyle])}]";
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
			var texture = ModContent.GetTexture("TerraScience/Content/Items/Placeable/TransportJunctionItem_Sheet");
			frame = texture.Frame(16, 1, item.placeStyle, 0);
			spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0);
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI){
			var texture = ModContent.GetTexture("TerraScience/Content/Items/Placeable/TransportJunctionItem_Sheet");
			Rectangle frame = texture.Frame(16, 1, item.placeStyle, 0);
			spriteBatch.Draw(texture, item.Center - Main.screenPosition, frame, lightColor, rotation, frame.Size() / 2f, scale, SpriteEffects.None, 0);
			return false;
		}
	}
}
