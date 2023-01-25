using SerousEnergyLib.API.Energy;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.API.Machines.Default;
using SerousEnergyLib.API.Machines.UI;
using SerousEnergyLib.Systems;
using System.IO;
using Terraria.ModLoader;
using TerraScience.Common.UI.Machines;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.MachineEntities {
	public class BatteryEntity : BasePowerStorageEntity, IReducedNetcodeMachine, IMachineUIAutoloading<BatteryEntity, BatteryUI> {
		public override int MachineTile => ModContent.TileType<Battery>();

		public override BaseMachineUI MachineUI => MachineUISingletons.GetInstance<BatteryEntity>();

		public override FluxStorage PowerStorage { get; } = new FluxStorage(new TerraFlux(30000d));

		public override void StorageUpdate() {
			Netcode.SendReducedData(this);
		}

		public override void NetSend(BinaryWriter writer) {
			base.NetSend(writer);
			ReducedNetSend(writer);
		}

		public override void NetReceive(BinaryReader reader) {
			base.NetReceive(reader);
			ReducedNetReceive(reader);
		}

		#region Implement IReducedNetcodeMachine
		public void ReducedNetSend(BinaryWriter writer) {
			writer.Write((double)PowerStorage.CurrentCapacity);
		}

		public void ReducedNetReceive(BinaryReader reader) {
			PowerStorage.CurrentCapacity = new TerraFlux(reader.ReadDouble());
		}
		#endregion
	}
}
