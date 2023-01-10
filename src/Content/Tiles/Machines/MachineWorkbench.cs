using SerousEnergyLib.API.CrossMod;
using SerousEnergyLib.Tiles;
using TerraScience.Content.Items.Machines;
using TerraScience.Content.MachineEntities;

namespace TerraScience.Content.Tiles.Machines {
	public class MachineWorkbench : BaseMachineTile<MachineWorkbenchEntity, MachineWorkbenchItem> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override void GetMachineDimensions(out uint width, out uint height) {
			width = 3;
			height = 3;
		}

		public override MachineWorkbenchRegistry GetRegistry() {
			return new(Type, static tick => new MachineRegistryDisplayAnimationState("TerraScience/Assets/Machines/MachineWorkbench/Example_tile", 1, 1, 0, 0));
		}
	}
}
