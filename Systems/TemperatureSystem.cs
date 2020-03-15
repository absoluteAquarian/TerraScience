using System;
using System.Collections.Generic;
using Terraria;
using TerraScience.API.Classes;

namespace TerraScience.Systems {
	/// <summary>
	/// The system for temperature. Stored in Kelvin
	/// </summary>
	public sealed class TemperatureSystem {
		public float DefaultPlayerTemperature { get; private set; }

		public Dictionary<string, float> DefaultLiquidTemps { get; } = new Dictionary<string, float>();

		internal void SetDefaultTemps() {
			DefaultPlayerTemperature = CelsiusToKelvin(37);

			DefaultLiquidTemps.Add("Water", CelsiusToKelvin(13));
			DefaultLiquidTemps.Add("Lava", CelsiusToKelvin(1300));
			DefaultLiquidTemps.Add("Honey", CelsiusToKelvin(18));
		}

		public static float CalculateItemTemp(Item item) {
			if (item == null)
				throw new ArgumentNullException("item returned null.");

			throw new NotImplementedException("Temperature has not been implimented yet.");
		}

		public static float CalculatePlayerTemp(Player player) {
			if (player == null)
				throw new ArgumentNullException("player returned null.");

			//for now, return normal player temperature. when temperature gets implimented, change.
			return DefaultPlayerTemperature;
		}

		public static Season CurrentSeason() {
			throw new NotImplementedException();
		}

		public static float CalculateTileTemp(Tile tile) {
			if (tile == null)
				throw new ArgumentNullException("tile returned null.");

			throw new NotImplementedException("Temperature has not been implimented yet.");
		}

		public static float CalculateLiquidTemp(ModLiquid liquid) {
			if (liquid == null)
				throw new ArgumentNullException("liquid returned null.");
			
			throw new NotImplementedException("Temperature and modded liquids have not been implimented yet.");
		}

		public static float CelsiusToKelvin(float celsius) => celsius + 273.15f;
	}

	public enum Season {
		Summer,
		Autumn,
		Winter,
		Spring
	}
}