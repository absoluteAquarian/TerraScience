namespace TerraScience.API.Networking{
	/// <summary>
	/// An enum for use with <seealso cref="NetHandler"/>
	/// </summary>
	public enum NetworkMessage : byte{
		WireNetworkEntryCreation,
		WireNetworkEntryDeletion,
		WireNetworkRefreshConnections,

		ItemNetworkEntryCreation,
		ItemNetworkEntryDeletion,
		ItemNetworkRefreshConnections,
		
		FluidNetworkEntryCreation,
		FluidNetworkEntryDeletion,
		FluidNetworkRefreshConnections,

		WireNetworkUpdateFlux,

		ItemNetworkUpdateItemPump,
		ItemNetworkUpdateAllItems,

		FluidNetworkUpdateFluidPump
	}
}
