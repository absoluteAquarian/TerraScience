using SerousEnergyLib;
using SerousEnergyLib.API;
using SerousEnergyLib.API.Fluid;
using SerousEnergyLib.API.Fluid.Default;
using SerousEnergyLib.Tiles;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.API;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.MachineEntities;

namespace TerraScience {
	public partial class TechMod : Mod {
		public static TechMod Instance => ModContent.GetInstance<TechMod>();

		public static string GetEffectPath<T>(string effect) where T : ModTile, IMachineTile {
			return $"TerraScience/Assets/Tiles/Machines/Effects/Effect_{typeof(T).Name}_{effect}";
		}

		public static string GetExamplePath<T>(string example) where T : ModTile, IMachineTile {
			return $"TerraScience/Assets/Machines/{typeof(T).Name}/Example_{example}";
		}

		public static class Sets {
			public static SetFactory Factory = new SetFactory(ItemLoader.ItemCount);
			public static SetFactory FluidFactory = new SetFactory(FluidLoader.Count);

			public static class ReinforcedFurnace {
				/// <summary>
				/// The minimum temperature in Celsius needed to start converting an input item
				/// </summary>
				public static double[] MinimumHeatForConversion = Factory.CreateCustomSet(-1d,
					ItemID.Wood, 300d,
					ItemID.BorealWood, 300d,
					ItemID.RichMahogany, 300d,
					ItemID.Ebonwood, 300d,
					ItemID.Shadewood, 300d,
					ItemID.PalmWood, 300d,
					ItemID.Pearlwood, 300d);

				/// <summary>
				/// The amount of game ticks it takes to convert one item, before applying speed bonuses
				/// </summary>
				public static Ticks[] ConversionDuration = Factory.CreateCustomSet(Ticks.FromSeconds(4),
					ItemID.BorealWood, Ticks.FromSeconds(5),
					ItemID.RichMahogany, Ticks.FromSeconds(3.5),
					ItemID.PalmWood, Ticks.FromSeconds(3.75));

				public static MachineSpriteEffectInformation[] ItemInFurnace = Factory.CreateCustomSet(default(MachineSpriteEffectInformation));
			}

			public static class FluidTank {
				// TODO: fluid vials?

				/// <summary>
				/// Whether an item can be placed in the fluid insertion input item slot
				/// </summary>
				public static bool[] CanBePlacedInFluidImportSlot = Factory.CreateBoolSet(false,
					ItemID.WaterBucket,
					ItemID.BottomlessBucket,
					ItemID.LavaBucket,
					ItemID.BottomlessLavaBucket,
					ItemID.HoneyBucket);
				// NOTE:  For futureproofing, the "can be placed" and "result" sets are separated.
				//        If an item can be used, but produces no leftover, then it should only have an entry in the "can be placed" set

				/// <summary>
				/// When an item from <see cref="CanBePlacedInFluidImportSlot"/> has its fluid "removed", this is the item type that will be placed in the fluid insertion output item slot
				/// </summary>
				public static int[] FluidImportLeftover = Factory.CreateIntSet(-1,
					ItemID.WaterBucket, ItemID.EmptyBucket,
					ItemID.BottomlessBucket, ItemID.BottomlessBucket,
					ItemID.LavaBucket, ItemID.EmptyBucket,
					ItemID.BottomlessLavaBucket, ItemID.BottomlessLavaBucket,
					ItemID.HoneyBucket, ItemID.EmptyBucket);

				/// <summary>
				/// The <see cref="FluidTypeID"/> that will be inserted into a <see cref="FluidTankEntity"/> when an item is used as input.<br/>
				/// Fluid import quantities are handled elsewhere.
				/// </summary>
				public static int[] FluidImport = Factory.CreateIntSet(FluidTypeID.None,
					ItemID.WaterBucket, SerousMachines.FluidType<WaterFluidID>(),
					ItemID.BottomlessBucket, SerousMachines.FluidType<WaterFluidID>(),
					ItemID.LavaBucket, SerousMachines.FluidType<LavaFluidID>(),
					ItemID.BottomlessLavaBucket, SerousMachines.FluidType<LavaFluidID>(),
					ItemID.HoneyBucket, SerousMachines.FluidType<HoneyFluidID>());

