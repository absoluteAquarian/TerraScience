using SerousEnergyLib.API.Machines.Default;
using SerousEnergyLib.API.Machines.UI;
using System;
using Terraria.ModLoader;
using TerraScience.Common.Systems;
using TerraScience.Common.UI.Machines;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.MachineEntities {
	public class MachineWorkbenchEntity : BaseInventoryEntity {
		public override int MachineTile => ModContent.TileType<MachineWorkbench>();

		public override BaseMachineUI MachineUI => MachineUISingletons.GetInstance<MachineWorkbenchEntity>();

		public override int DefaultInventoryCapacity => 1;

		public override int[] GetExportSlots() => Array.Empty<int>();

		public override int[] GetInputSlots() => Array.Empty<int>();

		public override int[] GetInputSlotsForRecipes() => GetInputSlots();

		public override void Load() {
			MachineUISingletons.RegisterUI<MachineWorkbenchEntity>(new MachineWorkbenchUI());
		}
	}
}
