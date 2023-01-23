using SerousEnergyLib.API.Fluid.Default;
using SerousEnergyLib.API.Fluid;
using SerousEnergyLib.API;
using SerousEnergyLib;
using TerraScience.API;
using Terraria.ID;

namespace TerraScience.Content.Tiles.Machines {
	partial class Greenhouse {
		private static void SetPlantInformation() {
			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.DirtBlock, ItemID.None, ItemID.BlinkrootSeeds,
				Ticks.FromSeconds(37),
				SerousMachines.FluidType<WaterFluidID>(), 0.02,
				new GreenhouseRecipeOutput(ItemID.Blinkroot, 1),
				new GreenhouseRecipeOutput(ItemID.BlinkrootSeeds, 1, 3, 0.333333)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.DirtBlock, ItemID.GrassSeeds, ItemID.Acorn,
				Ticks.FromSeconds(180),
				SerousMachines.FluidType<WaterFluidID>(), 0.1,
				new GreenhouseRecipeOutput(ItemID.Wood, 5, 12),
				// Chances taken from wiki page for Trees
				new GreenhouseRecipeOutput(ItemID.Acorn, 1, 2, chance: 0.141764),
				new GreenhouseRecipeOutput(ItemID.LivingWoodWand, 1, chance: 0.003333),
				new GreenhouseRecipeOutput(ItemID.LeafWand, 1, chance: 0.003322),
				new GreenhouseRecipeOutput(ItemID.Apple, 1, chance: 0.05533),
				new GreenhouseRecipeOutput(ItemID.Peach, 1, chance: 0.05533),
				new GreenhouseRecipeOutput(ItemID.Apricot, 1, chance: 0.05533),
				new GreenhouseRecipeOutput(ItemID.Grapefruit, 1, chance: 0.05533),
				new GreenhouseRecipeOutput(ItemID.Lemon, 1, chance: 0.05533)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.DirtBlock, ItemID.GrassSeeds, ItemID.PumpkinSeed,
				Ticks.FromSeconds(110),
				SerousMachines.FluidType<WaterFluidID>(), 0.08,
				new GreenhouseRecipeOutput(ItemID.Pumpkin, 5, 20),
				new GreenhouseRecipeOutput(ItemID.PumpkinSeed, 1, chance: 0.1)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.DirtBlock, ItemID.GrassSeeds, ItemID.DaybloomSeeds,
				Ticks.FromSeconds(30),
				SerousMachines.FluidType<WaterFluidID>(), 0.05,
				new GreenhouseRecipeOutput(ItemID.Daybloom, 1),
				new GreenhouseRecipeOutput(ItemID.DaybloomSeeds, 1, 3, chance: 0.333333)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.DirtBlock, ItemID.CorruptSeeds, ItemID.Acorn,
				Ticks.FromSeconds(180),
				SerousMachines.FluidType<WaterFluidID>(), 0.1,
				new GreenhouseRecipeOutput(ItemID.Ebonwood, 5, 12),
				// Chances taken from wiki page for Trees
				new GreenhouseRecipeOutput(ItemID.Elderberry, 1, chance: 0.07015),
				new GreenhouseRecipeOutput(ItemID.BlackCurrant, 1, chance: 0.07015)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.DirtBlock, ItemID.CorruptSeeds, ItemID.DeathweedSeeds,
				Ticks.FromSeconds(52),
				SerousMachines.FluidType<WaterFluidID>(), 0.05,
				new GreenhouseRecipeOutput(ItemID.Deathweed, 1),
				new GreenhouseRecipeOutput(ItemID.DeathweedSeeds, 1, 3, chance: 0.083333)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.DirtBlock, ItemID.CrimsonSeeds, ItemID.Acorn,
				Ticks.FromSeconds(180),
				SerousMachines.FluidType<WaterFluidID>(), 0.1,
				new GreenhouseRecipeOutput(ItemID.Ebonwood, 5, 12),
				// Chances taken from wiki page for Trees
				new GreenhouseRecipeOutput(ItemID.BloodOrange, 1, chance: 0.07015),
				new GreenhouseRecipeOutput(ItemID.Rambutan, 1, chance: 0.07015)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.DirtBlock, ItemID.CrimsonSeeds, ItemID.DeathweedSeeds,
				Ticks.FromSeconds(52),
				SerousMachines.FluidType<WaterFluidID>(), 0.05,
				new GreenhouseRecipeOutput(ItemID.Deathweed, 1),
				new GreenhouseRecipeOutput(ItemID.DeathweedSeeds, 1, 3, chance: 0.083333)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.DirtBlock, ItemID.HallowedSeeds, ItemID.Acorn,
				Ticks.FromSeconds(180),
				SerousMachines.FluidType<WaterFluidID>(), 0.1,
				new GreenhouseRecipeOutput(ItemID.Pearlwood, 5, 12),
				// Chances taken from wiki page for Trees
				new GreenhouseRecipeOutput(ItemID.Acorn, 1, 2, chance: 0.142857),
				new GreenhouseRecipeOutput(ItemID.Dragonfruit, 1, chance: 0.055757),
				new GreenhouseRecipeOutput(ItemID.Starfruit, 1, chance: 0.055757)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.SandBlock, ItemID.None, ItemID.Acorn,
				Ticks.FromSeconds(180),
				SerousMachines.FluidType<WaterFluidID>(), 0.1,
				new GreenhouseRecipeOutput(ItemID.PalmWood, 5, 12),
				// Chances taken from wiki page for Trees
				new GreenhouseRecipeOutput(ItemID.Coconut, 1, chance: 0.068941),
				new GreenhouseRecipeOutput(ItemID.Banana, 1, chance: 0.068941)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.SandBlock, ItemID.None, ItemID.Cactus,
				Ticks.FromSeconds(96),
				FluidTypeID.None, -1,
				new GreenhouseRecipeOutput(ItemID.Cactus, 1, 6),
				new GreenhouseRecipeOutput(ItemID.PinkPricklyPear, 1, chance: 0.015)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.SandBlock, ItemID.None, ItemID.WaterleafSeeds,
				Ticks.FromSeconds(28),
				SerousMachines.FluidType<WaterFluidID>(), 0.06,
				new GreenhouseRecipeOutput(ItemID.Waterleaf, 1),
				new GreenhouseRecipeOutput(ItemID.WaterleafSeeds, 1, 3, chance: 0.333333)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.MudBlock, ItemID.JungleGrassSeeds, ItemID.Acorn,
				Ticks.FromSeconds(180),
				SerousMachines.FluidType<WaterFluidID>(), 0.12,
				new GreenhouseRecipeOutput(ItemID.RichMahogany, 5, 12),
				// Chances taken from wiki page for Trees
				new GreenhouseRecipeOutput(ItemID.LivingMahoganyWand, 1, chance: 0.005),
				new GreenhouseRecipeOutput(ItemID.LivingMahoganyLeafWand, 1, chance: 0.004975),
				new GreenhouseRecipeOutput(ItemID.Mango, 1, chance: 0.066022),
				new GreenhouseRecipeOutput(ItemID.Pineapple, 1, chance: 0.066022)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.MudBlock, ItemID.JungleGrassSeeds, ItemID.BambooBlock,
				Ticks.FromSeconds(60),
				SerousMachines.FluidType<WaterFluidID>(), 0.025,
				new GreenhouseRecipeOutput(ItemID.BambooBlock, 1, 5)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.MudBlock, ItemID.JungleGrassSeeds, ItemID.MoonglowSeeds,
				Ticks.FromSeconds(40),
				SerousMachines.FluidType<WaterFluidID>(), 0.05,
				new GreenhouseRecipeOutput(ItemID.Moonglow, 1),
				new GreenhouseRecipeOutput(ItemID.MoonglowSeeds, 1, 3, chance: 0.333333)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.MudBlock, ItemID.MushroomGrassSeeds, ItemID.GlowingMushroom,
				Ticks.FromSeconds(18),
				SerousMachines.FluidType<WaterFluidID>(), 0.02,
				new GreenhouseRecipeOutput(ItemID.GlowingMushroom, 1),
				new GreenhouseRecipeOutput(ItemID.MushroomGrassSeeds, 1, chance: 0.125)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.SnowBlock, ItemID.None, ItemID.Acorn,
				Ticks.FromSeconds(180),
				SerousMachines.FluidType<WaterFluidID>(), 0.1,
				new GreenhouseRecipeOutput(ItemID.BorealWood, 5, 12),
				// Chances taken from wiki page for Trees
				new GreenhouseRecipeOutput(ItemID.Acorn, 1, 2, chance: 0.142857),
				new GreenhouseRecipeOutput(ItemID.Plum, 1, chance: 0.060425),
				new GreenhouseRecipeOutput(ItemID.Cherry, 1, chance: 0.060425)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.SnowBlock, ItemID.None, ItemID.ShiverthornSeeds,
				Ticks.FromSeconds(40),
				SerousMachines.FluidType<WaterFluidID>(), 0.05,
				new GreenhouseRecipeOutput(ItemID.Shiverthorn, 1),
				new GreenhouseRecipeOutput(ItemID.ShiverthornSeeds, 1, 3, chance: 0.333333)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.AshBlock, ItemID.None, ItemID.FireblossomSeeds,
				Ticks.FromSeconds(45),
				SerousMachines.FluidType<LavaFluidID>(), 0.05,
				new GreenhouseRecipeOutput(ItemID.Fireblossom, 1),
				new GreenhouseRecipeOutput(ItemID.FireblossomSeeds, 1, 3, chance: 0.333333)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.StoneBlock, ItemID.None, ItemID.GemTreeTopazSeed,
				Ticks.FromSeconds(480),
				FluidTypeID.None, -1,
				new GreenhouseRecipeOutput(ItemID.StoneBlock, 5, 18),
				new GreenhouseRecipeOutput(ItemID.Topaz, 1, 2),
				new GreenhouseRecipeOutput(ItemID.GemTreeTopazSeed, 1, 2, chance: 0.5)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.StoneBlock, ItemID.None, ItemID.GemTreeAmethystSeed,
				Ticks.FromSeconds(495),
				FluidTypeID.None, -1,
				new GreenhouseRecipeOutput(ItemID.StoneBlock, 5, 18),
				new GreenhouseRecipeOutput(ItemID.Amethyst, 1, 2),
				new GreenhouseRecipeOutput(ItemID.GemTreeAmethystSeed, 1, 2, chance: 0.5)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.StoneBlock, ItemID.None, ItemID.GemTreeSapphireSeed,
				Ticks.FromSeconds(510),
				FluidTypeID.None, -1,
				new GreenhouseRecipeOutput(ItemID.StoneBlock, 5, 18),
				new GreenhouseRecipeOutput(ItemID.Sapphire, 1, 2),
				new GreenhouseRecipeOutput(ItemID.GemTreeSapphireSeed, 1, 2, chance: 0.5)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.StoneBlock, ItemID.None, ItemID.GemTreeEmeraldSeed,
				Ticks.FromSeconds(515),
				FluidTypeID.None, -1,
				new GreenhouseRecipeOutput(ItemID.StoneBlock, 5, 18),
				new GreenhouseRecipeOutput(ItemID.Emerald, 1, 2),
				new GreenhouseRecipeOutput(ItemID.GemTreeEmeraldSeed, 1, 2, chance: 0.5)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.StoneBlock, ItemID.None, ItemID.GemTreeRubySeed,
				Ticks.FromSeconds(525),
				FluidTypeID.None, -1,
				new GreenhouseRecipeOutput(ItemID.StoneBlock, 5, 18),
				new GreenhouseRecipeOutput(ItemID.Ruby, 1, 2),
				new GreenhouseRecipeOutput(ItemID.GemTreeRubySeed, 1, 2, chance: 0.5)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.StoneBlock, ItemID.None, ItemID.GemTreeDiamondSeed,
				Ticks.FromSeconds(540),
				FluidTypeID.None, -1,
				new GreenhouseRecipeOutput(ItemID.StoneBlock, 5, 18),
				new GreenhouseRecipeOutput(ItemID.Diamond, 1, 2),
				new GreenhouseRecipeOutput(ItemID.GemTreeDiamondSeed, 1, 2, chance: 0.5)));

			TechMod.Sets.Greenhouse.AddInformation(new GreenhouseInputInformation(ItemID.StoneBlock, ItemID.None, ItemID.GemTreeAmberSeed,
				Ticks.FromSeconds(470),
				FluidTypeID.None, -1,
				new GreenhouseRecipeOutput(ItemID.StoneBlock, 5, 18),
				new GreenhouseRecipeOutput(ItemID.Amber, 1, 2),
				new GreenhouseRecipeOutput(ItemID.GemTreeAmberSeed, 1, 2, chance: 0.5)));
		}
	}
}
