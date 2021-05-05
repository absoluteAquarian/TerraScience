using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Tools{
	public class Crowbar : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crowbar");
			Tooltip.SetDefault("The perfect combination of crate-destroying and skull-smashing utility.");
		}

		public override void SetDefaults(){
			item.melee = true;
			item.width = 34;
			item.height = 46;
			item.damage = 30;
			item.knockBack = 4f;
			item.value = Item.sellPrice(silver: 11, copper: 25);
			item.rare = ItemRarityID.Blue;
			item.useTime = 18;
			item.useAnimation = 18;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
		}

		public override void AddRecipes(){
			RecipeUtils.SimpleRecipe("IronBar", 14, TileID.Anvils, this, 1);
		}
	}
}
