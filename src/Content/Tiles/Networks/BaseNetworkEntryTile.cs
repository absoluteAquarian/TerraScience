using SerousEnergyLib.Tiles;

namespace TerraScience.Content.Tiles.Networks {
	/// <summary>
	/// A base implementation for a <see cref="BaseNetworkTile"/> tile
	/// </summary>
	public abstract class BaseNetworkEntryTile : BaseNetworkTile {
		public override string Texture => base.Texture.Replace("Content", "Assets");
	}
}
