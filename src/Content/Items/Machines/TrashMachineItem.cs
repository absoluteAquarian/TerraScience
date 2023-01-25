using SerousEnergyLib.Items;
using Terraria;
using Terraria.ID;
using TerraScience.Content.Items.Networks.Fluids;
using TerraScience.Content.Items.Networks.Items;
using TerraScience.Content.Items.Networks.Power;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.Items.Machines {
	public class TrashMachineItem : BaseMachineItem<TrashMachine>, ICraftableMachineItem<CraftableTrashMachineItem> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override void SafeSetDefaults() {
			Item.width = 24;
			Item.height = 48;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(silver: 8, copper: 50);
		}
	}

	public class CraftableTrashMachineItem : DatalessMachineItem<TrashMachineItem, TrashMachine> {
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.TrashCan, 1)
				.AddIngredient<BasicItemTransportItem>(1)
				.AddIngredient<BasicFluidTransportItem>(1)
				.AddIngredient<BasicTerraFluxWireItem>(1)
				.AddTile<MachineWorkbench>()
				.Register();
		}
	}
}
