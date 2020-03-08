using System;

namespace TerraScience{
	public class TileNameNotFoundException : Exception{
		//Unused
		public TileNameNotFoundException(string internalName) : base($"The ModTile with the name \"{internalName}\" could not be found."){ }
	}
}
