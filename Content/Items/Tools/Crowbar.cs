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
			Item.DamageType = DamageClass.Melee;
			Item.width = 34;
			Item.height = 46;
			Item.damage = 30;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(silver: 11, copper: 25);
			Item.rare = ItemRarityID.Blue;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
		}

		public override void AddRecipes(){
			RecipeUtils.SimpleRecipe("IronBar", 14, TileID.Anvils, this, 1);
		}
	}
}
