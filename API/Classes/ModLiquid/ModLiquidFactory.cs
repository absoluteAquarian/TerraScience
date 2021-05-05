using System.Collections.Generic;
using Terraria.ModLoader;
using TerraScience.Systems.TemperatureSystem;

namespace TerraScience.API.Classes.ModLiquid {
	public class ModLiquidFactory {
		public Dictionary<string, ModLiquid> Liquids { get; }

		public ModLiquid Create(string internalName, string displayName, DefaultTemperature defaultTemp) {
			var liquid = new ModLiquid(internalName, displayName, defaultTemp);
			Liquids.Add(liquid.InternalName, liquid);

			TerraScience.Instance.temperatureSystem.DefaultLiquidTemps.Add(liquid.InternalName, liquid.DefaultTemp);

			return liquid;
		}
	}
}