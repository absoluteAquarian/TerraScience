using System;
using System.Collections.Generic;
using Terraria;
using TerraScience.API.Classes.ModLiquid;

namespace TerraScience.Systems.TemperatureSystem {
	/// <summary>
	/// The system for temperature. Stored in Kelvin
	/// </summary>
	public sealed class TemperatureSystem {
		public static DefaultTemperature DefaultPlayerTemperature { get; private set; } = new DefaultTemperature(CelsiusToKelvin(37));

		public Dictionary<string, DefaultTemperature> DefaultLiquidTemps { get; } = new Dictionary<string, DefaultTemperature>();

		internal void SetDefaultTemps() {
			// TODO add other seasons for water and honey
			DefaultLiquidTemps.Add("Water", new DefaultTemperature(CelsiusToKelvin(13)));
			DefaultLiquidTemps.Add("Lava", new DefaultTemperature(CelsiusToKelvin(1300)));
			DefaultLiquidTemps.Add("Honey", new DefaultTemperature(CelsiusToKelvin(18)));
		}

		public static float CalculateItemTemp(Item item) {
			if (item == null)
				throw new ArgumentNullException("item");

			throw new NotImplementedException("Temperature has not been implimented yet.");
		}

		public static float CalculatePlayerTemp(Player player) {
			if (player == null)
				throw new ArgumentNullException("player");

			throw new NotImplementedException("Temeprature has not been implimented yet.");
		}

		public static Season CurrentSeason() {
			throw new NotImplementedException();
		}

		public static float CalculateTileTemp(Tile tile) {
			if (tile == null)
				throw new ArgumentNullException("tile");

			throw new NotImplementedException("Temperature has not been implimented yet.");
		}

		public static float CalculateLiquidTemp(ModLiquid liquid) {
			if (liquid == null)
				throw new ArgumentNullException("liquid");

			throw new NotImplementedException("Temperature and modded liquids have not been implimented yet.");
		}

		public static float CelsiusToKelvin(float celsius) => celsius + 273.15f;
	}
}