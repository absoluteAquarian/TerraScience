using SerousEnergyLib.TileData;
using SerousEnergyLib.Tiles;

namespace TerraScience.Content.Tiles.Networks.Items {
	public class BasicItemTransportTile : BaseNetworkEntryTile, IItemTransportTile {
		public override NetworkType NetworkTypeToPlace => NetworkType.Items;

		public virtual double TransportSpeed => 3.5;

		public virtual float GetItemSize(int x, int y) => 3.85f * 2;
	}
}
