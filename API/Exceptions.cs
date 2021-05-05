using System;

namespace TerraScience.API {
	public class TileNameNotFoundException : Exception {
		public TileNameNotFoundException(string internalName)
			: base($"The ModTile with the name \"{internalName}\" could not be found."){ }
	}
}