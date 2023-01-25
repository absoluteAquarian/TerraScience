using SerousEnergyLib.API;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.API.Upgrades;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace TerraScience.Content.Upgrades.InventoryMachines {
	public class MoreOutputUpgrade : BaseUpgrade {
		public override int MaxUpgradesPerMachine => 5;

		public override bool CanApplyTo(IMachine machine) => machine is IItemOutputGeneratorMachine generator && generator.Inventory?.Length > 0;

		public override int GetItemOutputGeneratorExtraStack(int upgradeStack, int originalStack) {
			// 5% multiplicative chance to add at most 10% of the output stack, with a minimum of 1 extra item
			float threshold = 0.95f;
			for (int i = 1; i < upgradeStack; i++)
				threshold *= 0.95f;

			if (Main.rand.NextFloat() > 1f - threshold)
				return 0;

			int maxExtra = (int)Math.Max(1, originalStack * 0.1f);

			if (maxExtra == 1)
				return 1;

			return Main.rand.Next(1, maxExtra);
		}

		public override StatModifier GetPowerConsumptionMultiplier(int upgradeStack) {
			// Slight increase in power consumption
			return new StatModifier(additive: 1f + 0.05f * upgradeStack, multiplicative: 1f);
		}

		public override void ModifyMachineRecipeIngredient(int upgradeStack, ref IMachineRecipeIngredient ingredient, IReadOnlyList<IMachineRecipeIngredient> defaultIngredients, IReadOnlyList<MachineRecipeOutput> possibleOutputs) {
			if (ingredient is MachineRecipeInputPower power)
				ingredient = new MachineRecipeInputPower(power.type, (int)GetPowerConsumptionMultiplier(upgradeStack).ApplyTo(power.amount));
		}
	}
}
