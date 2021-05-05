using Terraria.DataStructures;

namespace TerraScience.Systems.Energy{
	public struct TFWire{
		public readonly Point16 location;
		public readonly WireNetwork network;

		public TFWire(Point16 tilePos, WireNetwork network){
			location = tilePos;
			this.network = network;
		}

		public override int GetHashCode(){
			unchecked{
				int hash = (int)2166136261;
				hash = (hash * 16777619) ^ location.X;
				hash = (hash * 16777619) ^ location.Y;
				return hash;
			}
		}

		public override bool Equals(object obj)
			=> obj is TFWire wire && location == wire.location;

		public static bool operator ==(TFWire first, TFWire second)
			=> first.location == second.location;

		public static bool operator !=(TFWire first, TFWire second)
			=> first.location != second.location;
	}
}
