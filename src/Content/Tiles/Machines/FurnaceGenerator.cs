using SerousEnergyLib.API.CrossMod;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.Tiles;
using Terraria.DataStructures;
using TerraScience.Content.Items.Machines;
using TerraScience.Content.MachineEntities;
using TerraScience.Content.Sounds;

namespace TerraScience.Content.Tiles.Machines {
	public class FurnaceGenerator : BaseMachineTile<FurnaceGeneratorEntity, FurnaceGeneratorItem> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override void GetMachineDimensions(out uint width, out uint height) {
			width = 3;
			height = 2;
		}

		public override MachineWorkbenchRegistry GetRegistry() {
			return new(Type, static tick => new MachineRegistryDisplayAnimationState(TechMod.GetExamplePath<FurnaceGenerator>("tile"), 1, 1, 0, 0));
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			// Kill the looping sound
			if (IMachine.TryFindMachineExact(new Point16(i, j), out FurnaceGeneratorEntity entity)) {
				ISoundEmittingMachine.StopSound(
					emitter: entity,
					RegisteredSounds.IDs.FurnaceGenerator.Running,
					ref entity.running,
					ref entity.servPlaying);
			}

			base.KillMultiTile(i, j, frameX, frameY);
		}
	}
}
