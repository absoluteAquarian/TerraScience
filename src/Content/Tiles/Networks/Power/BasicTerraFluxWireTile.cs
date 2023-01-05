using SerousEnergyLib.API.Energy;
using SerousEnergyLib.TileData;
using SerousEnergyLib.Tiles;

namespace TerraScience.Content.Tiles.Networks.Power {
	public class BasicTerraFluxWireTile : BaseNetworkTile, IPowerTransportTile {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override NetworkType NetworkTypeToPlace => NetworkType.Power;

		public virtual TerraFlux MaxCapacity => new TerraFlux(500);

		public virtual TerraFlux TransferRate => new TerraFlux(350d / 60d);
	}
}
