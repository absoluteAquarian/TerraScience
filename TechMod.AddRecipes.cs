using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.TileEntities;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Utilities;

namespace TerraScience {
    public partial class TechMod : Mod {
        public override void AddRecipes() {
            LoadMachineRecipes();

            //Electrolyzer recipes
            AddElectrolyzerRecipe(MachineFluidID.LiquidWater, MachineFluidID.HydrogenGas);
            AddElectrolyzerRecipe(MachineFluidID.LiquidWater, MachineFluidID.OxygenGas);
            AddElectrolyzerRecipe(MachineFluidID.LiquidSaltwater, MachineFluidID.HydrogenGas);
            AddElectrolyzerRecipe(MachineFluidID.LiquidSaltwater, MachineFluidID.ChlorineGas);


            //Blast furnace recipes
            foreach (var entry in BlastFurnaceEntity.ingredientToResult) {
                Recipe.Create(entry.Value.resultType, entry.Value.resultStack)
                    .AddIngredient(entry.Key, entry.Value.requireStack)
                    .AddTile(ModContent.TileType<BlastFurnace>())
                    .AddCondition(RecipeUtils.MadeAtMachine)
                    .Register();
            }

            //Matter Energizer recipes
            foreach (var entry in AirIonizerEntity.recipes) {
                Recipe.Create(entry.Value.resultType, entry.Value.resultStack)
                    .AddIngredient(entry.Key, entry.Value.requireStack)
                    .AddIngredient(ModContent.ItemType<TerraFluxIndicator>())
                    .AddCondition(RecipeUtils.MadeAtMachine)
                    .AddTile(ModContent.TileType<AirIonizer>())
                    .Register();
            }

            //Additional Extractinator recipes
            AddExtractinatorRecipe(ItemID.SandBlock, ModContent.ItemType<Silicon>());

            AddExtractinatorRecipe(ItemID.EbonsandBlock, ModContent.ItemType<Silicon>());
            AddExtractinatorRecipe(ItemID.EbonsandBlock, ItemID.CursedFlame);
            AddExtractinatorRecipe(ItemID.EbonsandBlock, ItemID.RottenChunk);
            AddExtractinatorRecipe(ItemID.EbonsandBlock, ItemID.VilePowder);
            AddExtractinatorRecipe(ItemID.EbonsandBlock, ItemID.SoulofNight);

            AddExtractinatorRecipe(ItemID.CrimsandBlock, ModContent.ItemType<Silicon>());
            AddExtractinatorRecipe(ItemID.CrimsandBlock, ItemID.Ichor);
            AddExtractinatorRecipe(ItemID.CrimsandBlock, ItemID.Vertebrae);
            AddExtractinatorRecipe(ItemID.CrimsandBlock, ItemID.ViciousPowder);
            AddExtractinatorRecipe(ItemID.CrimsandBlock, ItemID.SoulofNight);

            AddExtractinatorRecipe(ItemID.PearlsandBlock, ModContent.ItemType<Silicon>());
            AddExtractinatorRecipe(ItemID.PearlsandBlock, ItemID.CrystalShard);
            AddExtractinatorRecipe(ItemID.PearlsandBlock, ItemID.PixieDust);
            AddExtractinatorRecipe(ItemID.PearlsandBlock, ItemID.UnicornHorn);
            AddExtractinatorRecipe(ItemID.PearlsandBlock, ItemID.SoulofLight);

            //Greenhouse recipes
            AddGreenhouseRecipe(ItemID.Acorn, ItemID.DirtBlock, ItemID.GrassSeeds, ItemID.Wood);
            AddGreenhouseRecipe(ItemID.Acorn, ItemID.DirtBlock, ItemID.CorruptSeeds, ItemID.Ebonwood);
            AddGreenhouseRecipe(ItemID.Acorn, ItemID.DirtBlock, ItemID.CrimsonSeeds, ItemID.Shadewood);
            AddGreenhouseRecipe(ItemID.Acorn, ItemID.DirtBlock, ItemID.HallowedSeeds, ItemID.Pearlwood);
            AddGreenhouseRecipe(ItemID.Acorn, ItemID.DirtBlock, ModContent.ItemType<Vial_Saltwater>(), ItemID.PalmWood);
            AddGreenhouseRecipe(ItemID.Acorn, ItemID.SnowBlock, null, ItemID.BorealWood);
            AddGreenhouseRecipe(ItemID.Acorn, ItemID.MudBlock, ItemID.JungleGrassSeeds, ItemID.RichMahogany);

            AddGreenhouseRecipe(ItemID.DaybloomSeeds, ItemID.DirtBlock, ItemID.GrassSeeds, ItemID.Daybloom);
            AddGreenhouseRecipe(ItemID.BlinkrootSeeds, ItemID.DirtBlock, ItemID.GrassSeeds, ItemID.Blinkroot);
            AddGreenhouseRecipe(ItemID.ShiverthornSeeds, ItemID.SnowBlock, null, ItemID.Shiverthorn);
            AddGreenhouseRecipe(ItemID.WaterleafSeeds, ItemID.SandBlock, null, ItemID.Waterleaf);
            AddGreenhouseRecipe(ItemID.MoonglowSeeds, ItemID.MudBlock, ItemID.JungleGrassSeeds, ItemID.Moonglow);
            AddGreenhouseRecipe(ItemID.DeathweedSeeds, ItemID.DirtBlock, ItemID.CorruptSeeds, ItemID.Deathweed);
            AddGreenhouseRecipe(ItemID.DeathweedSeeds, ItemID.DirtBlock, ItemID.CrimsonSeeds, ItemID.Deathweed);
            AddGreenhouseRecipe(ItemID.Fireblossom, ItemID.AshBlock, null, ItemID.Fireblossom);

            AddGreenhouseRecipe(ItemID.MushroomGrassSeeds, ItemID.MudBlock, null, ItemID.GlowingMushroom);

            AddGreenhouseRecipe(ItemID.Cactus, ItemID.SandBlock, null, ItemID.Cactus);

            AddGreenhouseRecipe(ItemID.PumpkinSeed, ItemID.DirtBlock, ItemID.GrassSeeds, ItemID.Pumpkin);

            //Pulverizer recipes
            AddPulverizerEntry(inputItem: ItemID.DirtBlock,
                (ItemID.SandBlock, 1, 0.01),
                (ItemID.SiltBlock, 1, 0.01),
                (ItemID.StoneBlock, 1, 0.01),
                (ItemID.ClayBlock, 1, 0.01),
                (ItemID.GrassSeeds, 1, 0.015),
                (ItemID.DaybloomSeeds, 1, 0.015),
                (ItemID.BlinkrootSeeds, 1, 0.015),
                (ItemID.Acorn, 1, 0.05),
                (ItemID.Worm, 1, 0.125),
                (ItemID.EnchantedNightcrawler, 1, 0.0075));

            AddPulverizerEntry(inputItem: ItemID.StoneBlock,
                (ItemID.SandBlock, 1, 0.05),
                (ItemID.SiltBlock, 1, 0.025),
                (ItemID.CopperOre, 1, 0.01),
                (ItemID.TinOre, 1, 0.01),
                (ItemID.IronOre, 1, 0.005),
                (ItemID.LeadOre, 1, 0.005),
                (ItemID.SilverOre, 1, 0.002),
                (ItemID.SilverOre, 1, 0.002),
                (ItemID.GoldOre, 1, 0.001),
                (ItemID.PlatinumOre, 1, 0.001),
                (ItemID.Amethyst, 1, 0.01),
                (ItemID.Topaz, 1, 0.008),
                (ItemID.Sapphire, 1, 0.006),
                (ItemID.Emerald, 1, 0.005),
                (ItemID.Amber, 1, 0.005),
                (ItemID.Ruby, 1, 0.0025),
                (ItemID.Diamond, 1, 0.001));

            //Liquid duplicator recipes
            for (int i = 0; i < ItemLoader.ItemCount; i++) {
                MachineFluidID id = MiscUtils.GetFluidIDFromItem(i);

                if (id == MachineFluidID.None)
                    continue;

                int fakeItem = GetFakeIngredientType(id);

                Recipe recipe = Recipe.Create(fakeItem, 1)
                    .AddIngredient(i, 1)
                    .AddIngredient(ModContent.ItemType<TerraFluxIndicator>(), 1)
                    .AddTile(ModContent.TileType<LiquidDuplicator>())
                    .AddCondition(RecipeUtils.MadeAtMachine)
                    .Register();
            }

            AddComposterEntry(ItemID.Acorn, 4, 1);
            AddComposterEntry(ItemID.Seed, 20, 1);
            AddComposterEntry(ItemID.BlinkrootSeeds, 30, 1);
            AddComposterEntry(ItemID.CorruptSeeds, 32, 1);
            AddComposterEntry(ItemID.CrimsonSeeds, 32, 1);
            AddComposterEntry(ItemID.DaybloomSeeds, 24, 1);
            AddComposterEntry(ItemID.DeathweedSeeds, 10, 1);
            AddComposterEntry(ItemID.FireblossomSeeds, 8, 1);
            AddComposterEntry(ItemID.GrassSeeds, 45, 1);
            AddComposterEntry(ItemID.HallowedSeeds, 35, 1);
            AddComposterEntry(ItemID.JungleGrassSeeds, 30, 1);
            AddComposterEntry(ItemID.MoonglowSeeds, 24, 1);
            AddComposterEntry(ItemID.MushroomGrassSeeds, 10, 1);
            AddComposterEntry(ItemID.PumpkinSeed, 20, 1);
            AddComposterEntry(ItemID.ShiverthornSeeds, 30, 1);
            AddComposterEntry(ItemID.WaterleafSeeds, 24, 1);
            AddComposterEntry(ItemID.Blinkroot, 25, 1);
            AddComposterEntry(ItemID.Daybloom, 20, 1);
            AddComposterEntry(ItemID.Deathweed, 12, 1);
            AddComposterEntry(ItemID.Fireblossom, 20, 1);
            AddComposterEntry(ItemID.GlowingMushroom, 50, 1);
            AddComposterEntry(ItemID.Mushroom, 30, 1);
            AddComposterEntry(ItemID.Pumpkin, 100, 1);
            AddComposterEntry(ItemID.Shiverthorn, 30, 1);
            AddComposterEntry(ItemID.Waterleaf, 20, 1);
        }

