using SerousEnergyLib.Tiles;
using Terraria.ModLoader;
using TerraScience.Content.Items.Networks;

namespace TerraScience.Content.Tiles.Networks {
	/// <summary>
	/// A base implementation for a <see cref="BaseNetworkTile"/> tile
	/// </summary>
	public abstract class BaseNetworkEntryTile : BaseNetworkTile {
		public override string Texture => base.Texture.Replace("Content", "Assets");
	}

	/// <inheritdoc cref="BaseNetworkEntryTile"/>
	public abstract class BaseNetworkEntryTile<T> : BaseNetworkEntryTile where T : BaseNetworkEntryPlacingItem {
		public sealed override int NetworkItem => ModContent.ItemType<T>();
	}
}
