using Terraria.DataStructures;

namespace TerraScience.API.Interfaces{
	public interface INetworkable{
		Point16 Position{ get; set; }
	}

	public interface INetworkable<T> : INetworkable where T : struct, INetworkable<T>{
		INetwork<T> ParentNetwork{ get; set; }
	}
}
