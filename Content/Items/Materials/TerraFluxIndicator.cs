using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Materials{
	public class TerraFluxIndicator : BrazilOnTouchItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Terra Flux (TF)");
			Tooltip.SetDefault("The energy unit that all powered Terran Automation machines run on");
		}

		public override void SetDefaults(){
			item.width = 36;
			item.height = 28;
			item.scale = 0.8f;

			item.maxStack = 999;

			item.rare = ItemRarityID.Red;
		}

		public override void AddRecipes(){
			//Solar panel recipes
			RecipeUtils.CreateTFRecipe<BasicSolarPanel>(ModContent.ItemType<Sun>());

			//Wind turbine recipes
			RecipeUtils.CreateTFRecipe<BasicWindTurbine>(ModContent.ItemType<Wind>());

			//Thermal Generator recipes
			RecipeUtils.CreateTFRecipeWithRecipeGroupIngredient<BasicThermoGenerator>(RecipeGroupID.Wood);

			for(int i = 0; i < ItemLoader.ItemCount; i++){
				Item thing = new Item();
				thing.SetDefaults(i);

				if(thing.buffType == BuffID.WellFed || thing.buffType == BuffID.Tipsy)
					RecipeUtils.CreateTFRecipe<BasicThermoGenerator>(i);
			}

			RecipeUtils.CreateTFRecipe<BasicThermoGenerator>(ModContent.ItemType<Coal>());
		}
	}
}
