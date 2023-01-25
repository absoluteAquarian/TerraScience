using SerousEnergyLib.API.Machines;
using SerousEnergyLib.API.Upgrades;
using Terraria.ModLoader;

namespace TerraScience.Content.Upgrades.PoweredMachines {
	// This basically mimics the upgrade from Mekanism, since it's designed well
	public class FasterProcessingUpgrade : BaseUpgrade {
		public override int MaxUpgradesPerMachine => 8;

		public override bool CanApplyTo(IMachine machine) => machine is IPoweredMachine and not IPowerGeneratorMachine;

		// Reference: https://github.com/mekanism/Mekanism/blob/6a61646d7a37060e5bca48550bb5670340edc6a7/src/main/java/mekanism/common/util/MekanismUtils.java#L287
		public override StatModifier GetProgressTicksMultiplier(int upgradeStack) {
			StatModifier modifier = StatModifier.Default;
			modifier *= (float)GetUpgradeFactor(upgradeStack, -1d);
			return modifier;
		}

		public override StatModifier GetProgressStepMultiplier(int upgradeStack) {
			// Mekanism uses smaller time = faster speed
			// However, Terran Automation does the inverse for this method.  Hence why "1d" is used instead of "-1d"
			StatModifier modifier = StatModifier.Default;
			modifier *= (float)GetUpgradeFactor(upgradeStack, 1d);
			return modifier;
		}

		// Reference: https://github.com/mekanism/Mekanism/blob/6a61646d7a37060e5bca48550bb5670340edc6a7/src/main/java/mekanism/common/util/MekanismUtils.java#L302
		public override StatModifier GetPowerConsumptionMultiplier(int upgradeStack) {
			StatModifier modifier = StatModifier.Default;
			modifier *= (float)GetUpgradeFactor(upgradeStack, 2d);
			return modifier;
		}
	}
}
