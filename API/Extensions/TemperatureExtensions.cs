using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerraScience.Systems;

namespace TerraScience.API.Extensions {
	internal static class TemperatureExtensions {
		public static float CurrentTemperature(this Item item) => TemperatureSystem.CalculateItemTemp(item);

		public static float CurrentTemperature(this Player player) => TemperatureSystem.CalculatePlayerTemp(player);

		public static float CurrentTemperature(this Tile tile) => TemperatureSystem.CalculateTileTemp(tile);
	}
}