        private void AddElectrolyzerRecipe(MachineFluidID input, MachineFluidID output) {
            Recipe recipe = Recipe.Create(GetFakeIngredientType(output), 1)
                .AddIngredient(GetFakeIngredientType(input), 1)
                .AddIngredient(ModContent.ItemType<TerraFluxIndicator>())
                .AddTile(ModContent.TileType<Electrolyzer>())
                .AddCondition(RecipeUtils.MadeAtMachine)
                .Register();
        }

        private void AddGreenhouseRecipe(int inputType, int blockType, int? modifierType, int resultType) {
            Recipe recipe = Recipe.Create(resultType, 1)
                .AddIngredient(inputType, 1)
                .AddIngredient(blockType, 1)
                .AddCondition(RecipeUtils.MadeAtMachine)
                .AddTile(ModContent.TileType<Greenhouse>());
            if (modifierType is int modifier)
                recipe.AddIngredient(modifier, 1);
            recipe.Register();
        }

        private void AddExtractinatorRecipe(int inputType, int outputType) {
            Recipe.Create(outputType)
                .AddIngredient(inputType)
                .AddTile(TileID.Extractinator)
                .Register();
            Recipe.Create(outputType)
                .AddIngredient(inputType)
                .AddIngredient(ModContent.ItemType<TerraFluxIndicator>())
                .AddTile(ModContent.TileType<AutoExtractinator>())
                .AddCondition(RecipeUtils.MadeAtMachine)
                .Register();
        }

