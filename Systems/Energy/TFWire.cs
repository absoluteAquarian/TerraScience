using System;
using Terraria.DataStructures;
using TerraScience.API.Interfaces;

namespace TerraScience.Systems.Energy{
	public struct TFWire : INetworkable<TFWire>{
		public INetwork<TFWire> ParentNetwork{ get; set; }

		public Point16 Position{ get; set; }

		public TFWire(Point16 tilePos, INetwork network){
			Position = tilePos;
			ParentNetwork = network as WireNetwork;

			if(ParentNetwork is null)
				throw new ArgumentException("Wires must be connected to a WireNetwork");
		}

		public override int GetHashCode() => (Position.X << 16) | (int)Position.Y;

		public override bool Equals(object obj)
			=> obj is TFWire wire && Position == wire.Position;

		public static bool operator ==(TFWire first, TFWire second)
			=> first.Position == second.Position;

		public static bool operator !=(TFWire first, TFWire second)
			=> first.Position != second.Position;

		public override string ToString() => $"Pos: (X: {Position.X}, Y: {Position.Y}), Parent: {ParentNetwork?.ID.ToString() ?? "n/a"}";
	}
}
