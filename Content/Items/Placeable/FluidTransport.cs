using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles;

namespace TerraScience.Content.Items.Placeable{
	public class FluidTransport : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Fluid Pipe");
			Tooltip.SetDefault("Transfers liquids or gases between machines");
		}

		public override void SetDefaults(){
			Item.width = 34;
			Item.height = 8;
			Item.scale = 0.75f;
			Item.rare = ItemRarityID.White;
			Item.maxStack = 999;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 10;
			Item.useAnimation = 15;
			Item.createTile = ModContent.TileType<FluidTransportTile>();
			Item.value = 5;
			Item.consumable = true;
			Item.autoReuse = true;
			Item.useTurn = true;
		}

		public override void AddRecipes(){
			Recipe.Create(this.Type, 25)
			.AddRecipeGroup(RecipeGroupID.IronBar, 1)
			.AddIngredient(ItemID.WaterBucket, 1)
			.AddTile(TileID.Anvils)
			.Register();
		}

		public override void OnCraft(Recipe recipe){
			Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_DropAsItem(), ItemID.EmptyBucket, 1);
		}
	}
}
