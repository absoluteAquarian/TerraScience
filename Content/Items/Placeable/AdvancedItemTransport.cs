using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles;

namespace TerraScience.Content.Items.Placeable{
	public class AdvancedItemTransport : ItemTransport{
		public override void SetStaticDefaults(){
			base.SetStaticDefaults();
			DisplayName.SetDefault("Advanced Item Pipe");
		}

		public override void SetDefaults(){
			base.SetDefaults();
			Item.rare = ItemRarityID.Green;
			Item.createTile = ModContent.TileType<AdvancedItemTransportTile>();
			Item.value = Item.buyPrice(silver: 5);
		}

		public override void AddRecipes(){
			Recipe.Create(this.Type, 25)
				.AddRecipeGroup(TechMod.ScienceRecipeGroups.EvilBars, 1)
				.AddIngredient(ModContent.ItemType<ItemTransport>(), 25)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
