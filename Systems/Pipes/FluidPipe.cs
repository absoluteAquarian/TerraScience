using System;
using Terraria.DataStructures;
using TerraScience.API.Interfaces;

namespace TerraScience.Systems.Pipes{
	public struct FluidPipe : INetworkable, INetworkable<FluidPipe>{
		public Point16 Position{ get; set; }

		public INetwork<FluidPipe> ParentNetwork{ get; set; }

		public FluidPipe(Point16 tilePos, INetwork network){
			Position = tilePos;
			ParentNetwork = network as FluidNetwork;

			if(ParentNetwork is null)
				throw new ArgumentException("Wires must be connected to a FluidNetwork");
		}

		public override int GetHashCode() => (Position.X << 16) | (int)Position.Y;

		public override bool Equals(object obj)
			=> obj is FluidPipe pipe && Position == pipe.Position;

		public static bool operator ==(FluidPipe first, FluidPipe second)
			=> first.Position == second.Position;

		public static bool operator !=(FluidPipe first, FluidPipe second)
			=> first.Position != second.Position;
	}
}
