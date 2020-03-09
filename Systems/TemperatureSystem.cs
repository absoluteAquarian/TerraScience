using System;
using Terraria;

namespace TerraScience.Systems {
	internal class TemperatureSystem {
		public static float CurrentTemperature(Item item) => 0f;

		public static float CelsiusToKelvin(float celsius) => celsius + 273.15f;
	}
}