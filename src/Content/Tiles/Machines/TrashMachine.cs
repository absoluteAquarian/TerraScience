using SerousEnergyLib.API.CrossMod;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.Tiles;
using TerraScience.Content.Items.Machines;
using TerraScience.Content.MachineEntities;

namespace TerraScience.Content.Tiles.Machines {
	public class TrashMachine : BaseMachineTile<TrashMachineEntity, TrashMachineItem> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override void GetMachineDimensions(out uint width, out uint height) {
			width = 1;
			height = 1;
		}

		public override MachineWorkbenchRegistry GetRegistry() {
			return new(Type, static tick => new MachineRegistryDisplayAnimationState(TechMod.GetExamplePath<TrashMachine>("tile"), 1, 1, 0, 0));
		}

		public override bool PreRightClick(IMachine machine, int x, int y) {
			// No UI and no upgrades
			return true;
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			// Due to the machine being 1x1 tiles, ModTile.KillMultiTile() will not run
			if (!fail && !effectOnly)
				IMachineTile.DefaultKillMultitile(this, i, j, !noItem);
		}
	}
}
