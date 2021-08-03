namespace TerraScience.API.Networking{
	public enum Message : byte{
		/// <summary>
		/// Sync any tile entity
		/// </summary>
		SyncTileEntity,
		/// <summary>
		/// Sync the creation of a network
		/// </summary>
		SyncNetworkCreation,
		/// <summary>
		/// Sync an update to a network, be it creating a new entry, removing an entry or deleting the network
		/// </summary>
		SyncNetworkUpdate,
		/// <summary>
		/// Sync extracting an item from a machine or chest
		/// </summary>
		SyncItemExtraction,
		/// <summary>
		/// Sync an item in an Item Network pipe
		/// <para>This message should be used sparingly</para>
		/// </summary>
		SyncItemInPipe
	}
}
