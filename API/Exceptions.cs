using System;

namespace TerraScience.API{
	[Serializable]
	public class TileNameNotFoundException : Exception{
		public TileNameNotFoundException(string internalName) : base($"The ModTile with the name \"{internalName}\" could not be found."){ }

		public TileNameNotFoundException() {
		}

		public TileNameNotFoundException(string message, Exception innerException) : base(message, innerException) {
		}

		protected TileNameNotFoundException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) {
			throw new NotImplementedException();
		}
	}

	[Serializable]
	public class InvalidFamilyException : Exception{
		public InvalidFamilyException(string elementName, ElementFamily family) : base($"The element \"{elementName}\" doesn't exist in the {Enum.GetName(typeof(ElementFamily), family)} family!"){ }

		public InvalidFamilyException() {
		}

		public InvalidFamilyException(string message) : base(message) {
		}

		public InvalidFamilyException(string message, Exception innerException) : base(message, innerException) {
		}

		protected InvalidFamilyException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) {
			throw new NotImplementedException();
		}
	}
}