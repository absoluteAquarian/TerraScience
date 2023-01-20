using SerousEnergyLib.API;
using SerousEnergyLib.API.Energy.Default;
using SerousEnergyLib.API.Fluid;
using SerousEnergyLib.API.Fluid.Default;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.MachineEntities;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Common.Systems {
	public class TechRecipes : ModSystem {
		public override void AddRecipeGroups() {
			RegisterRecipeGroup(nameof(ItemID.CopperBar), ItemID.CopperBar, ItemID.TinBar);
			RegisterRecipeGroup(nameof(ItemID.GoldBar), ItemID.GoldBar, ItemID.PlatinumBar);
		}

		public static void RegisterRecipeGroup(string groupName, params int[] validTypes) {
			int displayedItemType = validTypes[0];

			RecipeGroup.RegisterGroup(groupName, new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(displayedItemType)}", validTypes));
		}

		public override void AddRecipes() {
			Recipe.Create(ItemID.Torch, 8)
				.AddIngredient<Charcoal>(1)
				.AddRecipeGroup(RecipeGroupID.Wood, 1)
				.Register();

			LoaderUtils.ResetStaticMembers(typeof(Sets), true);

			// ===== Reinforced Furnace recipes =====
			Ticks woodToCharcoalMin = TechMod.Sets.ReinforcedFurnace.ConversionDuration.Min();
			Ticks woodToCharcoalMax = TechMod.Sets.ReinforcedFurnace.ConversionDuration.Max();

			Sets.ReinforcedFurnace.Add(new MachineRecipe<ReinforcedFurnace>()
				.AddRecipeGroup(RecipeGroupID.Wood, 1)
				.AddPossibleOutput<Charcoal>(1)
				.AddTimeVarianceRequirement(woodToCharcoalMin, woodToCharcoalMax)
				.CreateAndRegisterAllPossibleRecipes());

			// ===== Fluid Tank recipes =====
			for (int i = 0; i < ItemLoader.ItemCount; i++) {
				var recipe = new MachineRecipe<FluidTank>()
					.AddIngredient(i);

				int leftover = TechMod.Sets.FluidTank.FluidImportLeftover[i];

				if (leftover > ItemID.None)
					recipe.AddPossibleOutput(leftover);

				int fluid = TechMod.Sets.FluidTank.FluidImport[i];

				if (fluid <= FluidTypeID.None)
					continue;

				// TODO: vials might not have 1 L
				Sets.FluidTank.Add(recipe
					.AddPossibleFluidOutput(fluid, 1d)
					.AddTimeRequirement(new Ticks(1))
					.CreateAndRegisterAllPossibleRecipes());
			}

			for (int i = 0; i < FluidLoader.Count; i++) {
				int[] set = TechMod.Sets.FluidTank.FluidExportResult[i];

				if (set is null)
					continue;

				for (int input = 0; input < set.Length; input++) {
					int output = set[input];

					if (output <= ItemID.None)
						continue;

					double quantity = TechMod.Sets.FluidTank.FluidExportQuantity[input];

					if (quantity <= 0)
						continue;

					Sets.FluidTank.Add(new MachineRecipe<FluidTank>()
						.AddIngredient(input)
						.AddFluidIngredient(i, 1d)
						.AddPossibleOutput(output)
						.AddPossibleFluidOutput(i, quantity)
						.AddTimeRequirement(new Ticks(1))
						.CreateAndRegisterAllPossibleRecipes());
				}
			}

			// ===== Combustion Generator recipes =====
			for (int i = 0; i < ItemLoader.ItemCount; i++) {
				Ticks duration = TechMod.Sets.FurnaceGenerator.BurnDuration[i];

				if (duration <= 0)
					continue;

				double flux = FurnaceGeneratorEntity.ConstantGenerationPerTick * duration.ticks;

				Sets.FurnaceGenerator.Add(new MachineRecipe<FurnaceGenerator>()
					.AddIngredient(i)
					.AddPossiblePowerOutput<TerraFluxTypeID>((int)flux)
					.AddTimeRequirement(duration)
					.CreateAndRegisterAllPossibleRecipes());
			}
		}

		public override void Unload() {
			Sets.ReinforcedFurnace = null;
			Sets.FluidTank = null;
		}

		public static Item GetIngredientItem(Recipe recipe, int index) {
			if (recipe is null || index < 0 || index >= recipe.requiredItem.Count)
				return new Item();

			Item item = recipe.requiredItem[index].Clone();
			if (recipe.HasRecipeGroup(RecipeGroupID.Wood) && item.type == ItemID.Wood)
				item.SetNameOverride(Language.GetText("LegacyMisc.37").Value + " " + Lang.GetItemNameValue(ItemID.Wood));
			if (recipe.HasRecipeGroup(RecipeGroupID.Sand) && item.type == ItemID.SandBlock)
				item.SetNameOverride(Language.GetText("LegacyMisc.37").Value + " " + Lang.GetItemNameValue(ItemID.SandBlock));
			if (recipe.HasRecipeGroup(RecipeGroupID.IronBar) && item.type == ItemID.IronBar)
				item.SetNameOverride(Language.GetText("LegacyMisc.37").Value + " " + Lang.GetItemNameValue(ItemID.IronBar));
			if (recipe.HasRecipeGroup(RecipeGroupID.Fragment) && item.type == ItemID.FragmentSolar)
				item.SetNameOverride(Language.GetText("LegacyMisc.37").Value + " " + Language.GetText("LegacyMisc.51").Value);
			if (recipe.HasRecipeGroup(RecipeGroupID.PressurePlate) && item.type == ItemID.GrayPressurePlate)
				item.SetNameOverride(Language.GetText("LegacyMisc.37").Value + " " + Language.GetText("LegacyMisc.38").Value);
			if (ProcessGroupsForText(recipe, item.type, out string nameOverride))
				item.SetNameOverride(nameOverride);

			return item;
		}

		internal static bool ProcessGroupsForText(Recipe recipe, int type, out string theText) {
			foreach (int num in recipe.acceptedGroups) {
				if (RecipeGroup.recipeGroups[num].ContainsItem(type)) {
					theText = RecipeGroup.recipeGroups[num].GetText();
					return true;
				}
			}

			theText = "";
			return false;
		}

		public static class Sets {
			public static List<MachineRecipe> ReinforcedFurnace = new();
			public static List<MachineRecipe> FluidTank = new();
			public static List<MachineRecipe> FurnaceGenerator = new();
		}
	}
}
