using SerousEnergyLib.API.Upgrades;
using SerousEnergyLib.Items;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Upgrades.PoweredMachines;

namespace TerraScience.Content.Items.Upgrades.PoweredMachines {
	public class IncreasedPowerStorageUpgradeItem : BaseUpgradeItem {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override BaseUpgrade Upgrade => ModContent.GetInstance<IncreasedPowerStorageUpgrade>();

		public override void SetDefaults() {
			base.SetDefaults();
			Item.width = 32;
			Item.height = 32;
			Item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes() {
			// TODO: add recipe for this upgrade
		}
	}
}
