using SerousEnergyLib.TileData;
using SerousEnergyLib.Tiles;

namespace TerraScience.Content.Tiles.Networks.Items {
	public class BasicItemTransportTile : BaseNetworkTile, IItemTransportTile {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override NetworkType NetworkTypeToPlace => NetworkType.Items;

		public virtual double TransportSpeed => 3.5;

		public virtual float GetItemSize(int x, int y) => 3.85f * 2;
	}
}
