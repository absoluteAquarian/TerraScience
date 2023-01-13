using SerousEnergyLib.API.Energy;
using SerousEnergyLib.TileData;
using SerousEnergyLib.Tiles;
using TerraScience.Content.Items.Networks.Power;

namespace TerraScience.Content.Tiles.Networks.Power {
	public class BasicTerraFluxWireTile : BaseNetworkEntryTile<BasicTerraFluxWireItem>, IPowerTransportTile {
		public override NetworkType NetworkTypeToPlace => NetworkType.Power;

		public virtual TerraFlux MaxCapacity => new TerraFlux(500);

		public virtual TerraFlux TransferRate => new TerraFlux(350d / 60d);
	}
}
