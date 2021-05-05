using TerraScience.Content.TileEntities.Energy;

namespace TerraScience.Content.UI.Energy{
	public abstract class PoweredMachineUI : MachineUI{
		public string GetFluxString(){
			var entity = UIEntity as PoweredMachineEntity;

			return $"Power: {UIDecimalFormat((float)entity.StoredFlux)} / {entity.FluxCap}TF";
		}
	}
}
