using System;

namespace TerraScience{
	public class TileNameNotFoundException : Exception{
		//Unused
		public TileNameNotFoundException(string internalName) : base($"The ModTile with the name \"{internalName}\" could not be found."){ }
	}

	public class InvalidFamilyException : Exception{
		public InvalidFamilyException(string elementName, ElementFamily family) : base($"The element \"{elementName}\" doesn't exist in the {Enum.GetName(typeof(ElementFamily), family)} family!"){ }
	}
}
