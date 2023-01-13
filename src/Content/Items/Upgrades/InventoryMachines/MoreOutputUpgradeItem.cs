using SerousEnergyLib.API.Upgrades;
using SerousEnergyLib.Items;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Upgrades.InventoryMachines;

namespace TerraScience.Content.Items.Upgrades.InventoryMachines {
	public class MoreOutputUpgradeItem : BaseUpgradeItem {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override BaseUpgrade Upgrade => ModContent.GetInstance<MoreOutputUpgrade>();

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
