using SerousEnergyLib.API.Machines;
using SerousEnergyLib.API.Machines.Default;
using SerousEnergyLib.API.Machines.UI;
using SerousEnergyLib.Systems;
using System;
using Terraria.ModLoader;
using TerraScience.Common.UI.Machines;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.MachineEntities {
	public class MachineWorkbenchEntity : BaseInventoryEntity, IMachineUIAutoloading<MachineWorkbenchEntity, MachineWorkbenchUI> {
		public override int MachineTile => ModContent.TileType<MachineWorkbench>();

		// BaseInventoryEntity usage requires overriding this, even though IMachineUIAutoloading<,> does this already
		public override BaseMachineUI MachineUI => MachineUISingletons.GetInstance<MachineWorkbenchEntity>();

		public override int DefaultInventoryCapacity => 1;

		public override int[] GetExportSlots() => Array.Empty<int>();

		public override int[] GetInputSlots() => Array.Empty<int>();

		public override int[] GetInputSlotsForRecipes() => GetInputSlots();
	}
}
