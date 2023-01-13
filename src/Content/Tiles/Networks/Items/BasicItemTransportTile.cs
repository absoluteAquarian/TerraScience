using SerousEnergyLib.TileData;
using SerousEnergyLib.Tiles;
using TerraScience.Content.Items.Networks.Items;

namespace TerraScience.Content.Tiles.Networks.Items {
	public class BasicItemTransportTile : BaseNetworkEntryTile<BasicItemTransportItem>, IItemTransportTile {
		public override NetworkType NetworkTypeToPlace => NetworkType.Items;

		public virtual double TransportSpeed => 1.5;

		public virtual float GetItemSize(int x, int y) => 3.85f * 2;
	}
}
