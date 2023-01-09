using SerousEnergyLib.API.CrossMod;
using SerousEnergyLib.Tiles;
using TerraScience.Content.Items.Machines;
using TerraScience.Content.MachineEntities;

namespace TerraScience.Content.Tiles.Machines {
	public class ReinforcedFurnace : BaseMachineTile<ReinforcedFurnaceEntity, ReinforcedFurnaceItem> {
		public override void GetMachineDimensions(out uint width, out uint height) {
			width = 3;
			height = 2;
		}

		public override MachineWorkbenchRegistry GetRegistry() {
			return new(Type,
				static tick => new MachineRegistryDisplayAnimationState("TerraScience/Assets/Machines/ReinforcedFurnace/Example_tile", 1, 1, 0, 0),
				static tick => new MachineRegistryDisplayAnimationState("TerraScience/Assets/Machines/ReinforcedFurnace/Example_anim", 1, 4, 0, tick % 24 / 6));
		}
	}
}
