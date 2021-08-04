using Terraria;
using Terraria.ID;
using Terraria.Localization;
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
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience{
	public partial class TechMod{
		public const string RecipeDescription_MadeAtMachine = "This recipe displays results from a machine and cannot be used by the Crafting interface.";

		public override void PostSetupContent() {
			Logger.DebugFormat("Loading tile data and machine structures...");

			TileUtils.RegisterAllEntities();

			DatalessMachineInfo.Register<SaltExtractorItem>(new[]{
				(ItemID.Glass, 10),                    (ModContent.ItemType<MachineSupportItem>(), 5), (ItemID.Glass, 10),
				(ModContent.ItemType<EmptyVial>(), 3), (ItemID.Torch, 30),                             (ModContent.ItemType<EmptyVial>(), 3),
				(ItemID.IronBar, 2),                   (ModContent.ItemType<TFWireItem>(), 5),         (ItemID.IronBar, 2)
			});

			DatalessMachineInfo.recipes.Add(ModContent.ItemType<ScienceWorkbenchItem>(), mod => {
				Recipe recipe = mod.CreateRecipe(ModContent.ItemType<ScienceWorkbenchItem>(), 1);
				recipe.AddRecipeGroup(RecipeGroupID.Wood, 20);
				recipe.AddIngredient(ItemID.CopperBar, 5);
				recipe.AddIngredient(ItemID.Glass, 8);
				recipe.AddIngredient(ItemID.GrayBrick, 30);
				recipe.AddTile(TileID.WorkBenches);
				recipe.Register();
			});

			DatalessMachineInfo.Register<ReinforcedFurnaceItem>(new[]{
				(ItemID.GrayBrick, 5), (ItemID.RedBrick, 5), (ItemID.GrayBrick, 5),
				(ItemID.RedBrick, 5),  (ItemID.TinBar, 8),   (ItemID.RedBrick, 5),
				(ItemID.GrayBrick, 5), (ItemID.RedBrick, 5), (ItemID.GrayBrick, 5)
			});

			DatalessMachineInfo.Register<AirIonizerItem>(new[]{
				(ItemID.TinBar, 3),    (ItemID.TinBar, 3),                           (ItemID.TinBar, 3),
				(ItemID.TinBar, 3),    (ModContent.ItemType<BasicMachineCore>(), 1), (ItemID.TinBar, 3),
				(ItemID.GrayBrick, 8), (ItemID.GrayBrick, 8),                        (ItemID.GrayBrick, 8)
			});

			DatalessMachineInfo.Register<ElectrolyzerItem>(new[]{
				(ItemID.IronBar, 5), (ModContent.ItemType<IronPipe>(), 2),         (ItemID.IronBar, 5),
				(ItemID.Glass, 20),  (ModContent.ItemType<BasicMachineCore>(), 1), (ItemID.Glass, 20),
				(ItemID.IronBar, 5), (ItemID.IronBar, 5),                          (ItemID.IronBar, 5)
			});

			DatalessMachineInfo.Register<BlastFurnaceItem>(new[]{
				(ModContent.ItemType<BlastBrick>(), 5), (ModContent.ItemType<BlastBrick>(), 5), (ModContent.ItemType<BlastBrick>(), 5),
				(ModContent.ItemType<BlastBrick>(), 5), (ModContent.ItemType<BlastBrick>(), 5), (ModContent.ItemType<BlastBrick>(), 5),
				(ModContent.ItemType<BlastBrick>(), 5), (ModContent.ItemType<BlastBrick>(), 5), (ModContent.ItemType<BlastBrick>(), 5)
			});

			DatalessMachineInfo.Register<BasicWindTurbineItem>(new[]{
				(ModContent.ItemType<MachineSupportItem>(), 5), (ModContent.ItemType<WindmillSail>(), 4),     (ModContent.ItemType<MachineSupportItem>(), 5),
				(ItemID.IronBar, 6),                            (ModContent.ItemType<BasicMachineCore>(), 1), (ItemID.IronBar, 6),
				(ItemID.IronBar, 6),                            (ModContent.ItemType<TFWireItem>(), 10),      (ItemID.IronBar, 6)
			});
			DatalessMachineInfo.Register<BasicBatteryItem>(new[]{
				(ItemID.IronBar, 5),   (ItemID.CopperBar, 4),                        (ItemID.IronBar, 5),
				(ItemID.TinBar, 4),    (ModContent.ItemType<BasicMachineCore>(), 1), (ItemID.TinBar, 4),
				(ItemID.IronBar, 5),   (ItemID.CopperBar, 4),                        (ItemID.IronBar, 5)
			});

			DatalessMachineInfo.Register<AutoExtractinatorItem>(new[]{
				(ModContent.ItemType<Silicon>(), 10), (ItemID.IronBar, 8),                          (ModContent.ItemType<Silicon>(), 10),
				(ItemID.WaterBucket, 2),              (ItemID.Extractinator, 1),                    (ItemID.WaterBucket, 2),
				(ItemID.IronBar, 8),                  (ModContent.ItemType<BasicMachineCore>(), 1), (ItemID.IronBar, 8)
			});

			DatalessMachineInfo.Register<BasicSolarPanelItem>(new[]{
				(ItemID.Glass, 5),                    (ItemID.Glass, 8),                            (ItemID.Glass, 5),
				(ModContent.ItemType<Silicon>(), 5),  (ModContent.ItemType<BasicMachineCore>(), 1), (ModContent.ItemType<Silicon>(), 5),
				(ItemID.IronBar, 4),                  (ModContent.ItemType<TFWireItem>(), 10),      (ItemID.IronBar, 4)
			});

			DatalessMachineInfo.recipes.Add(ModContent.ItemType<GreenhouseItem>(), mod => {
				Recipe recipe = mod.CreateRecipe(mod.Find<ModItem>("DatalessGreenhouse").Type, 1);
				recipe.AddIngredient(ItemID.Glass, 4);
				recipe.AddIngredient(ItemID.Glass, 4);
				recipe.AddIngredient(ItemID.Glass, 4);

				recipe.AddIngredient(ItemID.Glass, 4);
				recipe.AddIngredient(ModContent.ItemType<BasicMachineCore>(), 1);
				recipe.AddIngredient(ItemID.Glass, 4);
					
				recipe.AddRecipeGroup(RecipeGroupID.Wood, 30);
				recipe.AddIngredient(ModContent.ItemType<TFWireItem>(), 8);
				recipe.AddRecipeGroup(RecipeGroupID.Wood, 30);

				recipe.AddTile(ModContent.TileType<ScienceWorkbench>());
				
				recipe.AddCondition(NetworkText.FromLiteral(RecipeDescription_MadeAtMachine), recipe => false);
				recipe.Register();
			});

			DatalessMachineInfo.Register<BasicThermoGeneratorItem>(new[]{
				(ItemID.Glass, 3),    (ItemID.CopperBar, 2),                  (ItemID.Glass, 4),
				(ItemID.RedBrick, 5), (ItemID.Torch, 10),                     (ItemID.WaterBucket, 3),
				(ItemID.RedBrick, 5), (ModContent.ItemType<TFWireItem>(), 6), (ItemID.GrayBrick, 3)
			});

			DatalessMachineInfo.Register<PulverizerItem>(new[]{
				(ItemID.IronBar, 2),                  (ModContent.ItemType<IronPipe>(), 1),         (ItemID.IronBar, 2),
				(ModContent.ItemType<IronPipe>(), 1), (ModContent.ItemType<BasicMachineCore>(), 1), (ModContent.ItemType<IronPipe>(), 1),
				(ItemID.IronBar, 2),                  (ModContent.ItemType<TFWireItem>(), 6),       (ItemID.IronBar, 2)
			});

			DatalessMachineInfo.Register<FluidTankItem>(new[]{
				(ItemID.IronBar, 5), (ModContent.ItemType<FluidPump>(), 1), (ItemID.IronBar, 5),
				(ItemID.Glass, 10),  (ItemID.IronBar, 5),                   (ItemID.Glass, 10),
				(ItemID.IronBar, 5), (ModContent.ItemType<FluidPump>(), 1), (ItemID.IronBar, 5)
			});

			DatalessMachineInfo.Register<LiquidDuplicatorItem>(new[]{
				(ItemID.IronBar, 2),   (ItemID.OutletPump, 1),                       (ItemID.IronBar, 2),
				(ItemID.InletPump, 1), (ModContent.ItemType<BasicMachineCore>(), 1), (ItemID.InletPump, 1),
				(ItemID.IronBar, 2),   (ModContent.ItemType<TFWireItem>(), 6),       (ItemID.IronBar, 2)
			});

			DatalessMachineInfo.recipes.Add(ModContent.ItemType<MagicStorageConnectorItem>(), mod => {
				if(!MagicStorageHandler.handler.ModIsActive)
					return;

				Recipe recipe = mod.CreateRecipe(mod.Find<ModItem>("DatalessMagicStorageConnector").Type, 1);
				recipe.AddIngredient(MagicStorageHandler.ItemType("StorageAccess"));
				recipe.AddIngredient(ModContent.ItemType<Silicon>(), 30);
				recipe.AddIngredient(ModContent.ItemType<ItemPump>(), 2);
				recipe.AddTile(TileID.Anvils);
				recipe.AddCondition(NetworkText.FromLiteral(RecipeDescription_MadeAtMachine), recipe => false);
				recipe.Register();
			});

			DatalessMachineInfo.Register<ItemCacheItem>(new[]{
				(ItemID.IronBar, 1),                  (ItemID.IronBar, 1), (ItemID.IronBar, 1),
				(ItemID.IronBar, 1),                  (ItemID.Chest, 1),   (ItemID.IronBar, 1),
				(ModContent.ItemType<ItemPump>(), 1), (ItemID.IronBar, 1), (ModContent.ItemType<ItemPump>(), 1)
			});

			DatalessMachineInfo.Register<ComposterItem>(new[]{
				(ItemID.IronBar, 1),                  (ModContent.ItemType<IronPipe>(), 1),           (ModContent.ItemType<MachineSupportItem>(), 1),
				(ModContent.ItemType<IronPipe>(), 1), (ModContent.ItemType<MachineSupportItem>(), 1), (ModContent.ItemType<IronPipe>(), 1),
				(ItemID.Barrel, 1),                   (ModContent.ItemType<MachineSupportItem>(), 1), (ItemID.IronBar, 1)
			});

			//Loading merge data here instead of <tile>.SetDefaults()
			foreach(var type in types){
				if(type.IsAbstract)
					continue;

				if(typeof(JunctionMergeable).IsAssignableFrom(type)){
					int tileType = Find<ModTile>(type.Name).Type;

					foreach(var pair in TileUtils.tileToEntity){
						if(pair.Value is PoweredMachineEntity){
							Main.tileMerge[tileType][pair.Key] = true;
							Main.tileMerge[pair.Key][tileType] = true;
						}
					}
				}
			}
		}
	}
}