				/// <summary>
				/// Whether an item can be placed in the fluid extraction input item slot
				/// </summary>
				public static bool[] CanBePlacedInFluidExportSlot = Factory.CreateBoolSet(false,
					ItemID.EmptyBucket);

				/// <summary>
				/// When fluid is being extracted from a <see cref="FluidTankEntity"/>, this array dictates what the output will be.<br/>
				/// Indexed by <see cref="FluidTypeID"/>, then by item ID:<br/>
				/// <c>int result = FluidExportResult[fluid][item];</c>
				/// </summary>
				public static Dictionary<int, int[]> FluidExportResult = new() {
					[SerousMachines.FluidType<WaterFluidID>()] = Factory.CreateIntSet(-1,
						ItemID.EmptyBucket, ItemID.WaterBucket),
					[SerousMachines.FluidType<LavaFluidID>()] = Factory.CreateIntSet(-1,
						ItemID.EmptyBucket, ItemID.LavaBucket),
					[SerousMachines.FluidType<HoneyFluidID>()] = Factory.CreateIntSet(-1,
						ItemID.EmptyBucket, ItemID.HoneyBucket)
				};

				/// <summary>
				/// How much fluid should be exported when using an item
				/// </summary>
				public static double[] FluidExportQuantity = Factory.CreateCustomSet(-1d,
					ItemID.EmptyBucket, 1d);
			}

			public static class FurnaceGenerator {
				/// <summary>
				/// How long it takes for the Combustion Generator to burn an item
				/// </summary>
				public static Ticks[] BurnDuration = Factory.CreateCustomSet(Ticks.Zero,
					ItemID.Gel, Ticks.FromSeconds(0.75),
					ItemID.Wood, Ticks.FromSeconds(3),
					ItemID.BorealWood, Ticks.FromSeconds(3.25),
					ItemID.RichMahogany, Ticks.FromSeconds(2.8),
					ItemID.Ebonwood, Ticks.FromSeconds(3),
					ItemID.Shadewood, Ticks.FromSeconds(3),
					ItemID.PalmWood, Ticks.FromSeconds(2.9),
					ItemID.Pearlwood, Ticks.FromSeconds(3),
					ModContent.ItemType<Charcoal>(), Ticks.FromSeconds(6));
			}

			public static class Greenhouse {
				/// <summary>
				/// Whether an item can be used as the "soil" for a plant in a Greenhouse
				/// </summary>
				public static bool[] IsSoil = Factory.CreateBoolSet(false);

				/// <summary>
				/// Whether an item can be used as the "modifier" for a soil item in a Greenhouse
				/// </summary>
				public static bool[] IsSoilModifier = Factory.CreateBoolSet(false);

				/// <summary>
				/// Whether an item can be used as a "plant" in a Greenhouse
				/// </summary>
				public static bool[] IsPlant = Factory.CreateBoolSet(false);

				// TODO: modifier for dirt for farming Palm trees.  saltwater vial?
				/// <summary>
				/// Whether a given "soil" item from <see cref="IsSoil"/> can have an item modifier applied to it.<br/>
				/// The modifier affects the valid plant that can be placed on the soil as well as what items are generated from it.<br/>
				/// Indexed via:<br/>
				/// <c>bool allowed = SoilAllowsModifier[soil][modifier];</c>
				/// </summary>
				public static Dictionary<int, bool[]> SoilAllowsModifier = new();

				/// <summary>
				/// Whether a fluid can be used in a Greenhouse
				/// </summary>
				public static bool[] IsFluidPermitted = FluidFactory.CreateBoolSet(false);

				/// <summary>
				/// A collection of information regarding when a plant is allowed to grow.<br/>
				/// Indexed via:<br/>
				/// <c>var info = PlantGrowthInformation[soil][modifier][plant];</c>
				/// </summary>
				public static Dictionary<int, Dictionary<int, Dictionary<int, GreenhouseInputInformation>>> PlantGrowthInformation = new();

