using TerraScience.Systems.Energy;

namespace TerraScience.Content.TileEntities.Energy.Generators{
	public abstract class GeneratorEntity : PoweredMachineEntity{
		/// <summary>
		/// The max Terra Flux that is exported to the connected networks per tick. Exported flux is split across the networks.
		/// </summary>
		public abstract TerraFlux ExportRate{ get; }

		/// <summary>
		/// Attempts to send the <paramref name="flux"/> to a connected <seealso cref="WireNetwork"/>. The flux is split among all connected networks.
		/// </summary>
		/// <param name="flux"></param>
		public void ExportFlux(TerraFlux flux){
			if(StoredFlux < flux)
				flux = StoredFlux;

			//Ran out of TF
			if((float)flux <= 0)
				return;

			var networks = NetworkCollection.GetNetworksConnectedTo(this);
			foreach(WireNetwork network in networks){
				TerraFlux send = flux / networks.Count;
				network.ExportFlux(this, send);
			}
		}

		public abstract TerraFlux GetPowerGeneration(int ticks);

		public sealed override void PostReaction(){
			if((float)ExportRate > 0)
				ExportFlux(ExportRate);
		}
	}
}
