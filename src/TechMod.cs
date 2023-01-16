using SerousEnergyLib;
using SerousEnergyLib.API;
using SerousEnergyLib.API.Fluid;
using SerousEnergyLib.API.Fluid.Default;
using SerousEnergyLib.Tiles;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.MachineEntities;

namespace TerraScience {
	public class TechMod : Mod {
		public static TechMod Instance => ModContent.GetInstance<TechMod>();

		public static string GetEffectPath<T>(string effect) where T : ModTile, IMachineTile {
			return $"TerraScience/Assets/Tiles/Machines/Effect_{typeof(T).Name}_{effect}";
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
				/// This array is index by <see cref="FluidTypeID"/>, then by item ID:<br/>
				/// <c>int result = FluidExportResult[fluid][item];</c>
				/// </summary>
				public static int[][] FluidExportResult = FluidFactory.CreateCustomSet(default(int[]),
					SerousMachines.FluidType<WaterFluidID>(), Factory.CreateIntSet(-1,
						ItemID.EmptyBucket, ItemID.WaterBucket),
					SerousMachines.FluidType<LavaFluidID>(), Factory.CreateIntSet(-1,
						ItemID.EmptyBucket, ItemID.LavaBucket),
					SerousMachines.FluidType<HoneyFluidID>(), Factory.CreateIntSet(-1,
						ItemID.EmptyBucket, ItemID.HoneyBucket));

				/// <summary>
				/// How much fluid should be exported when using an item
				/// </summary>
				public static double[] FluidExportQuantity = Factory.CreateCustomSet(-1d,
					ItemID.EmptyBucket, 1d);
			}
		}
	}
}
