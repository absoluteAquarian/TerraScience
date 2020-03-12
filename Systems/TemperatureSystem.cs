using Terraria;
using TerraScience.Utilities;

namespace TerraScience.Systems {
	/// <summary>
	/// The system for world, player, tile, item and liquid temperature. Measured in Kelvin 
	/// </summary>
	internal class TemperatureSystem {
		public static float CurrentTemperature(Item item) => 0f;

		public static float DefaultWorldTemp = TemperatureUtils.CelsiusToKelvin(25f);

		public static float DefaultPlayerTemp = TemperatureUtils.CelsiusToKelvin(37f);
	}
}