				/// <summary>
				/// A collection of information regarding how a plant is rendered.<br/>
				/// Indexed via:<br/>
				/// <c>var info = PlantEffect[soil][modifier][plant];</c>
				/// </summary>
				/// </summary>
				public static Dictionary<int, Dictionary<int, Dictionary<int, GreenhousePlantSpriteInformation>>> PlantEffect = new();

				/// <summary>
				/// The sprite used to render the soil and soil modifier
				/// </summary>
				public static Dictionary<int, MachineSpriteEffectInformation[]> SoilEffect = new();

				/// <summary>
				/// Updates many of the dictionaries in this set to include the information in <paramref name="information"/>
				/// </summary>
				/// <param name="information">The growth requirement information</param>
				public static void AddInformation(GreenhouseInputInformation information) {
					IsSoil[information.soil] = true;
					IsSoilModifier[information.modifier] = true;
					IsPlant[information.plant] = true;
					
					// SoilAllowsModifier
					if (!SoilAllowsModifier.TryGetValue(information.soil, out bool[] allowed))
						SoilAllowsModifier[information.soil] = allowed = Factory.CreateBoolSet(false);

					allowed[information.modifier] = true;

					// PlantGrowthInformation
					if (!PlantGrowthInformation.TryGetValue(information.soil, out var soilGrowthDict))
						PlantGrowthInformation[information.soil] = soilGrowthDict = new();

					if (!soilGrowthDict.TryGetValue(information.modifier, out var modifierGrowthDict))
						soilGrowthDict[information.modifier] = modifierGrowthDict = new();

					modifierGrowthDict[information.plant] = information;

					// IsFluidPermitted
					if (information.requiredFluid > FluidTypeID.None)
						IsFluidPermitted[information.requiredFluid] = true;
				}

				/// <summary>
				/// Adds a sprite entry to <see cref="SoilEffect"/>
				/// </summary>
				/// <param name="soil">The item ID for the soil item</param>
				/// <param name="modifier">The item ID for the modifier item</param>
				/// <param name="sprite">The sprite data</param>
				public static void AddSoilEffect(int soil, int modifier, MachineSpriteEffectInformation sprite) {
					if (!SoilEffect.TryGetValue(soil, out var infoArray))
						SoilEffect[soil] = infoArray = Factory.CreateCustomSet(default(MachineSpriteEffectInformation));

					infoArray[modifier] = sprite;
				}

				/// <summary>
				/// Adds a sprite information entry to <see cref="PlantEffect"/>
				/// </summary>
				/// <param name="sprite">The sprite data</param>
				public static void AddPlantEffect(GreenhousePlantSpriteInformation sprite) {
					if (!PlantEffect.TryGetValue(sprite.soil, out var soilDict))
						PlantEffect[sprite.soil] = soilDict = new();

					if (!soilDict.TryGetValue(sprite.modifier, out var modifierDict))
						soilDict[sprite.modifier] = modifierDict = new();

					modifierDict[sprite.plant] = sprite;
				}

				public static bool IsModifierValid(int soil, int modifier) => SoilAllowsModifier.TryGetValue(soil, out var allowed) && allowed[modifier];

				public static bool TryGetPlantInformation(int soil, int modifier, int plant, out GreenhouseInputInformation information) {
					information = default;

					return PlantGrowthInformation.TryGetValue(soil, out var soilDict)
						&& soilDict.TryGetValue(modifier, out var modifierDict)
						&& modifierDict.TryGetValue(plant, out information);
				}

				public static bool TryGetSoilSprite(int soil, int modifier, out MachineSpriteEffectInformation sprite) {
					sprite = default;

					if (SoilEffect.TryGetValue(soil, out var modifierArray)) {
						sprite = modifierArray[modifier];
						
						if (sprite.asset is null) {
							// Invalid sprite
							sprite = default;
							return false;
						}

						return true;
					}

					return false;
				}

				public static bool TryGetPlantSprites(int soil, int modifier, int plant, out GreenhousePlantSpriteInformation information) {
					information = default;

					return PlantEffect.TryGetValue(soil, out var soilDict)
						&& soilDict.TryGetValue(modifier, out var modifierDict)
						&& modifierDict.TryGetValue(plant, out information);
				}
			}
		}
	}
}
