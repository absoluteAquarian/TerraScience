using System;
using Terraria;
using TerraScience.Systems;
using TerraScience.Systems.Energy;

namespace TerraScience.Content.TileEntities.Energy.Generators{
	public abstract class GeneratorEntity : PoweredMachineEntity{
		public sealed override TerraFlux FluxUsage => new TerraFlux(0f);

		/// <summary>
		/// The max Terra Flux that is exported to the connected networks per tick. Exported flux is split across the networks.
		/// </summary>
		public abstract TerraFlux ExportRate{ get; }

		/// <summary>
		/// Attempts to send the <paramref name="flux"/> to a connected <seealso cref="WireNetwork"/>. The flux is split among all connected networks.
		/// </summary>
		/// <param name="flux"></param>
		public void ExportFlux(WireNetwork network){
			TerraFlux flux = new TerraFlux(Math.Min((float)StoredFlux, (float)ExportRate));

			//Ran out of TF
			if((float)flux <= 0)
				return;

			TerraFlux send = flux / NetworkCollection.GetWireNetworksConnectedTo(this).Count;
			network.ImportFlux(this, ref send);

			if((float)send > 0)
				ImportFlux(ref send);
		}

		public abstract TerraFlux GetPowerGeneration(int ticks);

		public sealed override void PostReaction(){
			foreach(var network in NetworkCollection.GetWireNetworksConnectedTo(this))
				ExportFlux(network);
		}

		internal override int[] GetInputSlots() => new int[0];

		internal override int[] GetOutputSlots() => new int[0];

		internal override bool CanInputItem(int slot, Item item) => false;
	}
}
