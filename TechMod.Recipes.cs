using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.API.CrossMod.MagicStorage;
using TerraScience.Content.Items.Energy;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Items.Placeable;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.Items.Placeable.Machines.Energy;
using TerraScience.Content.Items.Placeable.Machines.Energy.Generators;
using TerraScience.Content.Items.Placeable.Machines.Energy.Storage;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.Tiles;
using TerraScience.Utilities;

namespace TerraScience {
	public partial class TechMod : Mod{
		private void LoadMachineRecipes() {
			//This was previously in PostSetupContent, but recipe groups would be added too late...
			Logger.DebugFormat("Loading machine recipes...");

			DatalessMachineInfo.Register<SaltExtractorItem>(new RecipeIngredientSet()
				.AddIngredient(ItemID.Glass, 25)
				.AddIngredient<EmptyVial>(5)
				.AddIngredient(ItemID.Torch, 30)
				.AddRecipeGroup(RecipeGroupID.IronBar, 4));

			//Science workbench has a regular recipe, but its recipe set still needs to be added
			RecipeIngredientSet scienceWorkbenchSet = new RecipeIngredientSet()
				.AddRecipeGroup(RecipeGroupID.Wood, 20)
				.AddIngredient(ItemID.CopperBar, 5)
				.AddIngredient(ItemID.Glass, 8)
				.AddIngredient(ItemID.GrayBrick, 30);

			DatalessMachineInfo.recipes.Add(ModContent.ItemType<ScienceWorkbenchItem>(), mod => {
				ModRecipe recipe = new ModRecipe(mod);

				scienceWorkbenchSet.Apply(recipe);

				recipe.AddTile(TileID.WorkBenches);
				recipe.SetResult(ItemType("DatalessScienceWorkbench"), 1);
				recipe.AddRecipe();

				scienceWorkbenchSet.recipeIndex = recipe.RecipeIndex;
			});

			DatalessMachineInfo.recipeIngredients[ModContent.ItemType<ScienceWorkbenchItem>()] = scienceWorkbenchSet;

			DatalessMachineInfo.Register<ReinforcedFurnaceItem>(new RecipeIngredientSet()
				.AddIngredient(ItemID.GrayBrick, 40)
				.AddIngredient(ItemID.RedBrick, 40)
				.AddRecipeGroup(ScienceRecipeGroups.PreHmBarsTier1, 8));

			DatalessMachineInfo.Register<AirIonizerItem>(new RecipeIngredientSet()
				.AddRecipeGroup(ScienceRecipeGroups.PreHmBarsTier1, 20)
				.AddIngredient(ItemID.GrayBrick, 15)
				.AddIngredient<BasicMachineCore>());

			DatalessMachineInfo.Register<ElectrolyzerItem>(new RecipeIngredientSet()
				.AddRecipeGroup(RecipeGroupID.IronBar, 16)
				.AddIngredient(ItemID.Glass, 40)
				.AddIngredient<IronPipe>(2)
				.AddIngredient<BasicMachineCore>());

			DatalessMachineInfo.Register<BlastFurnaceItem>(new RecipeIngredientSet()
				.AddIngredient<BlastBrick>(50)
				.AddIngredient(ItemID.Furnace, 2));

			DatalessMachineInfo.Register<BasicWindTurbineItem>(new RecipeIngredientSet()
				.AddRecipeGroup(RecipeGroupID.IronBar, 15)
				.AddIngredient<WindmillSail>(4)
				.AddIngredient<BasicMachineCore>()
				.AddIngredient<TFWireItem>(10));

			DatalessMachineInfo.Register<BasicBatteryItem>(new RecipeIngredientSet()
				.AddRecipeGroup(RecipeGroupID.IronBar, 16)
				.AddIngredient(ItemID.CopperBar, 8)
				.AddIngredient(ItemID.TinBar, 8)
				.AddIngredient<BasicMachineCore>());

			DatalessMachineInfo.Register<AutoExtractinatorItem>(new RecipeIngredientSet()
				.AddIngredient<Silicon>(20)
				.AddIngredient(RecipeGroupID.IronBar, 16)
				.AddIngredient(ItemID.WaterBucket, 4)
				.AddIngredient(ItemID.Extractinator)
				.AddIngredient<BasicMachineCore>());

			DatalessMachineInfo.Register<BasicSolarPanelItem>(new RecipeIngredientSet()
				.AddIngredient(ItemID.Glass, 12)
				.AddIngredient<Silicon>(8)
				.AddRecipeGroup(RecipeGroupID.IronBar, 8)
				.AddIngredient<BasicMachineCore>()
				.AddIngredient<TFWireItem>(10));

			DatalessMachineInfo.Register<GreenhouseItem>(new RecipeIngredientSet()
				.AddIngredient(ItemID.Glass, 20)
				.AddRecipeGroup(RecipeGroupID.Wood, 50)
				.AddIngredient<BasicMachineCore>()
				.AddIngredient<TFWireItem>(4));

			DatalessMachineInfo.Register<BasicThermoGeneratorItem>(new RecipeIngredientSet()
				.AddIngredient(ItemID.RedBrick, 12)
				.AddIngredient(ItemID.Glass, 6)
				.AddRecipeGroup(ScienceRecipeGroups.PreHmBarsTier1, 2)
				.AddIngredient(ItemID.Torch, 10)
				.AddIngredient(ItemID.WaterBucket, 1)
				.AddIngredient<TFWireItem>(4));

			DatalessMachineInfo.Register<PulverizerItem>(new RecipeIngredientSet()
				.AddRecipeGroup(RecipeGroupID.IronBar, 10)
				.AddIngredient<IronPipe>(6)
				.AddIngredient<BasicMachineCore>()
				.AddIngredient<TFWireItem>(6));

			DatalessMachineInfo.Register<FluidTankItem>(new RecipeIngredientSet()
				.AddRecipeGroup(RecipeGroupID.IronBar, 12)
				.AddIngredient(ItemID.Glass, 20)
				.AddIngredient<FluidPump>(2));

			DatalessMachineInfo.Register<LiquidDuplicatorItem>(new RecipeIngredientSet()
				.AddRecipeGroup(RecipeGroupID.IronBar, 12)
				.AddIngredient(ItemID.OutletPump)
				.AddIngredient(ItemID.InletPump, 2)
				.AddIngredient<BasicMachineCore>()
				.AddIngredient<TFWireItem>(4));

			if(MagicStorageHandler.handler.ModIsActive)
				InitializeMagicStorageMachineRecipes();

			DatalessMachineInfo.Register<ItemCacheItem>(new RecipeIngredientSet()
				.AddRecipeGroup(RecipeGroupID.IronBar, 5)
				.AddRecipeGroup(ScienceRecipeGroups.Chest)
				.AddIngredient<ItemPump>(2));

			DatalessMachineInfo.Register<ComposterItem>(new RecipeIngredientSet()
				.AddRecipeGroup(RecipeGroupID.IronBar, 5)
				.AddIngredient(ItemID.Barrel)
				.AddIngredient<IronPipe>(3)
				.AddIngredient<BasicMachineCore>());

			//Loading merge data here instead of <tile>.SetDefaults()
			foreach(var type in types){
				if(type.IsAbstract)
					continue;

				if(typeof(JunctionMergeable).IsAssignableFrom(type)){
					int tileType = GetTile(type.Name).Type;

					foreach(var pair in TileUtils.tileToEntity){
						if(pair.Value is PoweredMachineEntity){
							Main.tileMerge[tileType][pair.Key] = true;
							Main.tileMerge[pair.Key][tileType] = true;
						}
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void InitializeMagicStorageMachineRecipes(){
			DatalessMachineInfo.Register<MagicStorageConnectorItem>(new RecipeIngredientSet()
				.AddIngredient(MagicStorageHandler.ItemType("StorageAccess"))
				.AddIngredient<Silicon>(30)
				.AddIngredient<ItemPump>(2));
		}
	}
}