        private void AddPulverizerEntry(int inputItem, params (int type, int stack, double weight)[] outputs) {
			var wRand = new WeightedRandom<(int type, int stack)>(Main.rand);

			double remaining = 1.0;
			foreach((int type, int stack, double weight) in outputs){
				wRand.Add((type, stack), weight);
				remaining -= weight;

				//Add a recipe for the thing
				Recipe.Create(type, stack)
				    .AddIngredient(inputItem)
				    .AddIngredient(ModContent.ItemType<TerraFluxIndicator>())
				    .AddTile(ModContent.TileType<Pulverizer>())
				    .AddCondition(RecipeUtils.MadeAtMachine)
				    .Register();
			}

			if(remaining > 0)
				wRand.Add((-1, 0), remaining);

			PulverizerEntity.inputToOutputs.Add(inputItem, wRand);        
        }

        private void AddComposterEntry(int inputItem, int inputStack, int resultStack) {
            Recipe.Create(ItemID.DirtBlock, resultStack)
                .AddIngredient(inputItem, inputStack)
                .AddIngredient(ModContent.ItemType<TerraFluxIndicator>())
                .AddTile(ModContent.TileType<Composter>())
                .Register();

            ComposterEntity.ingredients.Add((inputItem, (float)resultStack / inputStack));
        }
    }
}
