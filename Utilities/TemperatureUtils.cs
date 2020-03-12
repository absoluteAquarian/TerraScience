using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraScience.Utilities {
	public static class TemperatureUtils {
		public static float CelsiusToKelvin(float celsius) => celsius + 273.15f;

		public static float FahrenheitToCelsius(float fahrenheit) => (fahrenheit - 32) * 5 / 9;
	}
}