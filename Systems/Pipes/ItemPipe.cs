using System;
using Terraria.DataStructures;
using TerraScience.API.Interfaces;

namespace TerraScience.Systems.Pipes{
	public struct ItemPipe : INetworkable, INetworkable<ItemPipe>{
		public Point16 Position{ get; set; }

		public INetwork<ItemPipe> ParentNetwork{ get; set; }

		public ItemPipe(Point16 tilePos, INetwork network){
			Position = tilePos;
			ParentNetwork = network as ItemNetwork;

			if(ParentNetwork is null)
				throw new ArgumentException("Wires must be connected to an ItemNetwork");
		}

		public override int GetHashCode() => (Position.X << 16) | (int)Position.Y;

		public override bool Equals(object obj)
			=> obj is ItemPipe pipe && Position == pipe.Position;

		public static bool operator ==(ItemPipe first, ItemPipe second)
			=> first.Position == second.Position;

		public static bool operator !=(ItemPipe first, ItemPipe second)
			=> first.Position != second.Position;
	}
}
